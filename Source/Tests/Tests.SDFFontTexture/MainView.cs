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
using StbImageSharp;
using Tests.Core;
using Tests.SDFFontTexture.Models;

namespace Tests.SDFFontTexture;

internal sealed unsafe class MainView : View
{
    #region Structs
    [StructLayout(LayoutKind.Sequential)]
    private struct Vertex
    {
        public Vector3 Position;

        public Vector2 TexCoord;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ConstantBufferObject
    {
        public Matrix4x4 Model;

        public Matrix4x4 View;

        public Matrix4x4 Projection;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Properties
    {
        public float PxRange;
    }
    #endregion

    private readonly GraphicsDevice _device;
    private readonly ImGuiController _imGuiController;

    private readonly Layout _layout;
    private readonly Texture _sdfTexture;
    private readonly TextureView _sdfTextureView;
    private readonly DeviceBuffer _vertexBuffer;
    private readonly DeviceBuffer _indexBuffer;
    private readonly DeviceBuffer _constantBuffer;
    private readonly DeviceBuffer _normalBuffer;
    private readonly ResourceLayout _resourceLayout;
    private readonly ResourceSet _resourceSet;
    private readonly Shader[] _shaders;
    private readonly VertexLayoutDescription[] _vertexLayoutDescriptions;
    private readonly CommandList _commandList;

    private FramebufferObject? framebufferObject;
    private Pipeline? pipeline;

    private string str = "ABC";
    private Vector3 position = new(0.0f, 0.0f, -20.0f);
    private Properties properties = new() { PxRange = 5.0f };

    public MainView(GraphicsDevice device, ImGuiController imGuiController) : base("SDF Font Texture")
    {
        _device = device;
        _imGuiController = imGuiController;

        _layout = Layout.Get(File.ReadAllText("Assets/msyh.json"), "Assets/msyh.png");

        byte[] bytes = File.ReadAllBytes(_layout.PngPath!);

        ImageResult imageResult = ImageResult.FromMemory(bytes, ColorComponents.RedGreenBlueAlpha);

        _sdfTexture = device.Factory.CreateTexture(TextureDescription.Texture2D((uint)imageResult.Width,
                                                                                (uint)imageResult.Height,
                                                                                1,
                                                                                PixelFormat.R8G8B8A8UNorm,
                                                                                TextureUsage.Sampled));

        device.UpdateTexture(_sdfTexture,
                             imageResult.Data,
                             0,
                             0,
                             0,
                             (uint)imageResult.Width,
                             (uint)imageResult.Height,
                             1,
                             0,
                             0);

        _sdfTextureView = device.Factory.CreateTextureView(new TextureViewDescription(_sdfTexture));

        Vertex[] vertices =
        [
            new() { Position = new Vector3(-0.5f, -0.5f, 0.0f), TexCoord = new Vector2(0.0f, 1.0f) },
            new() { Position = new Vector3(0.5f, -0.5f, 0.0f), TexCoord = new Vector2(1.0f, 1.0f) },
            new() { Position = new Vector3(0.5f, 0.5f, 0.0f), TexCoord = new Vector2(1.0f, 0.0f) },
            new() { Position = new Vector3(-0.5f, 0.5f, 0.0f), TexCoord = new Vector2(0.0f, 0.0f) }
        ];

        uint[] indices = [0, 1, 2, 2, 3, 0];

        _vertexBuffer = device.Factory.CreateBuffer(BufferDescription.Buffer<Vertex>(vertices.Length, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        device.UpdateBuffer(_vertexBuffer, vertices);

        _indexBuffer = device.Factory.CreateBuffer(BufferDescription.Buffer<uint>(indices.Length, BufferUsage.IndexBuffer));
        device.UpdateBuffer(_indexBuffer, indices);

        _constantBuffer = device.Factory.CreateBuffer(BufferDescription.Buffer<ConstantBufferObject>(1, BufferUsage.ConstantBuffer | BufferUsage.Dynamic));
        _normalBuffer = device.Factory.CreateBuffer(BufferDescription.Buffer<Properties>(1, BufferUsage.ConstantBuffer | BufferUsage.Dynamic));

        ElementDescription cboDescription = new("cbo", ResourceKind.ConstantBuffer, ShaderStages.Vertex);
        ElementDescription normalDescription = new("properties", ResourceKind.ConstantBuffer, ShaderStages.Pixel);
        ElementDescription msdfDescription = new("msdf", ResourceKind.SampledImage, ShaderStages.Pixel);
        ElementDescription msdfSamplerDescription = new("msdfSampler", ResourceKind.Sampler, ShaderStages.Pixel);

        _resourceLayout = device.Factory.CreateResourceLayout(new ResourceLayoutDescription(cboDescription, normalDescription, msdfDescription, msdfSamplerDescription));

        _resourceSet = device.Factory.CreateResourceSet(new ResourceSetDescription(_resourceLayout, _constantBuffer, _normalBuffer, _sdfTextureView, _device.LinearSampler));

        string hlsl = File.ReadAllText("Assets/Shaders/SDF.hlsl");
        _shaders = device.Factory.CreateShaderByHLSL(new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(hlsl), "mainVS"),
                                                     new ShaderDescription(ShaderStages.Pixel, Encoding.UTF8.GetBytes(hlsl), "mainPS"));

        _vertexLayoutDescriptions =
        [
            new(new VertexElementDescription("Position", VertexElementFormat.Float3), new VertexElementDescription("TexCoord", VertexElementFormat.Float2))
        ];

        _commandList = device.Factory.CreateGraphicsCommandList();
    }

    protected override void OnUpdate(TimeEventArgs e)
    {
    }

    protected override void OnRender(TimeEventArgs e)
    {
        if (ImGui.Begin("Settings"))
        {
            ImGui.InputText("Char", ref str, 20);
            ImGui.DragFloat3("Position", ref position, 0.1f);

            ImGui.End();
        }

        if (framebufferObject != null)
        {
            _commandList.Begin();

            _commandList.SetFramebuffer(framebufferObject.Framebuffer);
            _commandList.ClearColorTarget(0, RgbaFloat.Black);
            _commandList.ClearDepthStencil(1.0f);

            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.U32);
            _commandList.SetPipeline(pipeline!);
            _commandList.SetResourceSet(0, _resourceSet);

            float offset = 0.0f;
            foreach (char @char in str)
            {
                Glyph? glyph = _layout.Glyphs!.FirstOrDefault(x => x.Unicode == @char);
                glyph ??= _layout.Glyphs!.First(x => x.Unicode == '?');

                if (glyph.PlaneBounds.Width > 0)
                {
                    float vertex1 = glyph.PlaneBounds.Left - 0.5f;
                    float vertex2 = glyph.PlaneBounds.Right - 0.5f;
                    float vertex3 = glyph.PlaneBounds.Top - 0.5f;
                    float vertex4 = glyph.PlaneBounds.Bottom - 0.5f;

                    float beginU = glyph.AtlasBounds.Left / _layout.Atlas!.Width;
                    float endU = glyph.AtlasBounds.Right / _layout.Atlas!.Width;
                    float beginV = (_layout.Atlas!.Height - glyph.AtlasBounds.Top) / _layout.Atlas!.Height;
                    float endV = (_layout.Atlas!.Height - glyph.AtlasBounds.Bottom) / _layout.Atlas!.Height;

                    Vertex[] vertices =
                    [
                        new() { Position = new Vector3(vertex1, vertex4, 0.0f), TexCoord = new Vector2(beginU, endV) },
                        new() { Position = new Vector3(vertex2, vertex4, 0.0f), TexCoord = new Vector2(endU, endV) },
                        new() { Position = new Vector3(vertex2, vertex3, 0.0f), TexCoord = new Vector2(endU, beginV) },
                        new() { Position = new Vector3(vertex1, vertex3, 0.0f), TexCoord = new Vector2(beginU, beginV) }
                    ];

                    Matrix4x4 translation = Matrix4x4.CreateTranslation(new Vector3(offset, 0.0f, 0.0f));
                    Matrix4x4 model = translation;

                    ConstantBufferObject cbo = new()
                    {
                        Model = model * Matrix4x4.CreateTranslation(position),
                        View = Matrix4x4.CreateLookAt(new Vector3(0.0f, 0.0f, 2.0f), Vector3.Zero, Vector3.UnitY),
                        Projection = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4, (float)framebufferObject.Width / framebufferObject.Height, 0.1f, 1000.0f)
                    };

                    properties.PxRange = _layout.Atlas!.DistanceRange;

                    _commandList.UpdateBuffer(_vertexBuffer, vertices);
                    _commandList.UpdateBuffer(_constantBuffer, ref cbo);
                    _commandList.UpdateBuffer(_normalBuffer, ref properties);

                    _commandList.DrawIndexed(_indexBuffer.SizeInBytes / sizeof(uint), 1, 0, 0, 0);
                }

                offset += glyph.Advance;
            }

            framebufferObject.Present(_commandList);

            _commandList.End();

            _device.SubmitCommands(_commandList);

            ImGui.Image(_imGuiController.GetBinding(_device.Factory, framebufferObject.PresentTexture), new Vector2(framebufferObject.Width, framebufferObject.Height));
        }
    }

    protected override void OnResize(ValueEventArgs<Vector2D<int>> e)
    {
        if (framebufferObject != null)
        {
            _imGuiController.RemoveBinding(_imGuiController.GetBinding(_device.Factory, framebufferObject.PresentTexture));

            framebufferObject.Dispose();
        }
        framebufferObject = new FramebufferObject(_device, e.Value.X, e.Value.Y);

        GraphicsPipelineDescription pipelineDescription = new()
        {
            BlendState = BlendStateDescription.SingleAlphaBlend,
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            RasterizerState = RasterizerStateDescription.Default,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = [_resourceLayout],
            Shaders = new GraphicsShaderDescription(_vertexLayoutDescriptions, _shaders),
            Outputs = framebufferObject.Framebuffer.OutputDescription
        };

        pipeline?.Dispose();
        pipeline = _device.Factory.CreateGraphicsPipeline(pipelineDescription);
    }

    protected override void Destroy()
    {
        _sdfTexture.Dispose();
        _sdfTextureView.Dispose();
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _constantBuffer.Dispose();
        _normalBuffer.Dispose();
        _resourceLayout.Dispose();
        _resourceSet.Dispose();

        foreach (Shader shader in _shaders)
        {
            shader.Dispose();
        }

        _commandList.Dispose();

        framebufferObject?.Dispose();
        pipeline?.Dispose();
    }
}
