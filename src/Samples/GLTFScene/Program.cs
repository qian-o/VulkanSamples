﻿using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Core;
using SharpGLTF.Materials;
using SharpGLTF.Memory;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;
using Silk.NET.Maths;
using StbImageSharp;
using Vulkan;
using Vulkan.Descriptions;
using Vulkan.Helpers;
using Windowing;
using Windowing.Events;
using GMaterial = SharpGLTF.Schema2.Material;
using GNode = SharpGLTF.Schema2.Node;
using GTexture = SharpGLTF.Schema2.Texture;
using Texture = Vulkan.Texture;

namespace GLTFScene;

internal sealed unsafe class Program
{
    #region Structs
    [StructLayout(LayoutKind.Sequential)]
    private struct CBO
    {
        public Matrix4x4 Projection;

        public Matrix4x4 View;

        public Matrix4x4 Model;

        public Vector4 LightPos;

        public Vector4 ViewPos;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Vertex(Vector3 position, Vector3 normal, Vector2 texCoord, Vector3 color, Vector4 tangent, int colorMapIndex, int normalMapIndex)
    {
        public Vector3 Position = position;

        public Vector3 Normal = normal;

        public Vector2 TexCoord = texCoord;

        public Vector3 Color = color;

        public Vector4 Tangent = tangent;

        public int ColorMapIndex = colorMapIndex;

        public int NormalMapIndex = normalMapIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Primitive(uint firstIndex, uint indexCount, int materialIndex)
    {
        public uint FirstIndex = firstIndex;

        public uint IndexCount = indexCount;

        public int MaterialIndex = materialIndex;
    }
    #endregion

    #region Classes
    private sealed class Mesh
    {
        public List<Primitive> Primitives { get; } = [];
    }

    private sealed class Node
    {
        public string Name { get; set; } = string.Empty;

        public Node? Parent { get; set; }

        public List<Node> Children { get; } = [];

        public Mesh? Mesh { get; set; }

        public Matrix4x4 LocalTransform { get; set; } = Matrix4x4.Identity;

        public Matrix4x4 WorldTransform { get; set; } = Matrix4x4.Identity;

        public bool IsVisible { get; set; } = true;
    }

    private sealed class Material
    {
        public Vector4 BaseColorFactor { get; set; } = Vector4.One;

        public uint BaseColorTextureIndex { get; set; }

        public uint NormalTextureIndex { get; set; }

        public string AlphaMode { get; set; } = "OPAQUE";

        public float AlphaCutoff { get; set; } = 0.5f;

        public bool DoubleSided { get; set; }
    }
    #endregion

    private static SdlWindow mainWindow = null!;
    private static Context context = null!;
    private static GraphicsDevice device = null!;
    private static Swapchain swapchain = null!;

    private static readonly List<Texture> _textures = [];
    private static readonly List<TextureView> _textureViews = [];
    private static readonly List<Material> _materials = [];
    private static readonly List<Node> _nodes = [];

    private static DeviceBuffer _vertexBuffer = null!;
    private static DeviceBuffer _indexBuffer = null!;
    private static DeviceBuffer _cboBuffer = null!;
    private static ResourceLayout _cboLayout = null!;
    private static ResourceSet _cboSet = null!;
    private static ResourceLayout _textureMapLayout = null!;
    private static ResourceSet _textureMapSet = null!;
    private static ResourceLayout _textureSamplerLayout = null!;
    private static ResourceSet _textureSamplerSet = null!;
    private static Shader[] _shaders = null!;
    private static VertexLayoutDescription[] _vertexLayoutDescriptions = null!;
    private static Pipeline[] _pipelines = null!;
    private static CommandList _commandList = null!;

    private static CBO _cbo;

    private static void Main(string[] _)
    {
        mainWindow = new()
        {
            Title = "GLTFScene",
            MinimumSize = new(100, 100)
        };

        mainWindow.Loaded += Loaded;
        mainWindow.Unloaded += Unloaded;
        mainWindow.SizeChanged += SizeChanged;
        mainWindow.Update += Update;
        mainWindow.Render += Render;

        mainWindow.Show();

        WindowManager.Loop();
    }

    private static void Loaded(object? sender, EventArgs e)
    {
        context = new();
        device = context.CreateGraphicsDevice(context.GetBestPhysicalDevice());
        swapchain = device.Factory.CreateSwapchain(new SwapchainDescription(mainWindow.Surface!, device.GetBestDepthFormat()));

        string hlsl = File.ReadAllText("Assets/Shaders/GLTF.hlsl");

        ModelRoot root = ModelRoot.Load("Assets/Models/Sponza/glTF/Sponza.gltf", new ReadSettings() { Validation = ValidationMode.Skip });

        using CommandList commandList = device.Factory.CreateGraphicsCommandList();

        commandList.Begin();
        foreach (GTexture gltfTexture in root.LogicalTextures)
        {
            using Stream stream = gltfTexture.PrimaryImage.Content.Open();

            if (ImageInfo.FromStream(stream) is not ImageInfo imageInfo)
            {
                continue;
            }

            int width = imageInfo.Width;
            int height = imageInfo.Height;

            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            uint mipLevels = Math.Max(1, (uint)MathF.Log2(Math.Max(width, height))) + 1;

            TextureDescription description = TextureDescription.Texture2D((uint)width,
                                                                          (uint)height,
                                                                          mipLevels,
                                                                          PixelFormat.R8G8B8A8UNorm,
                                                                          TextureUsage.Sampled | TextureUsage.GenerateMipmaps);

            Texture texture = device.Factory.CreateTexture(in description);
            texture.Name = gltfTexture.Name;

            TextureView textureView = device.Factory.CreateTextureView(texture);
            textureView.Name = gltfTexture.Name;

            commandList.UpdateTexture(texture, image.Data, 0, 0, 0, (uint)width, (uint)height, 1, 0, 0);
            commandList.GenerateMipmaps(texture);

            _textures.Add(texture);
            _textureViews.Add(textureView);

            gltfTexture.ClearImages();
        }
        commandList.End();

        device.SubmitCommands(commandList);

        foreach (GMaterial gltfMaterial in root.LogicalMaterials)
        {
            Material material = new();

            if (gltfMaterial.FindChannel(KnownChannel.BaseColor.ToString()) is MaterialChannel baseColor)
            {
                material.BaseColorFactor = baseColor.Color;

                if (baseColor.Texture != null)
                {
                    material.BaseColorTextureIndex = (uint)baseColor.Texture.LogicalIndex;
                }
            }

            if (gltfMaterial.FindChannel(KnownChannel.Normal.ToString()) is MaterialChannel normal)
            {
                if (normal.Texture != null)
                {
                    material.NormalTextureIndex = (uint)normal.Texture.LogicalIndex;
                }
            }

            material.AlphaMode = gltfMaterial.Alpha.ToString();
            material.AlphaCutoff = gltfMaterial.AlphaCutoff;
            material.DoubleSided = gltfMaterial.DoubleSided;

            _materials.Add(material);
        }

        List<Vertex> vertices = [];
        List<uint> indices = [];

        foreach (GNode gltfNode in root.LogicalNodes)
        {
            LoadNode(gltfNode, null, vertices, indices);
        }

        _vertexBuffer = device.Factory.CreateBuffer(BufferDescription.Buffer<Vertex>(vertices.Count, BufferUsage.VertexBuffer));
        device.UpdateBuffer(_vertexBuffer, [.. vertices]);

        _indexBuffer = device.Factory.CreateBuffer(BufferDescription.Buffer<uint>(indices.Count, BufferUsage.IndexBuffer));
        device.UpdateBuffer(_indexBuffer, [.. indices]);

        _cboBuffer = device.Factory.CreateBuffer(BufferDescription.Buffer<CBO>(1, BufferUsage.ConstantBuffer | BufferUsage.Dynamic));

        ResourceLayoutDescription cboLayoutDescription = new(new ElementDescription("cbo", ResourceKind.ConstantBuffer, ShaderStages.Vertex));
        ResourceLayoutDescription textureMapDescription = ResourceLayoutDescription.Bindless((uint)_textureViews.Count,
                                                                                             new ElementDescription("textureMap", ResourceKind.SampledImage, ShaderStages.Pixel));
        ResourceLayoutDescription textureSamplerDescription = ResourceLayoutDescription.Bindless(2,
                                                                                                 new ElementDescription("textureSampler", ResourceKind.Sampler, ShaderStages.Pixel));

        _cboLayout = device.Factory.CreateResourceLayout(in cboLayoutDescription);
        _cboSet = device.Factory.CreateResourceSet(new ResourceSetDescription(_cboLayout, _cboBuffer));

        _textureMapLayout = device.Factory.CreateResourceLayout(in textureMapDescription);
        _textureMapSet = device.Factory.CreateResourceSet(new ResourceSetDescription(_textureMapLayout));
        _textureMapSet.UpdateBindless([.. _textureViews]);

        _textureSamplerLayout = device.Factory.CreateResourceLayout(in textureSamplerDescription);
        _textureSamplerSet = device.Factory.CreateResourceSet(new ResourceSetDescription(_textureSamplerLayout));
        _textureSamplerSet.UpdateBindless(device.Aniso4xSampler, device.LinearSampler);

        VertexElementDescription positionDescription = new("Position", VertexElementFormat.Float3);
        VertexElementDescription normalDescription = new("Normal", VertexElementFormat.Float3);
        VertexElementDescription texCoordDescription = new("TexCoord", VertexElementFormat.Float2);
        VertexElementDescription colorDescription = new("Color", VertexElementFormat.Float3);
        VertexElementDescription tangentDescription = new("Tangent", VertexElementFormat.Float4);
        VertexElementDescription colorMapIndexDescription = new("ColorMapIndex", VertexElementFormat.Int1);
        VertexElementDescription normalMapIndexDescription = new("NormalMapIndex", VertexElementFormat.Int1);

        _shaders = device.Factory.CreateShaderByHLSL(new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(hlsl), "mainVS"),
                                                      new ShaderDescription(ShaderStages.Pixel, Encoding.UTF8.GetBytes(hlsl), "mainPS"));

        _vertexLayoutDescriptions = [new VertexLayoutDescription(positionDescription,
                                                                 normalDescription,
                                                                 texCoordDescription,
                                                                 colorDescription,
                                                                 tangentDescription,
                                                                 colorMapIndexDescription,
                                                                 normalMapIndexDescription)];

        _pipelines = new Pipeline[_materials.Count];
        for (int i = 0; i < _materials.Count; i++)
        {
            bool alphaMask = _materials[i].AlphaMode == "MASK";
            float alphaCutoff = _materials[i].AlphaCutoff;

            GraphicsPipelineDescription pipelineDescription = new()
            {
                BlendState = BlendStateDescription.SingleAlphaBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = _materials[i].DoubleSided ? RasterizerStateDescription.CullNone : RasterizerStateDescription.Default,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ResourceLayouts = [_cboLayout, _textureMapLayout, _textureSamplerLayout],
                Shaders = new GraphicsShaderDescription(_vertexLayoutDescriptions, _shaders, [new SpecializationConstant(0, alphaMask), new SpecializationConstant(1, alphaCutoff)]),
                Outputs = swapchain.OutputDescription
            };

            _pipelines[i] = device.Factory.CreateGraphicsPipeline(ref pipelineDescription);
        }

        _commandList = device.Factory.CreateGraphicsCommandList();
    }

    private static void Unloaded(object? sender, EventArgs e)
    {
        _commandList.Dispose();

        foreach (Pipeline pipeline in _pipelines)
        {
            pipeline.Dispose();
        }

        foreach (Shader shader in _shaders)
        {
            shader.Dispose();
        }

        _textureSamplerSet.Dispose();
        _textureSamplerLayout.Dispose();

        _textureMapSet.Dispose();
        _textureMapLayout.Dispose();

        _cboSet.Dispose();
        _cboLayout.Dispose();

        _cboBuffer.Dispose();
        _indexBuffer.Dispose();
        _vertexBuffer.Dispose();

        foreach (TextureView textureView in _textureViews)
        {
            textureView.Dispose();
        }

        foreach (Texture texture in _textures)
        {
            texture.Dispose();
        }

        swapchain.Dispose();
        device.Dispose();
        context.Dispose();
    }

    private static void SizeChanged(object? sender, ValueEventArgs<Vector2D<int>> e)
    {
        swapchain.Resize();
    }

    private static void Update(object? sender, TimeEventArgs e)
    {
        _cbo = new()
        {
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4, (float)mainWindow.Size.X / mainWindow.Size.Y, 0.1f, 1000.0f),
            View = Matrix4x4.CreateLookAt(new Vector3(7.8f, 2.1f, 0.0f), Vector3.Zero, Vector3.UnitY),
            LightPos = Vector4.Transform(new Vector4(0.0f, 2.5f, 0.0f, 1.0f), Matrix4x4.CreateRotationX(MathF.Sin((float)e.TotalTime))),
            ViewPos = new Vector4(new Vector3(7.8f, 2.1f, 0.0f), 1.0f)
        };
    }

    private static void Render(object? sender, TimeEventArgs e)
    {
        _commandList.Begin();

        _commandList.SetFramebuffer(swapchain.Framebuffer);
        _commandList.ClearColorTarget(0, RgbaFloat.Black);
        _commandList.ClearDepthStencil(1.0f);

        _commandList.SetVertexBuffer(0, _vertexBuffer);
        _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.U32);

        foreach (Node node in _nodes)
        {
            DrawNode(_commandList, node);
        }

        _commandList.End();

        device.SubmitCommandsAndSwapBuffers(_commandList, swapchain);
    }

    private static void LoadNode(GNode gltfNode, Node? parent, List<Vertex> vertices, List<uint> indices)
    {
        Node node = new()
        {
            Name = gltfNode.Name,
            LocalTransform = gltfNode.LocalTransform.Matrix,
            WorldTransform = gltfNode.WorldMatrix
        };

        foreach (GNode children in gltfNode.VisualChildren)
        {
            LoadNode(children, node, vertices, indices);
        }

        if (gltfNode.Mesh != null)
        {
            foreach (MeshPrimitive primitive in gltfNode.Mesh.Primitives)
            {
                uint firsetIndex = (uint)indices.Count;
                uint vertexOffset = (uint)vertices.Count;
                int indexCount = 0;

                // Vertices
                {
                    IList<Vector3>? positionBuffer = null;
                    IList<Vector3>? normalBuffer = null;
                    IList<Vector2>? texCoordBuffer = null;
                    IList<Vector3>? colorBuffer = null;
                    IList<Vector4>? tangentBuffer = null;
                    uint vertexCount = 0;

                    if (primitive.VertexAccessors.TryGetValue("POSITION", out Accessor? positionAccessor))
                    {
                        positionBuffer = positionAccessor.AsVector3Array();
                        vertexCount = (uint)positionAccessor.Count;
                    }

                    if (primitive.VertexAccessors.TryGetValue("NORMAL", out Accessor? normalAccessor))
                    {
                        normalBuffer = normalAccessor.AsVector3Array();
                    }

                    if (primitive.VertexAccessors.TryGetValue("TEXCOORD_0", out Accessor? texCoordAccessor))
                    {
                        texCoordBuffer = texCoordAccessor.AsVector2Array();
                    }

                    if (primitive.VertexAccessors.TryGetValue("COLOR_0", out Accessor? colorAccessor))
                    {
                        colorBuffer = colorAccessor.AsVector3Array();
                    }

                    if (primitive.VertexAccessors.TryGetValue("TANGENT", out Accessor? tangentAccessor))
                    {
                        tangentBuffer = tangentAccessor.AsVector4Array();
                    }

                    for (uint i = 0; i < vertexCount; i++)
                    {
                        Vector3 position = positionBuffer != null ? positionBuffer[(int)i] : Vector3.Zero;
                        Vector3 normal = normalBuffer != null ? normalBuffer[(int)i] : Vector3.Zero;
                        Vector2 texCoord = texCoordBuffer != null ? texCoordBuffer[(int)i] : Vector2.Zero;
                        Vector3 color = colorBuffer != null ? colorBuffer[(int)i] : Vector3.One;
                        Vector4 tangent = tangentBuffer != null ? tangentBuffer[(int)i] : Vector4.Zero;
                        int colorMapIndex = (int)_materials[primitive.Material.LogicalIndex].BaseColorTextureIndex;
                        int normalMapIndex = (int)_materials[primitive.Material.LogicalIndex].NormalTextureIndex;

                        vertices.Add(new Vertex(position,
                                                normal,
                                                texCoord,
                                                color,
                                                tangent,
                                                colorMapIndex,
                                                normalMapIndex));
                    }
                }

                // Indices
                {
                    if (primitive.IndexAccessor != null)
                    {
                        indexCount = primitive.IndexAccessor.Count;

                        IntegerArray indexBuffer = primitive.IndexAccessor.AsIndicesArray();

                        for (int i = 0; i < indexCount; i++)
                        {
                            indices.Add(indexBuffer[i] + vertexOffset);
                        }
                    }
                }

                node.Mesh ??= new();
                node.Mesh.Primitives.Add(new Primitive(firsetIndex, (uint)indexCount, primitive.Material.LogicalIndex));
            }
        }

        if (parent != null)
        {
            node.Parent = parent;
            parent.Children.Add(node);
        }
        else
        {
            _nodes.Add(node);
        }
    }

    private static void DrawNode(CommandList commandList, Node node)
    {
        if (!node.IsVisible)
        {
            return;
        }

        if (node.Mesh != null)
        {
            _cbo.Model = node.WorldTransform;

            commandList.UpdateBuffer(_cboBuffer, ref _cbo);

            foreach (Primitive primitive in node.Mesh.Primitives)
            {
                commandList.SetPipeline(_pipelines![primitive.MaterialIndex]);
                commandList.SetResourceSet(0, _cboSet);
                commandList.SetResourceSet(1, _textureMapSet);
                commandList.SetResourceSet(2, _textureSamplerSet);
                commandList.DrawIndexed(primitive.IndexCount, 1, primitive.FirstIndex, 0, 0);
            }
        }

        foreach (Node children in node.Children)
        {
            DrawNode(commandList, children);
        }
    }
}