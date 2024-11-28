using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Graphics.Core;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Graphics.Vulkan.ImGui;
using Graphics.Windowing.Events;
using Hexa.NET.ImGui;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;
using Silk.NET.Maths;
using StbImageSharp;
using Tests.Core;
using Tests.Core.Helpers;
using AlphaMode = SharpGLTF.Schema2.AlphaMode;
using GLTFTexture = SharpGLTF.Schema2.Texture;
using Texture = Graphics.Vulkan.Texture;

namespace Tests.RayQuery;

internal sealed unsafe class MainView : View
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

        public Matrix4x4 InvViewMatrix;

        public Matrix4x4 InvProjectionMatrix;
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

        public PixelOffsets PixelOffsets;

        public uint Samples;
    }

    [InlineArray(127)]
    private struct PixelOffsets
    {
        public Vector2 Element0;
    }
    #endregion

    private static readonly Vector2[] HaltonSequence =
    [
        new Vector2(5.000000e-01f, 6.666667e-01f),
        new Vector2(2.500000e-01f, 3.333333e-01f),
        new Vector2(7.500000e-01f, 2.222222e-01f),
        new Vector2(1.250000e-01f, 8.888889e-01f),
        new Vector2(6.250000e-01f, 5.555556e-01f),
        new Vector2(3.750000e-01f, 1.111111e-01f),
        new Vector2(8.750000e-01f, 7.777778e-01f),
        new Vector2(6.250000e-02f, 4.444444e-01f),
        new Vector2(5.625000e-01f, 7.407407e-02f),
        new Vector2(3.125000e-01f, 7.407407e-01f),
        new Vector2(8.125000e-01f, 4.074074e-01f),
        new Vector2(1.875000e-01f, 2.962963e-01f),
        new Vector2(6.875000e-01f, 9.629630e-01f),
        new Vector2(4.375000e-01f, 6.296296e-01f),
        new Vector2(9.375000e-01f, 1.851852e-01f),
        new Vector2(3.125000e-02f, 8.518519e-01f),
        new Vector2(5.312500e-01f, 5.185185e-01f),
        new Vector2(2.812500e-01f, 3.703704e-02f),
        new Vector2(7.812500e-01f, 7.037037e-01f),
        new Vector2(1.562500e-01f, 3.703704e-01f),
        new Vector2(6.562500e-01f, 2.592593e-01f),
        new Vector2(4.062500e-01f, 9.259259e-01f),
        new Vector2(9.062500e-01f, 5.925926e-01f),
        new Vector2(9.375000e-02f, 1.481481e-01f),
        new Vector2(5.937500e-01f, 8.148148e-01f),
        new Vector2(3.437500e-01f, 4.814815e-01f),
        new Vector2(8.437500e-01f, 2.469136e-02f),
        new Vector2(2.187500e-01f, 6.913580e-01f),
        new Vector2(7.187500e-01f, 3.580247e-01f),
        new Vector2(4.687500e-01f, 2.469136e-01f),
        new Vector2(9.687500e-01f, 9.135802e-01f),
        new Vector2(1.562500e-02f, 5.802469e-01f),
        new Vector2(5.156250e-01f, 1.358025e-01f),
        new Vector2(2.656250e-01f, 8.024691e-01f),
        new Vector2(7.656250e-01f, 4.691358e-01f),
        new Vector2(1.406250e-01f, 9.876543e-02f),
        new Vector2(6.406250e-01f, 7.654321e-01f),
        new Vector2(3.906250e-01f, 4.320988e-01f),
        new Vector2(8.906250e-01f, 3.209877e-01f),
        new Vector2(7.812500e-02f, 9.876543e-01f),
        new Vector2(5.781250e-01f, 6.543210e-01f),
        new Vector2(3.281250e-01f, 2.098765e-01f),
        new Vector2(8.281250e-01f, 8.765432e-01f),
        new Vector2(2.031250e-01f, 5.432099e-01f),
        new Vector2(7.031250e-01f, 6.172840e-02f),
        new Vector2(4.531250e-01f, 7.283951e-01f),
        new Vector2(9.531250e-01f, 3.950617e-01f),
        new Vector2(4.687500e-02f, 2.839506e-01f),
        new Vector2(5.468750e-01f, 9.506173e-01f),
        new Vector2(2.968750e-01f, 6.172840e-01f),
        new Vector2(7.968750e-01f, 1.728395e-01f),
        new Vector2(1.718750e-01f, 8.395062e-01f),
        new Vector2(6.718750e-01f, 5.061728e-01f),
        new Vector2(4.218750e-01f, 1.234568e-02f),
        new Vector2(9.218750e-01f, 6.790123e-01f),
        new Vector2(1.093750e-01f, 3.456790e-01f),
        new Vector2(6.093750e-01f, 2.345679e-01f),
        new Vector2(3.593750e-01f, 9.012346e-01f),
        new Vector2(8.593750e-01f, 5.679012e-01f),
        new Vector2(2.343750e-01f, 1.234568e-01f),
        new Vector2(7.343750e-01f, 7.901235e-01f),
        new Vector2(4.843750e-01f, 4.567901e-01f),
        new Vector2(9.843750e-01f, 8.641975e-02f),
        new Vector2(7.812500e-03f, 7.530864e-01f),
        new Vector2(5.078125e-01f, 4.197531e-01f),
        new Vector2(2.578125e-01f, 3.086420e-01f),
        new Vector2(7.578125e-01f, 9.753086e-01f),
        new Vector2(1.328125e-01f, 6.419753e-01f),
        new Vector2(6.328125e-01f, 1.975309e-01f),
        new Vector2(3.828125e-01f, 8.641975e-01f),
        new Vector2(8.828125e-01f, 5.308642e-01f),
        new Vector2(7.031250e-02f, 4.938272e-02f),
        new Vector2(5.703125e-01f, 7.160494e-01f),
        new Vector2(3.203125e-01f, 3.827160e-01f),
        new Vector2(8.203125e-01f, 2.716049e-01f),
        new Vector2(1.953125e-01f, 9.382716e-01f),
        new Vector2(6.953125e-01f, 6.049383e-01f),
        new Vector2(4.453125e-01f, 1.604938e-01f),
        new Vector2(9.453125e-01f, 8.271605e-01f),
        new Vector2(3.906250e-02f, 4.938272e-01f),
        new Vector2(5.390625e-01f, 8.230453e-03f),
        new Vector2(2.890625e-01f, 6.748971e-01f),
        new Vector2(7.890625e-01f, 3.415638e-01f),
        new Vector2(1.640625e-01f, 2.304527e-01f),
        new Vector2(6.640625e-01f, 8.971193e-01f),
        new Vector2(4.140625e-01f, 5.637860e-01f),
        new Vector2(9.140625e-01f, 1.193416e-01f),
        new Vector2(1.015625e-01f, 7.860082e-01f),
        new Vector2(6.015625e-01f, 4.526749e-01f),
        new Vector2(3.515625e-01f, 8.230453e-02f),
        new Vector2(8.515625e-01f, 7.489712e-01f),
        new Vector2(2.265625e-01f, 4.156379e-01f),
        new Vector2(7.265625e-01f, 3.045267e-01f),
        new Vector2(4.765625e-01f, 9.711934e-01f),
        new Vector2(9.765625e-01f, 6.378601e-01f),
        new Vector2(2.343750e-02f, 1.934156e-01f),
        new Vector2(5.234375e-01f, 8.600823e-01f),
        new Vector2(2.734375e-01f, 5.267490e-01f),
        new Vector2(7.734375e-01f, 4.526749e-02f),
        new Vector2(1.484375e-01f, 7.119342e-01f),
        new Vector2(6.484375e-01f, 3.786008e-01f),
        new Vector2(3.984375e-01f, 2.674897e-01f),
        new Vector2(8.984375e-01f, 9.341564e-01f),
        new Vector2(8.593750e-02f, 6.008230e-01f),
        new Vector2(5.859375e-01f, 1.563786e-01f),
        new Vector2(3.359375e-01f, 8.230453e-01f),
        new Vector2(8.359375e-01f, 4.897119e-01f),
        new Vector2(2.109375e-01f, 3.292181e-02f),
        new Vector2(7.109375e-01f, 6.995885e-01f),
        new Vector2(4.609375e-01f, 3.662551e-01f),
        new Vector2(9.609375e-01f, 2.551440e-01f),
        new Vector2(5.468750e-02f, 9.218107e-01f),
        new Vector2(5.546875e-01f, 5.884774e-01f),
        new Vector2(3.046875e-01f, 1.440329e-01f),
        new Vector2(8.046875e-01f, 8.106996e-01f),
        new Vector2(1.796875e-01f, 4.773663e-01f),
        new Vector2(6.796875e-01f, 1.069959e-01f),
        new Vector2(4.296875e-01f, 7.736626e-01f),
        new Vector2(9.296875e-01f, 4.403292e-01f),
        new Vector2(1.171875e-01f, 3.292181e-01f),
        new Vector2(6.171875e-01f, 9.958848e-01f),
        new Vector2(3.671875e-01f, 6.625514e-01f),
        new Vector2(8.671875e-01f, 2.181070e-01f),
        new Vector2(2.421875e-01f, 8.847737e-01f),
        new Vector2(7.421875e-01f, 5.514403e-01f),
        new Vector2(4.921875e-01f, 6.995885e-02f),
        new Vector2(9.921875e-01f, 7.366255e-01f),
    ];

    private readonly GraphicsDevice _device;
    private readonly ImGuiController _imGuiController;
    private readonly ViewController _viewController;
    private readonly CameraController _cameraController;

    private readonly List<Texture> _textures;
    private readonly List<TextureView> _textureViews;
    private readonly List<Vertex> _vertices;
    private readonly List<uint> _indices;
    private readonly List<GeometryNode> _nodes;
    private readonly List<AccelStructTriangles> _triangles;
    private readonly List<Light> _lights;

    private readonly DeviceBuffer _vertexBuffer;
    private readonly DeviceBuffer _indexBuffer;
    private readonly DeviceBuffer _nodeBuffer;
    private readonly DeviceBuffer _cameraBuffer;
    private readonly DeviceBuffer _lightBuffer;
    private readonly DeviceBuffer _paramBuffer;
    private readonly BottomLevelAS _bottomLevel;
    private readonly TopLevelAS _topLevel;
    private readonly ResourceLayout _resourceLayout0;
    private readonly ResourceSet _resourceSet0;
    private readonly ResourceLayout _resourceLayout1;
    private readonly ResourceSet _resourceSet1;
    private readonly ResourceLayout _resourceLayout2;
    private readonly ResourceSet _resourceSet2;
    private readonly VertexLayoutDescription _vertexLayout;
    private readonly Shader[] _shaders;
    private readonly CommandList _commandList;

    private FramebufferObject? _framebufferObject;
    private Pipeline? _pipeline;

    private Param _param;

    public MainView(GraphicsDevice device, ImGuiController imGuiController) : base("Ray Query")
    {
        _device = device;
        _imGuiController = imGuiController;
        _viewController = new ViewController(this);
        _cameraController = new CameraController(_viewController);
        _cameraController.Transform(Matrix4x4.CreateRotationY(90.0f.ToRadians()) * Matrix4x4.CreateTranslation(new Vector3(0.0f, 1.2f, 0.0f)));

        _textures = [];
        _textureViews = [];
        _vertices = [];
        _indices = [];
        _nodes = [];
        _triangles = [];
        _lights = [];

        LoadGLTF("Assets/Models/Sponza/glTF/Sponza.gltf");

        _lights.Add(new Light()
        {
            Position = new Vector3(-100.0f, 100.0f, 0.0f),
            AmbientColor = new Vector4(0.05f, 0.05f, 0.05f, 1.0f),
            DiffuseColor = new Vector4(1.0f, 0.9f, 0.7f, 1.0f)
        });

        _vertexBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<Vertex>(_vertices.Count, BufferUsage.VertexBuffer | BufferUsage.AccelerationStructure));
        _device.UpdateBuffer(_vertexBuffer, [.. _vertices]);

        _indexBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<uint>(_indices.Count, BufferUsage.IndexBuffer | BufferUsage.AccelerationStructure));
        _device.UpdateBuffer(_indexBuffer, [.. _indices]);

        _nodeBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<GeometryNode>(_nodes.Count, BufferUsage.StorageBuffer));
        _device.UpdateBuffer(_nodeBuffer, [.. _nodes]);

        _cameraBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<Camera>(1, BufferUsage.ConstantBuffer | BufferUsage.Dynamic));

        _lightBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<Light>(_lights.Count, BufferUsage.StorageBuffer));
        _device.UpdateBuffer(_lightBuffer, [.. _lights]);

        _paramBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<Param>(1, BufferUsage.ConstantBuffer | BufferUsage.Dynamic));

        for (int i = 0; i < _triangles.Count; i++)
        {
            _triangles[i].VertexBuffer = _vertexBuffer;
            _triangles[i].IndexBuffer = _indexBuffer;
        }

        BottomLevelASDescription bottomLevelASDescription = new()
        {
            Geometries = [.. _triangles]
        };

        _bottomLevel = device.Factory.CreateBottomLevelAS(in bottomLevelASDescription);

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

        _topLevel = device.Factory.CreateTopLevelAS(in topLevelASDescription);

        ElementDescription[] elements0 =
        [
            new ElementDescription("as", ResourceKind.AccelerationStructure, ShaderStages.Pixel),
            new ElementDescription("geometryNodes", ResourceKind.StorageBuffer, ShaderStages.Vertex | ShaderStages.Pixel),
            new ElementDescription("camera", ResourceKind.ConstantBuffer, ShaderStages.Vertex | ShaderStages.Pixel),
            new ElementDescription("lights", ResourceKind.StorageBuffer, ShaderStages.Vertex | ShaderStages.Pixel),
            new ElementDescription("param", ResourceKind.ConstantBuffer, ShaderStages.Pixel)
        ];

        _resourceLayout0 = device.Factory.CreateResourceLayout(new ResourceLayoutDescription(elements0));
        _resourceSet0 = device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout0,
                                                                                    _topLevel,
                                                                                    _nodeBuffer,
                                                                                    _cameraBuffer,
                                                                                    _lightBuffer,
                                                                                    _paramBuffer));

        ElementDescription[] elements1 =
        [
            new ElementDescription("textureArray", ResourceKind.SampledImage, ShaderStages.Pixel)
        ];

        _resourceLayout1 = device.Factory.CreateResourceLayout(ResourceLayoutDescription.Bindless((uint)_textureViews.Count, elements1));
        _resourceSet1 = device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout1));
        _resourceSet1.UpdateBindless([.. _textureViews]);

        ElementDescription[] elements2 =
        [
            new ElementDescription("samplerArray", ResourceKind.Sampler, ShaderStages.Pixel)
        ];

        _resourceLayout2 = device.Factory.CreateResourceLayout(ResourceLayoutDescription.Bindless(2, elements2));
        _resourceSet2 = device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout2));
        _resourceSet2.UpdateBindless([_device.Aniso4xSampler, _device.LinearSampler]);

        VertexElementDescription positionElement = new(nameof(Vertex.Position), VertexElementFormat.Float3);
        VertexElementDescription normalElement = new(nameof(Vertex.Normal), VertexElementFormat.Float3);
        VertexElementDescription texCoordElement = new(nameof(Vertex.TexCoord), VertexElementFormat.Float2);
        VertexElementDescription colorElement = new(nameof(Vertex.Color), VertexElementFormat.Float3);
        VertexElementDescription tangentElement = new(nameof(Vertex.Tangent), VertexElementFormat.Float4);
        VertexElementDescription nodeIndexElement = new(nameof(Vertex.NodeIndex), VertexElementFormat.UInt1);

        _vertexLayout = new(positionElement, normalElement, texCoordElement, colorElement, tangentElement, nodeIndexElement);

        string hlsl = File.ReadAllText("Assets/Shaders/rayQuery.hlsl");

        _shaders = _device.Factory.CreateShaderByHLSL(new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(hlsl), "mainVS"),
                                                      new ShaderDescription(ShaderStages.Pixel, Encoding.UTF8.GetBytes(hlsl), "mainPS"));

        _commandList = _device.Factory.CreateGraphicsCommandList();

        _param = new Param()
        {
            Width = Width,
            Height = Height,
            Samples = 4
        };

        for (int i = 0; i < HaltonSequence.Length; i++)
        {
            _param.PixelOffsets[i] = HaltonSequence[i];
        }
    }

    protected override void OnUpdate(TimeEventArgs e)
    {
        _viewController.Update();
        _cameraController.Update((float)e.DeltaTime);

        Camera camera = new()
        {
            Position = _cameraController.Position,
            Forward = _cameraController.Forward,
            Right = _cameraController.Right,
            Up = _cameraController.Up,
            NearPlane = _cameraController.NearPlane,
            FarPlane = _cameraController.FarPlane,
            Fov = _cameraController.Fov.ToRadians(),
            ViewMatrix = _cameraController.ViewMatrix,
            ProjectionMatrix = _cameraController.ProjectionMatrix(Width, Height)
        };
        Matrix4x4.Invert(camera.ViewMatrix, out camera.InvViewMatrix);
        Matrix4x4.Invert(camera.ProjectionMatrix, out camera.InvProjectionMatrix);

        _device.UpdateBuffer(_cameraBuffer, in camera);

        _param.Width = Width;
        _param.Height = Height;

        _device.UpdateBuffer(_paramBuffer, in _param);
    }

    protected override void OnRender(TimeEventArgs e)
    {
        if (_framebufferObject != null)
        {
            _commandList.Begin();

            _commandList.SetFramebuffer(_framebufferObject.Framebuffer);
            _commandList.ClearColorTarget(0, RgbaFloat.Black);
            _commandList.ClearDepthStencil(1.0f);

            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.U32);
            _commandList.SetPipeline(_pipeline!);
            _commandList.SetResourceSet(0, _resourceSet0);
            _commandList.SetResourceSet(1, _resourceSet1);
            _commandList.SetResourceSet(2, _resourceSet2);

            _commandList.DrawIndexed((uint)_indices.Count, 1, 0, 0, 0);

            _framebufferObject.Present(_commandList);

            _commandList.End();

            _device.SubmitCommands(_commandList);

            ImGui.Image(_imGuiController.GetBinding(_device.Factory, _framebufferObject.PresentTexture), new Vector2(_framebufferObject.Width, _framebufferObject.Height));
        }
    }

    protected override void OnResize(ValueEventArgs<Vector2D<int>> e)
    {
        if (_framebufferObject != null)
        {
            _imGuiController.RemoveBinding(_imGuiController.GetBinding(_device.Factory, _framebufferObject.PresentTexture));

            _framebufferObject.Dispose();
        }
        _framebufferObject = new FramebufferObject(_device, e.Value.X, e.Value.Y);

        GraphicsPipelineDescription pipelineDescription = new()
        {
            BlendState = BlendStateDescription.SingleAlphaBlend,
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            RasterizerState = RasterizerStateDescription.Default,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = [_resourceLayout0, _resourceLayout1, _resourceLayout2],
            Shaders = new GraphicsShaderDescription([_vertexLayout], _shaders),
            Outputs = _framebufferObject.Framebuffer.OutputDescription
        };

        _pipeline?.Dispose();
        _pipeline = _device.Factory.CreateGraphicsPipeline(pipelineDescription);
    }

    protected override void Destroy()
    {
        _pipeline?.Dispose();
        _framebufferObject?.Dispose();

        _commandList.Dispose();

        foreach (Shader shader in _shaders)
        {
            shader.Dispose();
        }

        _resourceSet2.Dispose();
        _resourceLayout2.Dispose();

        _resourceSet1.Dispose();
        _resourceLayout1.Dispose();

        _resourceSet0.Dispose();
        _resourceLayout0.Dispose();

        _topLevel.Dispose();
        _bottomLevel.Dispose();

        _paramBuffer.Dispose();
        _lightBuffer.Dispose();
        _cameraBuffer.Dispose();
        _nodeBuffer.Dispose();
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
    }

    private void LoadGLTF(string path)
    {
        ModelRoot root = ModelRoot.Load(path, new ReadSettings() { Validation = ValidationMode.Skip });

        #region Load Textures
        using CommandList commandList = _device.Factory.CreateGraphicsCommandList();

        commandList.Begin();

        foreach (GLTFTexture gltfTexture in root.LogicalTextures)
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

            Texture texture = _device.Factory.CreateTexture(in description);
            texture.Name = gltfTexture.Name;

            TextureView textureView = _device.Factory.CreateTextureView(texture);
            textureView.Name = gltfTexture.Name;

            commandList.UpdateTexture(texture, image.Data, 0, 0, 0, (uint)width, (uint)height, 1, 0, 0);
            commandList.GenerateMipmaps(texture);

            _textures.Add(texture);
            _textureViews.Add(textureView);

            gltfTexture.ClearImages();
        }

        commandList.End();

        _device.SubmitCommands(commandList);
        #endregion

        #region Load Meshes
        foreach (Node node in root.LogicalNodes)
        {
            LoadNode(node);
        }
        #endregion
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
}
