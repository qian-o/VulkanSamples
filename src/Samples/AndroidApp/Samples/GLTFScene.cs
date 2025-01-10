using System.Numerics;
using System.Runtime.InteropServices;
using Graphics.Core;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using SharpGLTF.Materials;
using SharpGLTF.Memory;
using SharpGLTF.Schema2;
using StbImageSharp;
using Tests.AndroidApp.Controls;
using Tests.AndroidApp.Helpers;
using Tests.Core.Helpers;
using GLTFMaterial = SharpGLTF.Schema2.Material;
using GLTFNode = SharpGLTF.Schema2.Node;
using GLTFTexture = SharpGLTF.Schema2.Texture;
using Texture = Graphics.Vulkan.Texture;
using TextureView = Graphics.Vulkan.TextureView;

namespace Tests.AndroidApp.Samples;

public class GLTFScene : BaseSample
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

        public Dictionary<int, Primitive[]> GroupByMaterial { get; private set; } = [];

        public void GroupPrimitivesByMaterial()
        {
            GroupByMaterial = Primitives.GroupBy(primitive => primitive.MaterialIndex).ToDictionary(group => group.Key, group => group.ToArray());
        }
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

    private readonly List<Texture> _textures = [];
    private readonly List<TextureView> _textureViews = [];
    private readonly List<Material> _materials = [];
    private readonly List<Node> _nodes = [];

    private DeviceBuffer _vertexBuffer = null!;
    private DeviceBuffer _indexBuffer = null!;
    private DeviceBuffer _cboBuffer = null!;
    private ResourceLayout _cboLayout = null!;
    private ResourceSet _cboSet = null!;
    private ResourceLayout _textureMapLayout = null!;
    private ResourceSet _textureMapSet = null!;
    private ResourceLayout _textureSamplerLayout = null!;
    private ResourceSet _textureSamplerSet = null!;
    private Pipeline[] _pipelines = null!;

    private CBO _cbo;

    public override void Load(Swapchain swapchain, CameraController camera)
    {
        camera.Transform(Matrix4x4.CreateRotationY(90.0f.ToRadians()) * Matrix4x4.CreateTranslation(new Vector3(0.0f, 1.2f, 0.0f)));

        ModelRoot root = ModelRoot.Load("Sponza.gltf", ReadContext.Create(new FileReader("Assets/Models/Sponza/glTF").ReadFile));

        using Shader vs = App.Device.Factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, [.. new FileReader("Assets/Shaders").ReadFile("GLTF.vs.hlsl.spv")], "main"));
        using Shader ps = App.Device.Factory.CreateShader(new ShaderDescription(ShaderStages.Pixel, [.. new FileReader("Assets/Shaders").ReadFile("GLTF.ps.hlsl.spv")], "main"));

        foreach (GLTFTexture gltfTexture in root.LogicalTextures)
        {
            using Stream stream = gltfTexture.PrimaryImage.Content.Open();

            if (ImageInfo.FromStream(stream) is not ImageInfo imageInfo)
            {
                return;
            }

            int width = imageInfo.Width;
            int height = imageInfo.Height;

            uint mipLevels = Math.Max(1, (uint)MathF.Log2(Math.Max(width, height))) + 1;

            TextureDescription description = TextureDescription.Texture2D((uint)width, (uint)height, mipLevels, PixelFormat.R8G8B8A8UNorm, TextureUsage.Sampled | TextureUsage.GenerateMipmaps);

            Texture texture = App.Device.Factory.CreateTexture(in description);
            texture.Name = gltfTexture.Name;

            TextureView textureView = App.Device.Factory.CreateTextureView(texture);
            textureView.Name = gltfTexture.Name;

            _textures.Add(texture);
            _textureViews.Add(textureView);
        }

        foreach (GLTFMaterial gltfMaterial in root.LogicalMaterials)
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

        foreach (GLTFNode gltfNode in root.LogicalNodes)
        {
            LoadNode(gltfNode, null, vertices, indices);
        }

        _vertexBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<Vertex>(vertices.Count, BufferUsage.VertexBuffer));
        App.Device.UpdateBuffer(_vertexBuffer, [.. vertices]);

        _indexBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<uint>(indices.Count, BufferUsage.IndexBuffer));
        App.Device.UpdateBuffer(_indexBuffer, [.. indices]);

        _cboBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<CBO>(1, BufferUsage.ConstantBuffer | BufferUsage.Dynamic));

        ResourceLayoutDescription cboLayoutDescription = new(new ElementDescription("cbo", ResourceKind.ConstantBuffer, ShaderStages.Vertex));
        ResourceLayoutDescription textureMapDescription = ResourceLayoutDescription.Bindless((uint)root.LogicalTextures.Count,
                                                                                             new ElementDescription("textureMap", ResourceKind.SampledImage, ShaderStages.Pixel));
        ResourceLayoutDescription textureSamplerDescription = ResourceLayoutDescription.Bindless(2,
                                                                                                 new ElementDescription("textureSampler", ResourceKind.Sampler, ShaderStages.Pixel));

        _cboLayout = App.Device.Factory.CreateResourceLayout(in cboLayoutDescription);
        _cboSet = App.Device.Factory.CreateResourceSet(new ResourceSetDescription(_cboLayout, _cboBuffer));

        _textureMapLayout = App.Device.Factory.CreateResourceLayout(in textureMapDescription);
        _textureMapSet = App.Device.Factory.CreateResourceSet(new ResourceSetDescription(_textureMapLayout));
        _textureMapSet.UpdateBindless([.. _textureViews]);

        _textureSamplerLayout = App.Device.Factory.CreateResourceLayout(in textureSamplerDescription);
        _textureSamplerSet = App.Device.Factory.CreateResourceSet(new ResourceSetDescription(_textureSamplerLayout));
        _textureSamplerSet.UpdateBindless(App.Device.Aniso4xSampler, App.Device.LinearSampler);

        VertexElementDescription positionDescription = new("Position", VertexElementFormat.Float3);
        VertexElementDescription normalDescription = new("Normal", VertexElementFormat.Float3);
        VertexElementDescription texCoordDescription = new("TexCoord", VertexElementFormat.Float2);
        VertexElementDescription colorDescription = new("Color", VertexElementFormat.Float3);
        VertexElementDescription tangentDescription = new("Tangent", VertexElementFormat.Float4);
        VertexElementDescription colorMapIndexDescription = new("ColorMapIndex", VertexElementFormat.Int1);
        VertexElementDescription normalMapIndexDescription = new("NormalMapIndex", VertexElementFormat.Int1);

        VertexLayoutDescription[] vertexLayoutDescriptions = [new VertexLayoutDescription(positionDescription,
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
                Shaders = new GraphicsShaderDescription(vertexLayoutDescriptions, [vs, ps], [new SpecializationConstant(0, alphaMask), new SpecializationConstant(1, alphaCutoff)]),
                Outputs = swapchain.OutputDescription
            };

            _pipelines[i] = App.Device.Factory.CreateGraphicsPipeline(ref pipelineDescription);
        }

        AddBackgroundTask(LoadTextures, root, _textures);
    }

    public override void Update(Swapchain swapchain, float width, float height, CameraController camera, float deltaTime, float totalTime)
    {
        base.Update(swapchain, width, height, camera, deltaTime, totalTime);

        _cbo = new()
        {
            Projection = camera.GetProjection(width, height),
            View = camera.GetView(),
            LightPos = Vector4.Transform(new Vector4(0.0f, 2.5f, 0.0f, 1.0f), Matrix4x4.CreateRotationX(MathF.Sin(totalTime))),
            ViewPos = new Vector4(camera.Position, 1.0f)
        };
    }

    public override void Render(CommandList commandList, Swapchain swapchain, float deltaTime, float totalTime)
    {
        base.Render(commandList, swapchain, deltaTime, totalTime);

        commandList.SetFramebuffer(swapchain.Framebuffer);
        commandList.ClearColorTarget(0, RgbaFloat.Black);
        commandList.ClearDepthStencil(1.0f);

        commandList.SetVertexBuffer(0, _vertexBuffer);
        commandList.SetIndexBuffer(_indexBuffer, IndexFormat.U32);

        foreach (Node node in _nodes)
        {
            DrawNode(commandList, node);
        }
    }

    public override void Unload()
    {
    }

    private void LoadNode(GLTFNode gltfNode, Node? parent, List<Vertex> vertices, List<uint> indices)
    {
        Node node = new()
        {
            Name = gltfNode.Name,
            LocalTransform = gltfNode.LocalTransform.Matrix,
            WorldTransform = gltfNode.WorldMatrix
        };

        foreach (GLTFNode children in gltfNode.VisualChildren)
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

            node.Mesh!.GroupPrimitivesByMaterial();
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

    private void DrawNode(CommandList commandList, Node node)
    {
        if (!node.IsVisible)
        {
            return;
        }

        if (node.Mesh != null)
        {
            _cbo.Model = node.WorldTransform;

            commandList.UpdateBuffer(_cboBuffer, ref _cbo);

            foreach (KeyValuePair<int, Primitive[]> pair in node.Mesh.GroupByMaterial)
            {
                commandList.SetPipeline(_pipelines![pair.Key]);
                commandList.SetResourceSet(0, _cboSet);
                commandList.SetResourceSet(1, _textureMapSet);
                commandList.SetResourceSet(2, _textureSamplerSet);

                foreach (Primitive primitive in pair.Value)
                {
                    commandList.DrawIndexed(primitive.IndexCount, 1, primitive.FirstIndex, 0, 0);
                }
            }
        }

        foreach (Node children in node.Children)
        {
            DrawNode(commandList, children);
        }
    }

    private void LoadTextures(object[] args)
    {
        ModelRoot root = (ModelRoot)args[0];
        List<Texture> textures = (List<Texture>)args[1];

        Parallel.For(0, textures.Count, i =>
        {
            using Stream stream = root.LogicalTextures[i].PrimaryImage.Content.Open();

            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            root.LogicalTextures[i].ClearImages();

            AddRenderTask(WriteTexture, textures[i], image);
        });
    }

    private void WriteTexture(CommandList commandList, object[] args)
    {
        Texture texture = (Texture)args[0];
        ImageResult image = (ImageResult)args[1];

        commandList.UpdateTexture(texture, image.Data, 0, 0, 0, (uint)image.Width, (uint)image.Height, 1, 0, 0);
        commandList.GenerateMipmaps(texture);
    }
}
