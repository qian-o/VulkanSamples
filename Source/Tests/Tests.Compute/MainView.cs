using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Graphics.Core;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Graphics.Vulkan.ImGui;
using Graphics.Windowing.Events;
using Hexa.NET.ImGui;
using Silk.NET.Maths;
using Tests.Core;
using Tests.Core.Helpers;

namespace Tests.Compute;

internal sealed unsafe class MainView : View
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

        public int Width;

        public int Height;

        public Vector3 Background;

        public int AntiAliasing;

        public int maxSteps;

        public float Epsilon;
    }

    private readonly ViewController _viewController;
    private readonly CameraController _cameraController;

    private readonly GraphicsDevice _device;
    private readonly ImGuiController _imGuiController;

    private readonly DeviceBuffer _buffer;
    private readonly ResourceLayout _resourceLayout;
    private readonly ResourceSet _resourceSet;
    private readonly Shader _shader;
    private readonly Pipeline _pipeline;
    private readonly CommandList _commandList;

    private Texture? _outputTexture;
    private TextureView? _outputTextureView;

    private Camera _camera;

    public MainView(GraphicsDevice device, ImGuiController imGuiController) : base("Compute View")
    {
        _viewController = new ViewController(this)
        {
            UseDpiScale = false
        };
        _cameraController = new CameraController(_viewController);

        string hlsl = File.ReadAllText("Assets/Shaders/SDF.hlsl");

        _device = device;
        _imGuiController = imGuiController;

        _buffer = device.Factory.CreateBuffer(BufferDescription.Buffer<Camera>(1, BufferUsage.ConstantBuffer | BufferUsage.Dynamic));

        _resourceLayout = device.Factory.CreateResourceLayout(new ResourceLayoutDescription(new ElementDescription("camera", ResourceKind.ConstantBuffer, ShaderStages.Compute),
                                                                                            new ElementDescription("outputTexture", ResourceKind.StorageImage, ShaderStages.Compute)));

        _resourceSet = device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout, _buffer));

        ShaderDescription[] shaderDescriptions = [new ShaderDescription(ShaderStages.Compute, Encoding.UTF8.GetBytes(hlsl), "main")];

        _shader = device.Factory.CreateShaderByHLSL(shaderDescriptions, (path) =>
        {
            return Encoding.UTF8.GetBytes(File.ReadAllText(Path.Combine("Assets/Shaders/", path)));
        }).First();

        _pipeline = device.Factory.CreateComputePipeline(new ComputePipelineDescription(_shader, [_resourceLayout]));

        _commandList = device.Factory.CreateComputeCommandList();

        _camera = new()
        {
            Position = _cameraController.Position = new Vector3(0.0f, 2.0f, 6.0f),
            Forward = _cameraController.Forward = new Vector3(0.0f, 0.0f, -1.0f),
            Right = new Vector3(1.0f, 0.0f, 0.0f),
            Up = new Vector3(0.0f, 1.0f, 0.0f),
            NearPlane = 0.1f,
            FarPlane = 100.0f,
            Fov = MathF.PI / 4.0f,
            Width = 100,
            Height = 100,
            Background = new Vector3(0.7f, 0.7f, 0.9f),
            AntiAliasing = 4,
            maxSteps = 256,
            Epsilon = 0.0001f
        };
    }

    protected override void OnUpdate(TimeEventArgs e)
    {
        _viewController.Update();
        _cameraController.Update((float)e.DeltaTime);

        if (ImGui.Begin("Properties"))
        {
            _cameraController.ShowEditor();

            ImGui.ColorEdit3("Background", ref _camera.Background);

            ImGui.DragInt("Anti Aliasing", ref _camera.AntiAliasing, 1, 1, 4);

            ImGui.DragInt("Max Steps", ref _camera.maxSteps, 1, 1, 1024);

            ImGui.End();
        }

        _camera.Position = _cameraController.Position;
        _camera.Forward = _cameraController.Forward;
        _camera.Right = _cameraController.Right;
        _camera.Up = _cameraController.Up;
        _camera.NearPlane = _cameraController.NearPlane;
        _camera.FarPlane = _cameraController.FarPlane;
        _camera.Fov = _cameraController.Fov.ToRadians();

        _device.UpdateBuffer(_buffer, in _camera);
    }

    protected override void OnRender(TimeEventArgs e)
    {
        if (_outputTexture != null)
        {
            _commandList.Begin();

            _commandList.SetPipeline(_pipeline);
            _commandList.SetResourceSet(0, _resourceSet);

            _commandList.Dispatch((uint)Math.Ceiling(_outputTexture.Width / 32.0),
                                  (uint)Math.Ceiling(_outputTexture.Height / 32.0),
                                  1);

            _commandList.End();

            _device.SubmitCommands(_commandList);

            ImGui.Image(_imGuiController.GetBinding(_device.Factory, _outputTexture), new Vector2(_outputTexture.Width, _outputTexture.Height));
        }
    }

    protected override void OnResize(ValueEventArgs<Vector2D<int>> e)
    {
        _outputTextureView?.Dispose();

        if (_outputTexture != null)
        {
            _imGuiController.RemoveBinding(_imGuiController.GetBinding(_device.Factory, _outputTexture));

            _outputTexture.Dispose();
        }

        _outputTexture = _device.Factory.CreateTexture(TextureDescription.Texture2D((uint)e.Value.X,
                                                                                    (uint)e.Value.Y,
                                                                                    1,
                                                                                    PixelFormat.R8G8B8A8UNorm,
                                                                                    TextureUsage.Sampled | TextureUsage.Storage));

        _outputTextureView = _device.Factory.CreateTextureView(_outputTexture);

        _resourceSet.UpdateSet(_outputTextureView, 1);

        _camera.Width = e.Value.X;
        _camera.Height = e.Value.Y;
    }

    protected override void Destroy()
    {
        _outputTextureView?.Dispose();
        _outputTexture?.Dispose();

        _commandList.Dispose();
        _pipeline.Dispose();
        _shader.Dispose();
        _resourceSet.Dispose();
        _resourceLayout.Dispose();
        _buffer.Dispose();
    }
}
