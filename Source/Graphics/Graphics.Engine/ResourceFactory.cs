using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;

namespace Graphics.Engine;

public abstract class ResourceFactory(Context context)
{
    public Context Context { get; } = context;

    public abstract SwapChain CreateSwapChain(ref readonly SwapChainDesc desc);

    public abstract Shader CreateShader(ref readonly ShaderDesc desc);

    public abstract Buffer CreateBuffer(ref readonly BufferDesc desc);

    public abstract Texture CreateTexture(ref readonly TextureDesc desc);

    public abstract TextureView CreateTextureView(ref readonly TextureViewDesc desc);

    public abstract Sampler CreateSampler(ref readonly SamplerDesc desc);

    public abstract ResourceLayout CreateResourceLayout(ref readonly ResourceLayoutDesc desc);

    public abstract ResourceSet CreateResourceSet(ref readonly ResourceSetDesc desc);

    public abstract FrameBuffer CreateFrameBuffer(ref readonly FrameBufferDesc desc);

    public abstract GraphicsPipeline CreateGraphicsPipeline(ref readonly GraphicsPipelineDesc desc);

    public abstract CommandProcessor CreateCommandProcessor(CommandProcessorType type = CommandProcessorType.Graphics);
}
