using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Graphics.Core;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Hexa.NET.ImGui;

namespace Graphics.Vulkan.ImGui;

public unsafe class ImGuiRenderer : DisposableObject
{
    private const string HLSL = @"
[[vk::constant_id(0)]] const bool UseLegacyColorSpaceHandling = false;

struct CBO
{
    float4x4 Projection;
};

struct VSInput
{
    [[vk::location(0)]] float2 Position : POSITION0;
    [[vk::location(1)]] float2 UV : NORMAL0;
    [[vk::location(2)]] float4 Color : COLOR0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    [[vk::location(0)]] float2 UV : TEXCOORD0;
    [[vk::location(1)]] float4 Color : COLOR0;
};

ConstantBuffer<CBO> cbo : register(b0, space0);
SamplerState pointSampler : register(s1, space0);
Texture2D textureColor : register(t0, space1);

float3 SrgbToLinear(float3 srgb)
{
    return srgb * (srgb * (srgb * 0.305306011f + 0.682171111f) + 0.012522878f);
}

VSOutput mainVS(VSInput input)
{
    VSOutput output;

    output.Position = mul(cbo.Projection, float4(input.Position, 0.0f, 1.0f));
    output.UV = input.UV;
    output.Color = input.Color;

    if (UseLegacyColorSpaceHandling)
    {
        output.Color.rgb = SrgbToLinear(output.Color.rgb);
    }

    return output;
}

float4 mainPS(VSOutput input) : SV_TARGET
{
    return input.Color * textureColor.Sample(pointSampler, input.UV);
}";

    private readonly GraphicsDevice _graphicsDevice;
    private readonly ResourceFactory _factory;
    private readonly OutputDescription _outputDescription;

    #region Resource Management
    private readonly object _lock = new();
    private readonly Dictionary<TextureView, ulong> _mapped = [];
    private readonly Dictionary<Texture, ulong> _mappedTextures = [];
    private readonly Dictionary<ulong, ResourceSet> _selfSets = [];
    private readonly Dictionary<ulong, TextureView> _selfViews = [];
    #endregion

    private DeviceBuffer _vertexBuffer = null!;
    private DeviceBuffer _indexBuffer = null!;
    private DeviceBuffer _cboBuffer = null!;
    private ResourceLayout _layout0 = null!;
    private ResourceLayout _layout1 = null!;
    private ResourceSet _resourceSet = null!;
    private Pipeline _pipeline = null!;
    private Texture _fontTexture = null!;

    internal ImGuiRenderer(GraphicsDevice graphicsDevice, OutputDescription outputDescription, ColorSpaceHandling colorSpaceHandling)
    {
        _graphicsDevice = graphicsDevice;
        _factory = graphicsDevice.Factory;
        _outputDescription = outputDescription;

        CreateDeviceResources(colorSpaceHandling);
    }

    public ulong GetBinding(ResourceFactory factory, TextureView textureView)
    {
        lock (_lock)
        {
            if (!_mapped.TryGetValue(textureView, out ulong binding))
            {
                while (_selfSets.ContainsKey(binding))
                {
                    binding++;
                }

                _mapped[textureView] = binding;
                _selfSets[binding] = factory.CreateResourceSet(new ResourceSetDescription(_layout1, textureView));
            }

            return binding;
        }
    }

    public ulong GetBinding(ResourceFactory factory, Texture texture)
    {
        lock (_lock)
        {
            if (!_mappedTextures.TryGetValue(texture, out ulong binding))
            {
                TextureView textureView = factory.CreateTextureView(texture);

                binding = GetBinding(factory, textureView);

                _mappedTextures[texture] = binding;
                _selfViews[binding] = textureView;
            }

            return binding;
        }
    }

    public void RemoveBinding(ulong binding)
    {
        lock (_lock)
        {
            if (_selfSets.TryGetValue(binding, out ResourceSet? resourceSet))
            {
                resourceSet.Dispose();
                _selfSets.Remove(binding);
            }

            if (_selfViews.TryGetValue(binding, out TextureView? textureView))
            {
                textureView.Dispose();
                _selfViews.Remove(binding);
            }
            else
            {
                textureView = _mapped.Keys.Where(k => _mapped[k] == binding).FirstOrDefault();
            }

            if (_mappedTextures.Keys.Where(k => _mappedTextures[k] == binding).FirstOrDefault() is Texture texture)
            {
                _mappedTextures.Remove(texture);
            }

            if (textureView != null)
            {
                _mapped.Remove(textureView);
            }
        }
    }

    public void RenderImDrawData(CommandList commandList, ImDrawDataPtr drawDataPtr)
    {
        if (drawDataPtr.CmdListsCount == 0)
        {
            return;
        }

        uint totalVBSize = (uint)(drawDataPtr.TotalVtxCount * sizeof(ImDrawVert));
        if (totalVBSize > _vertexBuffer.SizeInBytes)
        {
            commandList.DisposeSubmitted(_vertexBuffer);
            _vertexBuffer = _factory.CreateBuffer(BufferDescription.Buffer<ImDrawVert>(Convert.ToInt32(drawDataPtr.TotalVtxCount * 1.5), BufferUsage.VertexBuffer));
        }

        uint totalIBSize = (uint)(drawDataPtr.TotalIdxCount * sizeof(ushort));
        if (totalIBSize > _indexBuffer.SizeInBytes)
        {
            commandList.DisposeSubmitted(_indexBuffer);
            _indexBuffer = _factory.CreateBuffer(BufferDescription.Buffer<ushort>(Convert.ToInt32(drawDataPtr.TotalIdxCount * 1.5), BufferUsage.IndexBuffer));
        }

        Vector2 displayPos = drawDataPtr.DisplayPos;
        Vector2 displaySize = drawDataPtr.DisplaySize;

        // Update vertex and index buffers
        {
            uint vertOffset = 0;
            uint idxOffset = 0;
            for (int i = 0; i < drawDataPtr.CmdListsCount; i++)
            {
                ImDrawListPtr imDrawListPtr = drawDataPtr.CmdLists.Data[i];

                commandList.UpdateBuffer(_vertexBuffer,
                                         vertOffset,
                                         imDrawListPtr.VtxBuffer.Data,
                                         imDrawListPtr.VtxBuffer.Size);

                commandList.UpdateBuffer(_indexBuffer,
                                         idxOffset,
                                         imDrawListPtr.IdxBuffer.Data,
                                         imDrawListPtr.IdxBuffer.Size);

                vertOffset += (uint)(imDrawListPtr.VtxBuffer.Size * (uint)sizeof(ImDrawVert));
                idxOffset += (uint)(imDrawListPtr.IdxBuffer.Size * sizeof(ushort));
            }
        }

        // Orthographic projection matrix
        {
            Matrix4x4 orthoProjection = Matrix4x4.CreateOrthographicOffCenter(displayPos.X,
                                                                              displayPos.X + displaySize.X,
                                                                              displayPos.Y + displaySize.Y,
                                                                              displayPos.Y,
                                                                              -1.0f,
                                                                              1.0f);

            commandList.UpdateBuffer(_cboBuffer, ref orthoProjection);
        }

        commandList.SetVertexBuffer(0, _vertexBuffer);
        commandList.SetIndexBuffer(_indexBuffer, IndexFormat.U16);
        commandList.SetPipeline(_pipeline);
        commandList.SetResourceSet(0, _resourceSet);

        drawDataPtr.ScaleClipRects(DearImGui.GetIO().DisplayFramebufferScale);

        int vertexOffset = 0;
        int indexOffset = 0;
        for (int i = 0; i < drawDataPtr.CmdListsCount; i++)
        {
            ImDrawListPtr imDrawListPtr = drawDataPtr.CmdLists.Data[i];

            for (int j = 0; j < imDrawListPtr.CmdBuffer.Size; j++)
            {
                ImDrawCmd imDrawCmd = imDrawListPtr.CmdBuffer.Data[j];

                if (imDrawCmd.UserCallback != null)
                {
                    ImDrawCallback callback = Marshal.GetDelegateForFunctionPointer<ImDrawCallback>((nint)imDrawCmd.UserCallback);
                    callback(imDrawListPtr, &imDrawCmd);
                }
                else
                {
                    commandList.SetResourceSet(1, GetResourceSet(imDrawCmd.TextureId.Handle));

                    commandList.SetScissorRect(0,
                                               (uint)Math.Max(0, imDrawCmd.ClipRect.X - displayPos.X),
                                               (uint)Math.Max(0, imDrawCmd.ClipRect.Y - displayPos.Y),
                                               (uint)Math.Max(0, imDrawCmd.ClipRect.Z - imDrawCmd.ClipRect.X),
                                               (uint)Math.Max(0, imDrawCmd.ClipRect.W - imDrawCmd.ClipRect.Y));

                    commandList.DrawIndexed(imDrawCmd.ElemCount,
                                            1,
                                            (uint)(imDrawCmd.IdxOffset + indexOffset),
                                            (int)(imDrawCmd.VtxOffset + vertexOffset),
                                            0);
                }
            }

            vertexOffset += imDrawListPtr.VtxBuffer.Size;
            indexOffset += imDrawListPtr.IdxBuffer.Size;
        }
    }

    public void RecreateFontDeviceTexture()
    {
        RemoveBinding(GetBinding(_factory, _fontTexture));

        _fontTexture.Dispose();

        CreateFontDeviceTexture();
    }

    protected override void Destroy()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _cboBuffer.Dispose();
        _layout0.Dispose();
        _layout1.Dispose();
        _resourceSet.Dispose();
        _pipeline.Dispose();
        _fontTexture.Dispose();

        foreach (ResourceSet resourceSet in _selfSets.Values)
        {
            resourceSet.Dispose();
        }

        foreach (TextureView textureView in _selfViews.Values)
        {
            textureView.Dispose();
        }

        _selfSets.Clear();
        _selfViews.Clear();
        _mappedTextures.Clear();
        _mapped.Clear();
    }

    private ResourceSet GetResourceSet(ulong handle)
    {
        if (!_selfSets.TryGetValue(handle, out ResourceSet? resourceSet))
        {
            throw new InvalidOperationException("No resource set found for the given handle.");
        }

        return resourceSet;
    }

    private void CreateFontDeviceTexture()
    {
        ImGuiIOPtr io = DearImGui.GetIO();

        byte* pixels;
        int width;
        int height;
        io.Fonts.GetTexDataAsRGBA32(&pixels, &width, &height);

        TextureDescription description = TextureDescription.Texture2D((uint)width,
                                                                      (uint)height,
                                                                      1,
                                                                      PixelFormat.R8G8B8A8UNorm,
                                                                      TextureUsage.Sampled);

        _fontTexture = _factory.CreateTexture(description);

        _graphicsDevice.UpdateTexture(_fontTexture,
                                      pixels,
                                      width * height * 4,
                                      0,
                                      0,
                                      0,
                                      (uint)width,
                                      (uint)height,
                                      1,
                                      0,
                                      0);

        io.Fonts.SetTexID(GetBinding(_factory, _fontTexture));
    }

    private void CreateDeviceResources(ColorSpaceHandling colorSpaceHandling)
    {
        _vertexBuffer = _factory.CreateBuffer(BufferDescription.Buffer<ImDrawVert>(2000, BufferUsage.VertexBuffer));
        _indexBuffer = _factory.CreateBuffer(BufferDescription.Buffer<ushort>(4000, BufferUsage.IndexBuffer));
        _cboBuffer = _factory.CreateBuffer(BufferDescription.Buffer<Matrix4x4>(1, BufferUsage.ConstantBuffer));

        Shader[] shaders = _factory.CreateShaderByHLSL(new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(HLSL), "mainVS"),
                                                       new ShaderDescription(ShaderStages.Pixel, Encoding.UTF8.GetBytes(HLSL), "mainPS"));

        VertexLayoutDescription vertexLayoutDescription = new(new VertexElementDescription("Position", VertexElementFormat.Float2),
                                                              new VertexElementDescription("UV", VertexElementFormat.Float2),
                                                              new VertexElementDescription("Color", VertexElementFormat.Byte4Norm));

        ResourceLayoutDescription set0 = new(new ElementDescription("cbo", ResourceKind.ConstantBuffer, ShaderStages.Vertex),
                                             new ElementDescription("pointSampler", ResourceKind.Sampler, ShaderStages.Pixel));

        ResourceLayoutDescription set1 = new(new ElementDescription("textureColor", ResourceKind.SampledImage, ShaderStages.Pixel));

        _layout0 = _factory.CreateResourceLayout(set0);
        _layout1 = _factory.CreateResourceLayout(set1);
        _resourceSet = _factory.CreateResourceSet(new ResourceSetDescription(_layout0, _cboBuffer, _graphicsDevice.PointSampler));

        GraphicsPipelineDescription pipelineDescription = new()
        {
            BlendState = BlendStateDescription.SingleAlphaBlend,
            DepthStencilState = DepthStencilStateDescription.Disabled,
            RasterizerState = RasterizerStateDescription.CullNone,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = [_layout0, _layout1],
            Shaders = new GraphicsShaderDescription([vertexLayoutDescription],
                                                    shaders,
                                                    [new SpecializationConstant(0, colorSpaceHandling == ColorSpaceHandling.Legacy)]),
            Outputs = _outputDescription
        };

        _pipeline = _factory.CreateGraphicsPipeline(pipelineDescription);
        _pipeline.Name = "ImGui Pipeline";

        foreach (Shader shader in shaders)
        {
            shader.Dispose();
        }

        CreateFontDeviceTexture();
    }
}
