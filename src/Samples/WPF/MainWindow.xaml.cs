using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using Graphics.Core;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Graphics.Windowing.Events;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;
using StbImageSharp;
using Tests.Core.Helpers;
using AlphaMode = SharpGLTF.Schema2.AlphaMode;
using GLTFTexture = SharpGLTF.Schema2.Texture;
using Pipeline = Graphics.Vulkan.Pipeline;
using Texture = Graphics.Vulkan.Texture;
using Window = System.Windows.Window;

namespace Tests.WPF;

public unsafe partial class MainWindow : Window
{
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

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Position, Forward, Right, Up, NearPlane, FarPlane, Fov);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Other
    {
        public int FrameCount;

        public int PathTracerSampleIndex;

        public float PathTracerAccumulationFactor;

        public Vector2 PixelOffset;

        public int LightCount;

        public int NumRays;

        public float AORadius;

        public float AORayMin;

        public int NumBounces;

        public float DiffuseCoef;

        public float SpecularCoef;

        public float SpecularPower;

        public float ReflectanceCoef;

        public int MaxRecursionDepth;

        public override readonly int GetHashCode()
        {
            HashCode hashCode = new();
            hashCode.Add(LightCount);
            hashCode.Add(NumRays);
            hashCode.Add(AORadius);
            hashCode.Add(AORayMin);
            hashCode.Add(NumBounces);
            hashCode.Add(DiffuseCoef);
            hashCode.Add(SpecularCoef);
            hashCode.Add(SpecularPower);
            hashCode.Add(ReflectanceCoef);
            hashCode.Add(MaxRecursionDepth);

            return hashCode.ToHashCode();
        }
    };

    [StructLayout(LayoutKind.Sequential)]
    private struct Light
    {
        public Vector3 Position;

        public float Radius;

        public Vector4 AmbientColor;

        public Vector4 DiffuseColor;

        public Vector4 SpecularColor;
    };

    [StructLayout(LayoutKind.Sequential)]
    private struct Vertex(Vector3 position, Vector3 normal, Vector2 texCoord, Vector3 color, Vector4 tangent)
    {
        public Vector3 Position = position;

        public Vector3 Normal = normal;

        public Vector2 TexCoord = texCoord;

        public Vector3 Color = color;

        public Vector4 Tangent = tangent;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GeometryNode
    {
        public uint VertexBuffer;

        public uint IndexBuffer;

        public bool AlphaMask;

        public float AlphaCutoff;

        public bool DoubleSided;

        public Vector4 BaseColorFactor;

        public uint BaseColorTextureIndex;

        public uint NormalTextureIndex;

        public uint RoughnessTextureIndex;
    }

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

    private Surface surface = null!;

    private CameraController _cameraController = null!;

    private List<Texture> _textures = null!;
    private List<TextureView> _textureViews = null!;
    private List<DeviceBuffer> _vertexBuffers = null!;
    private List<DeviceBuffer> _indexBuffers = null!;
    private List<GeometryNode> _geometryNodes = null!;
    private List<AccelStructTriangles> _triangles = null!;
    private List<Light> _lights = null!;

    private DeviceBuffer _cameraBuffer = null!;
    private DeviceBuffer _otherBuffer = null!;
    private DeviceBuffer _lightsBuffer = null!;
    private DeviceBuffer _geometryNodesBuffer = null!;
    private BottomLevelAS _bottomLevel = null!;
    private TopLevelAS _topLevel = null!;
    private ResourceLayout _resourceLayout0 = null!;
    private ResourceSet _resourceSet0 = null!;
    private ResourceLayout _resourceLayout1 = null!;
    private ResourceSet _resourceSet1 = null!;
    private ResourceLayout _resourceLayout2 = null!;
    private ResourceSet _resourceSet2 = null!;
    private ResourceLayout _resourceLayout3 = null!;
    private ResourceSet _resourceSet3 = null!;
    private ResourceLayout _resourceLayout4 = null!;
    private ResourceSet _resourceSet4 = null!;
    private Pipeline _pipeline = null!;

    private Camera _camera;
    private Other _other;
    private int _pathTracerSampleIndex;
    private int _pathTracerNumSamples = 256;

    private int cameraHashCode;
    private int otherHashCode;

    public MainWindow()
    {
        InitializeComponent();

        Loaded += MainWindow_Loaded;
        Unloaded += MainWindow_Unloaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Main.Child = surface = new();

        surface.Resized += Surface_Resized;
        surface.Rendering += Surface_Rendering;

        Init();
    }

    private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
    {
        Uninit();

        surface.Rendering -= Surface_Rendering;
        surface.Resized -= Surface_Resized;

        Main.Child = null;
    }

    private void Init()
    {
        _cameraController = new CameraController(surface);
        _cameraController.Transform(Matrix4x4.CreateRotationY(90.0f.ToRadians()));
        _cameraController.Transform(Matrix4x4.CreateTranslation(new Vector3(0.0f, 1.2f, 0.0f)));
        _textures = [];
        _textureViews = [];
        _vertexBuffers = [];
        _indexBuffers = [];
        _geometryNodes = [];
        _triangles = [];
        _lights = [];

        LoadGLTF("Assets/Models/Sponza/glTF/Sponza.gltf");

        _lights.Add(new Light()
        {
            Position = new Vector3(-100.0f, 100.0f, 0.0f),
            Radius = 3.0f,
            AmbientColor = new Vector4(0.05f, 0.05f, 0.05f, 1.0f),
            DiffuseColor = new Vector4(1.0f, 0.9f, 0.7f, 1.0f),
            SpecularColor = new Vector4(1.0f, 0.9f, 0.7f, 1.0f)
        });

        _cameraBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<Camera>(1, BufferUsage.ConstantBuffer));

        _otherBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<Other>(1, BufferUsage.ConstantBuffer));

        _lightsBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<Light>(_lights.Count, BufferUsage.StorageBuffer));
        App.Device.UpdateBuffer(_lightsBuffer, _lights.ToArray());

        _geometryNodesBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<GeometryNode>(_geometryNodes.Count, BufferUsage.StorageBuffer));
        App.Device.UpdateBuffer(_geometryNodesBuffer, _geometryNodes.ToArray());

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

        _resourceLayout0 = App.Device.Factory.CreateResourceLayout(new ResourceLayoutDescription(new ElementDescription("as", ResourceKind.AccelerationStructure, ShaderStages.RayGeneration | ShaderStages.ClosestHit),
                                                                                             new ElementDescription("camera", ResourceKind.ConstantBuffer, ShaderStages.RayGeneration | ShaderStages.ClosestHit),
                                                                                             new ElementDescription("other", ResourceKind.ConstantBuffer, ShaderStages.RayGeneration | ShaderStages.ClosestHit),
                                                                                             new ElementDescription("lights", ResourceKind.StorageBuffer, ShaderStages.ClosestHit),
                                                                                             new ElementDescription("geometryNodes", ResourceKind.StorageBuffer, ShaderStages.ClosestHit | ShaderStages.AnyHit),
                                                                                             new ElementDescription("outputTexture", ResourceKind.StorageImage, ShaderStages.RayGeneration)));
        _resourceSet0 = App.Device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout0, _topLevel, _cameraBuffer, _otherBuffer, _lightsBuffer, _geometryNodesBuffer));

        _resourceLayout1 = App.Device.Factory.CreateResourceLayout(ResourceLayoutDescription.Bindless((uint)_vertexBuffers.Count, new ElementDescription("vertexArray", ResourceKind.StorageBuffer, ShaderStages.ClosestHit | ShaderStages.AnyHit)));
        _resourceSet1 = App.Device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout1));
        _resourceSet1.UpdateBindless([.. _vertexBuffers]);

        _resourceLayout2 = App.Device.Factory.CreateResourceLayout(ResourceLayoutDescription.Bindless((uint)_indexBuffers.Count, new ElementDescription("indexArray", ResourceKind.StorageBuffer, ShaderStages.ClosestHit | ShaderStages.AnyHit)));
        _resourceSet2 = App.Device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout2));
        _resourceSet2.UpdateBindless([.. _indexBuffers]);

        _resourceLayout3 = App.Device.Factory.CreateResourceLayout(ResourceLayoutDescription.Bindless((uint)_textureViews.Count, new ElementDescription("textureArray", ResourceKind.SampledImage, ShaderStages.ClosestHit | ShaderStages.AnyHit)));
        _resourceSet3 = App.Device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout3));
        _resourceSet3.UpdateBindless([.. _textureViews]);

        _resourceLayout4 = App.Device.Factory.CreateResourceLayout(ResourceLayoutDescription.Bindless(2, new ElementDescription("samplerArray", ResourceKind.Sampler, ShaderStages.ClosestHit | ShaderStages.AnyHit)));
        _resourceSet4 = App.Device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout4));
        _resourceSet4.UpdateBindless(App.Device.Aniso4xSampler, App.Device.LinearSampler);

        byte[] shaderBytes = DxcHelpers.Compile(ShaderStages.Library,
                                                File.ReadAllText("Assets/Shaders/rayTracing.hlsl"),
                                                string.Empty);

        ShaderDescription[] shaderDescriptions =
        [
            new ShaderDescription(ShaderStages.RayGeneration, shaderBytes, "rayGen"),
            new ShaderDescription(ShaderStages.Miss, shaderBytes, "miss"),
            new ShaderDescription(ShaderStages.Miss, shaderBytes, "missShadow"),
            new ShaderDescription(ShaderStages.Miss, shaderBytes, "missAO"),
            new ShaderDescription(ShaderStages.Miss, shaderBytes, "missGI"),
            new ShaderDescription(ShaderStages.ClosestHit, shaderBytes, "closestHit"),
            new ShaderDescription(ShaderStages.ClosestHit, shaderBytes, "shadowChs"),
            new ShaderDescription(ShaderStages.ClosestHit, shaderBytes, "aoChs"),
            new ShaderDescription(ShaderStages.ClosestHit, shaderBytes, "giChs"),
            new ShaderDescription(ShaderStages.AnyHit, shaderBytes, "anyHit")
        ];

        Shader[] shaders = shaderDescriptions.Select(App.Device.Factory.CreateShader).ToArray();

        RaytracingPipelineDescription raytracingPipelineDescription = new()
        {
            Shaders = new RaytracingShaderDescription()
            {
                RayGenerationShader = shaders[0],
                MissShader = [shaders[1], shaders[2], shaders[3], shaders[4]],
                HitGroupShader =
                [
                    new HitGroupDescription()
                    {
                        ClosestHitShader = shaders[5],
                        AnyHitShader = shaders[9]
                    },
                    new HitGroupDescription()
                    {
                        ClosestHitShader = shaders[6]
                    },
                    new HitGroupDescription()
                    {
                        ClosestHitShader = shaders[7]
                    },
                    new HitGroupDescription()
                    {
                        ClosestHitShader = shaders[8]
                    }
                ]
            },
            ResourceLayouts = [_resourceLayout0, _resourceLayout1, _resourceLayout2, _resourceLayout3, _resourceLayout4],
            MaxTraceRecursionDepth = 6
        };

        _pipeline = App.Device.Factory.CreateRaytracingPipeline(raytracingPipelineDescription);

        foreach (Shader shader in shaders)
        {
            shader.Dispose();
        }

        _camera = new Camera()
        {
            Position = _cameraController.Position,
            Forward = _cameraController.Forward,
            Right = _cameraController.Right,
            Up = _cameraController.Up,
            NearPlane = _cameraController.NearPlane,
            FarPlane = _cameraController.FarPlane,
            Fov = _cameraController.Fov.ToRadians()
        };

        _other = new Other()
        {
            PathTracerSampleIndex = 0,
            PathTracerAccumulationFactor = 1.0f,
            PixelOffset = HaltonSequence[0],
            LightCount = _lights.Count,
            NumRays = 8,
            AORadius = 1.0f,
            AORayMin = 0.01f,
            FrameCount = 0,
            NumBounces = 3,
            DiffuseCoef = 0.9f,
            SpecularCoef = 0.7f,
            SpecularPower = 50,
            ReflectanceCoef = 0.5f,
            MaxRecursionDepth = 2
        };
    }

    private void Update(TimeEventArgs arg)
    {
        _cameraController.Update((float)arg.DeltaTime);

        _camera.Position = _cameraController.Position;
        _camera.Forward = _cameraController.Forward;
        _camera.Right = _cameraController.Right;
        _camera.Up = _cameraController.Up;
        _camera.NearPlane = _cameraController.NearPlane;
        _camera.FarPlane = _cameraController.FarPlane;
        _camera.Fov = _cameraController.Fov.ToRadians();

        if (cameraHashCode != _camera.GetHashCode())
        {
            _pathTracerSampleIndex = 0;

            cameraHashCode = _camera.GetHashCode();
        }

        if (otherHashCode != _other.GetHashCode())
        {
            _pathTracerSampleIndex = 0;

            otherHashCode = _other.GetHashCode();
        }

        _other.FrameCount++;
        _other.PathTracerSampleIndex = _pathTracerSampleIndex;
        _other.PathTracerAccumulationFactor = 1.0f / (_pathTracerSampleIndex + 1.0f);
        _other.PixelOffset = HaltonSequence[_other.FrameCount % HaltonSequence.Length];

        App.Device.UpdateBuffer(_cameraBuffer, in _camera);
        App.Device.UpdateBuffer(_otherBuffer, in _other);
        App.Device.UpdateBuffer(_lightsBuffer, _lights.ToArray());
    }

    private void Render(CommandList arg1)
    {
        if (_pathTracerSampleIndex < _pathTracerNumSamples)
        {
            arg1.SetPipeline(_pipeline);

            arg1.SetResourceSet(0, _resourceSet0);
            arg1.SetResourceSet(1, _resourceSet1);
            arg1.SetResourceSet(2, _resourceSet2);
            arg1.SetResourceSet(3, _resourceSet3);
            arg1.SetResourceSet(4, _resourceSet4);

            arg1.DispatchRays(surface.Texture.Width, surface.Texture.Height, 1);

            _pathTracerSampleIndex++;
        }

        Progress.Value = (int)(_pathTracerSampleIndex / (float)_pathTracerNumSamples * 100);
    }

    private void Uninit()
    {
        _pipeline.Dispose();
        _resourceSet4.Dispose();
        _resourceLayout4.Dispose();
        _resourceSet3.Dispose();
        _resourceLayout3.Dispose();
        _resourceSet2.Dispose();
        _resourceLayout2.Dispose();
        _resourceSet1.Dispose();
        _resourceLayout1.Dispose();
        _resourceSet0.Dispose();
        _resourceLayout0.Dispose();
        _topLevel.Dispose();
        _bottomLevel.Dispose();
        _geometryNodesBuffer.Dispose();
        _lightsBuffer.Dispose();
        _otherBuffer.Dispose();
        _cameraBuffer.Dispose();

        foreach (DeviceBuffer indexBuffer in _indexBuffers)
        {
            indexBuffer.Dispose();
        }

        foreach (DeviceBuffer vertexBuffer in _vertexBuffers)
        {
            vertexBuffer.Dispose();
        }

        foreach (TextureView textureView in _textureViews)
        {
            textureView.Dispose();
        }

        foreach (Texture texture in _textures)
        {
            texture.Dispose();
        }
    }

    private void Surface_Rendering(CommandList arg1, TimeEventArgs arg2)
    {
        Update(arg2);

        Render(arg1);
    }

    private void Surface_Resized(object? sender, EventArgs e)
    {
        _resourceSet0.UpdateSet(surface.TextureView, 5);

        _pathTracerSampleIndex = 0;
    }

    private void LoadGLTF(string path)
    {
        ModelRoot root = ModelRoot.Load(path, new ReadSettings() { Validation = ValidationMode.Skip });

        #region Load Textures
        using CommandList commandList = App.Device.Factory.CreateGraphicsCommandList();

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

            Texture texture = App.Device.Factory.CreateTexture(in description);
            texture.Name = gltfTexture.Name;

            TextureView textureView = App.Device.Factory.CreateTextureView(texture);
            textureView.Name = gltfTexture.Name;

            commandList.UpdateTexture(texture, image.Data, 0, 0, 0, (uint)width, (uint)height, 1, 0, 0);
            commandList.GenerateMipmaps(texture);

            _textures.Add(texture);
            _textureViews.Add(textureView);

            gltfTexture.ClearImages();
        }

        commandList.End();

        App.Device.SubmitCommands(commandList);
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

                vertices.Add(new Vertex(position,
                                        normal,
                                        texCoord,
                                        color,
                                        tangent));
            }

            if (primitive.IndexAccessor != null)
            {
                indices.AddRange(primitive.IndexAccessor.AsIndicesArray());
            }
            const BufferUsage bufferUsage = BufferUsage.StorageBuffer | BufferUsage.AccelerationStructure;

            DeviceBuffer vertexBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<Vertex>(vertices.Count, bufferUsage));
            App.Device.UpdateBuffer(vertexBuffer, vertices.ToArray());

            DeviceBuffer indexBuffer = App.Device.Factory.CreateBuffer(BufferDescription.Buffer<uint>(indices.Count, bufferUsage));
            App.Device.UpdateBuffer(indexBuffer, indices.ToArray());

            GeometryNode geometryNode = new()
            {
                VertexBuffer = (uint)_vertexBuffers.Count,
                IndexBuffer = (uint)_indexBuffers.Count,
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

            AccelStructTriangles triangles = new()
            {
                VertexBuffer = vertexBuffer,
                VertexFormat = PixelFormat.R32G32B32Float,
                VertexStrideInBytes = (uint)sizeof(Vertex),
                VertexCount = (uint)vertices.Count,
                VertexOffsetInBytes = 0,
                IndexBuffer = indexBuffer,
                IndexFormat = IndexFormat.U32,
                IndexCount = (uint)indices.Count,
                IndexOffsetInBytes = 0,
                Transform = node.WorldMatrix
            };

            _vertexBuffers.Add(vertexBuffer);
            _indexBuffers.Add(indexBuffer);
            _geometryNodes.Add(geometryNode);
            _triangles.Add(triangles);
        }
    }
}