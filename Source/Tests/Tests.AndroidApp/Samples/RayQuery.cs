using System.Numerics;
using System.Runtime.InteropServices;
using Graphics.Core;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;
using StbImageSharp;
using Tests.AndroidApp.Helpers;
using Tests.Core.Helpers;
using AlphaMode = SharpGLTF.Schema2.AlphaMode;
using CameraController = Tests.AndroidApp.Controls.CameraController;
using GLTFTexture = SharpGLTF.Schema2.Texture;
using Texture = Graphics.Vulkan.Texture;

namespace Tests.AndroidApp.Samples;

public unsafe class RayQuery : BaseSample
{
    #region Structs
    [StructLayout(LayoutKind.Sequential)]
    private struct Vertex
    {
        public Vector3 Position;

        public Vector3 Normal;

        public Vector2 TexCoord;

        public Vector3 Color;

        public Vector4 Tangent;

        public uint NodeIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GeometryNode
    {
        public Matrix4x4 Transform;

        public bool AlphaMask;

        public float AlphaCutoff;

        public bool DoubleSided;

        public Vector4 BaseColorFactor;

        public uint BaseColorTextureIndex;

        public uint NormalTextureIndex;

        public uint RoughnessTextureIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Camera
    {
        public Vector3 Position;

        public Vector3 Forward;

        public Vector3 Right;

        public Vector3 Up;

        public float NearPlane;

        public float FarPlane;

        public float Fov;

        public Matrix4x4 ViewMatrix;

        public Matrix4x4 ProjectionMatrix;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Light
    {
        public Vector3 Position;

        public Vector4 AmbientColor;

        public Vector4 DiffuseColor;
    };

    [StructLayout(LayoutKind.Sequential)]
    private struct Param
    {
        public uint Width;

        public uint Height;
    }
    #endregion

    private readonly List<Texture> _textures = [];
    private readonly List<TextureView> _textureViews = [];
    private readonly List<Vertex> _vertices = [];
    private readonly List<uint> _indices = [];
    private readonly List<GeometryNode> _nodes = [];
    private readonly List<AccelStructTriangles> _triangles = [];
    private readonly List<Light> _lights = [];

    private DeviceBuffer _vertexBuffer = null!;
    private DeviceBuffer _indexBuffer = null!;
    private DeviceBuffer _nodeBuffer = null!;
    private DeviceBuffer _cameraBuffer = null!;
    private DeviceBuffer _lightBuffer = null!;
    private DeviceBuffer _paramBuffer = null!;
    private BottomLevelAS _bottomLevel = null!;
    private TopLevelAS _topLevel = null!;
    private ResourceLayout _resourceLayout0 = null!;
    private ResourceSet _resourceSet0 = null!;
    private ResourceLayout _resourceLayout1 = null!;
    private ResourceSet _resourceSet1 = null!;
    private ResourceLayout _resourceLayout2 = null!;
    private ResourceSet _resourceSet2 = null!;
    private Pipeline? _pipeline = null!;

    public override void Load(Swapchain swapchain, CameraController camera)
    {
        camera.Transform(Matrix4x4.CreateRotationY(90.0f.ToRadians()) * Matrix4x4.CreateTranslation(new Vector3(0.0f, 1.2f, 0.0f)));

        LoadGLTF("Assets/Models/Sponza/glTF", "Sponza.gltf");

        _lights.Add(new Light()
        {
            Position = new Vector3(-100.0f, 100.0f, 0.0f),
            AmbientColor = new Vector4(0.05f, 0.05f, 0.05f, 1.0f),
            DiffuseColor = new Vector4(1.0f, 0.9f, 0.7f, 1.0f)
        });

        _vertexBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<Vertex>(_vertices.Count, BufferUsage.VertexBuffer | BufferUsage.AccelerationStructure));
        App.Device.UpdateBuffer(_vertexBuffer, [.. _vertices]);

        _indexBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<uint>(_indices.Count, BufferUsage.IndexBuffer | BufferUsage.AccelerationStructure));
        App.Device.UpdateBuffer(_indexBuffer, [.. _indices]);

        _nodeBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<GeometryNode>(_nodes.Count, BufferUsage.StorageBuffer));
        App.Device.UpdateBuffer(_nodeBuffer, [.. _nodes]);

        _cameraBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<Camera>(1, BufferUsage.ConstantBuffer | BufferUsage.Dynamic));

        _lightBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<Light>(_lights.Count, BufferUsage.StorageBuffer));
        App.Device.UpdateBuffer(_lightBuffer, [.. _lights]);

        _paramBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<Param>(1, BufferUsage.ConstantBuffer | BufferUsage.Dynamic));

        for (int i = 0; i < _triangles.Count; i++)
        {
            _triangles[i].VertexBuffer = _vertexBuffer;
            _triangles[i].IndexBuffer = _indexBuffer;
        }

        BottomLevelASDescription bottomLevelASDescription = new()
        {
            Geometries = [.. _triangles]
        };

        _bottomLevel = App.Device.Factory.CreateBottomLevelAS(in bottomLevelASDescription);

        AccelStructInstance instance = new()
        {
            Transform4x4 = Matrix4x4.Identity,
            InstanceID = 0,
            InstanceMask = 0xFF,
            InstanceContributionToHitGroupIndex = 0,
            Options = AccelStructInstanceOptions.TriangleCullDisable,
            BottomLevel = _bottomLevel
        };

        TopLevelASDescription topLevelASDescription = new()
        {
            Instances = [instance],
            Options = AccelStructBuildOptions.PreferFastTrace
        };

        _topLevel = App.Device.Factory.CreateTopLevelAS(in topLevelASDescription);

        ElementDescription[] elements0 =
        [
            new ElementDescription("as", ResourceKind.AccelerationStructure, ShaderStages.Pixel),
            new ElementDescription("geometryNodes", ResourceKind.StorageBuffer, ShaderStages.Vertex | ShaderStages.Pixel),
            new ElementDescription("camera", ResourceKind.ConstantBuffer, ShaderStages.Vertex | ShaderStages.Pixel),
            new ElementDescription("lights", ResourceKind.StorageBuffer, ShaderStages.Vertex | ShaderStages.Pixel),
            new ElementDescription("param", ResourceKind.ConstantBuffer, ShaderStages.Pixel)
        ];

        _resourceLayout0 = App.Device.Factory.CreateResourceLayout(new ResourceLayoutDescription(elements0));
        _resourceSet0 = App.Device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout0,
                                                                                        _topLevel,
                                                                                        _nodeBuffer,
                                                                                        _cameraBuffer,
                                                                                        _lightBuffer,
                                                                                        _paramBuffer));

        ElementDescription[] elements1 =
        [
            new ElementDescription("textureArray", ResourceKind.SampledImage, ShaderStages.Pixel)
        ];

        _resourceLayout1 = App.Device.Factory.CreateResourceLayout(ResourceLayoutDescription.Bindless((uint)_textureViews.Count, elements1));
        _resourceSet1 = App.Device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout1));
        _resourceSet1.UpdateBindless([.. _textureViews]);

        ElementDescription[] elements2 =
        [
            new ElementDescription("samplerArray", ResourceKind.Sampler, ShaderStages.Pixel)
        ];

        _resourceLayout2 = App.Device.Factory.CreateResourceLayout(ResourceLayoutDescription.Bindless(2, elements2));
        _resourceSet2 = App.Device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout2));
        _resourceSet2.UpdateBindless([App.Device.Aniso4xSampler, App.Device.LinearSampler]);

        VertexElementDescription positionElement = new(nameof(Vertex.Position), VertexElementFormat.Float3);
        VertexElementDescription normalElement = new(nameof(Vertex.Normal), VertexElementFormat.Float3);
        VertexElementDescription texCoordElement = new(nameof(Vertex.TexCoord), VertexElementFormat.Float2);
        VertexElementDescription colorElement = new(nameof(Vertex.Color), VertexElementFormat.Float3);
        VertexElementDescription tangentElement = new(nameof(Vertex.Tangent), VertexElementFormat.Float4);
        VertexElementDescription nodeIndexElement = new(nameof(Vertex.NodeIndex), VertexElementFormat.UInt1);

        VertexLayoutDescription vertexLayout = new(positionElement, normalElement, texCoordElement, colorElement, tangentElement, nodeIndexElement);

        using Shader vs = App.Device.Factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, [.. new FileReader("Assets/Shaders").ReadFile("RayQuery.vs.hlsl.spv")], "main"));
        using Shader ps = App.Device.Factory.CreateShader(new ShaderDescription(ShaderStages.Pixel, [.. new FileReader("Assets/Shaders").ReadFile("RayQuery.ps.hlsl.spv")], "main"));

        GraphicsPipelineDescription pipelineDescription = new()
        {
            BlendState = BlendStateDescription.SingleAlphaBlend,
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            RasterizerState = RasterizerStateDescription.Default,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = [_resourceLayout0, _resourceLayout1, _resourceLayout2],
            Shaders = new GraphicsShaderDescription([vertexLayout], [vs, ps]),
            Outputs = swapchain.Framebuffer.OutputDescription
        };

        _pipeline = App.Device.Factory.CreateGraphicsPipeline(pipelineDescription);
    }

    public override void Update(Swapchain swapchain, float width, float height, CameraController camera, float deltaTime, float totalTime)
    {
        base.Update(swapchain, width, height, camera, deltaTime, totalTime);

        Camera cameraBuffer = new()
        {
            Position = camera.Position,
            Forward = camera.Forward,
            Right = camera.Right,
            Up = camera.Up,
            NearPlane = camera.NearPlane,
            FarPlane = camera.FarPlane,
            Fov = camera.Fov.ToRadians(),
            ViewMatrix = camera.GetView(),
            ProjectionMatrix = camera.GetProjection(width, height)
        };

        App.Device.UpdateBuffer(_cameraBuffer, in cameraBuffer);

        Param param = new()
        {
            Width = (uint)width,
            Height = (uint)height
        };

        App.Device.UpdateBuffer(_paramBuffer, in param);
    }

    public override void Render(CommandList commandList, Swapchain swapchain, float deltaTime, float totalTime)
    {
        base.Render(commandList, swapchain, deltaTime, totalTime);

        commandList.SetFramebuffer(swapchain.Framebuffer);
        commandList.ClearColorTarget(0, RgbaFloat.Black);
        commandList.ClearDepthStencil(1.0f);

        commandList.SetVertexBuffer(0, _vertexBuffer);
        commandList.SetIndexBuffer(_indexBuffer, IndexFormat.U32);
        commandList.SetPipeline(_pipeline!);
        commandList.SetResourceSet(0, _resourceSet0);
        commandList.SetResourceSet(1, _resourceSet1);
        commandList.SetResourceSet(2, _resourceSet2);

        commandList.DrawIndexed((uint)_indices.Count, 1, 0, 0, 0);
    }

    public override void Unload()
    {
    }

    private void LoadGLTF(string path, string name)
    {
        ModelRoot root = ModelRoot.Load(name, ReadContext.Create(new FileReader(path).ReadFile));

        #region Load Textures
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
        #endregion

        #region Load Meshes
        foreach (Node node in root.LogicalNodes)
        {
            LoadNode(node);
        }
        #endregion

        AddBackgroundTask(LoadTextures, root, _textures);
    }

    private void LoadNode(Node node)
    {
        foreach (Node children in node.VisualChildren)
        {
            LoadNode(children);
        }

        if (node.Mesh == null)
        {
            return;
        }

        foreach (MeshPrimitive primitive in node.Mesh.Primitives)
        {
            List<Vertex> vertices = [];
            List<uint> indices = [];

            uint vertexCount = 0;

            IList<Vector3>? positionBuffer = null;
            IList<Vector3>? normalBuffer = null;
            IList<Vector2>? texCoordBuffer = null;
            IList<Vector3>? colorBuffer = null;
            IList<Vector4>? tangentBuffer = null;

            if (primitive.VertexAccessors.TryGetValue("POSITION", out Accessor? positionAccessor))
            {
                vertexCount = (uint)positionAccessor.Count;

                positionBuffer = positionAccessor.AsVector3Array();
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

            if (vertexCount == 0)
            {
                continue;
            }

            for (uint i = 0; i < vertexCount; i++)
            {
                Vector3 position = positionBuffer != null ? positionBuffer[(int)i] : Vector3.Zero;
                Vector3 normal = normalBuffer != null ? normalBuffer[(int)i] : Vector3.Zero;
                Vector2 texCoord = texCoordBuffer != null ? texCoordBuffer[(int)i] : Vector2.Zero;
                Vector3 color = colorBuffer != null ? colorBuffer[(int)i] : Vector3.One;
                Vector4 tangent = tangentBuffer != null ? tangentBuffer[(int)i] : Vector4.Zero;

                vertices.Add(new Vertex()
                {
                    Position = position,
                    Normal = normal,
                    TexCoord = texCoord,
                    Color = color,
                    Tangent = tangent,
                    NodeIndex = (uint)_nodes.Count
                });
            }

            if (primitive.IndexAccessor != null)
            {
                indices.AddRange(primitive.IndexAccessor.AsIndicesArray());
            }

            GeometryNode geometryNode = new()
            {
                Transform = node.WorldMatrix,
                AlphaMask = primitive.Material.Alpha == AlphaMode.MASK,
                AlphaCutoff = primitive.Material.AlphaCutoff,
                DoubleSided = primitive.Material.DoubleSided
            };

            if (primitive.Material.FindChannel(KnownChannel.BaseColor.ToString()) is MaterialChannel baseColorChannel)
            {
                geometryNode.BaseColorFactor = baseColorChannel.Color;

                if (baseColorChannel.Texture != null)
                {
                    geometryNode.BaseColorTextureIndex = (uint)baseColorChannel.Texture.LogicalIndex;
                }
            }

            if (primitive.Material.FindChannel(KnownChannel.Normal.ToString()) is MaterialChannel normalChannel)
            {
                if (normalChannel.Texture != null)
                {
                    geometryNode.NormalTextureIndex = (uint)normalChannel.Texture.LogicalIndex;
                }
            }

            if (primitive.Material.FindChannel(KnownChannel.MetallicRoughness.ToString()) is MaterialChannel roughnessChannel)
            {
                if (roughnessChannel.Texture != null)
                {
                    geometryNode.RoughnessTextureIndex = (uint)roughnessChannel.Texture.LogicalIndex;
                }
            }

            uint vertexOffset = (uint)_vertices.Count;
            uint indexOffset = (uint)_indices.Count;

            AccelStructTriangles triangles = new()
            {
                VertexFormat = PixelFormat.R32G32B32Float,
                VertexStrideInBytes = (uint)sizeof(Vertex),
                VertexCount = (uint)vertices.Count,
                VertexOffsetInBytes = 0,
                IndexFormat = IndexFormat.U32,
                IndexCount = (uint)indices.Count,
                IndexOffsetInBytes = sizeof(uint) * indexOffset,
                Transform = node.WorldMatrix
            };

            _vertices.AddRange(vertices);
            _indices.AddRange(indices.Select(index => index + vertexOffset));
            _nodes.Add(geometryNode);
            _triangles.Add(triangles);
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
