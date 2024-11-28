using Graphics.Core;
using Graphics.Vulkan.Descriptions;

namespace Graphics.Vulkan;

public class ResourceFactory
{
    private readonly VulkanResources _vkRes;

    internal ResourceFactory(VulkanResources vkRes)
    {
        _vkRes = vkRes;
    }

    public DeviceBuffer CreateBuffer(ref readonly BufferDescription description)
    {
        return new DeviceBuffer(_vkRes, in description);
    }

    public DeviceBuffer CreateBuffer(BufferDescription description) => CreateBuffer(in description);

    public Texture CreateTexture(ref readonly TextureDescription description)
    {
        return new Texture(_vkRes, in description);
    }

    public Texture CreateTexture(TextureDescription description) => CreateTexture(in description);

    public Texture CreateTexture(nint win32Handle, uint width, uint height, PixelFormat format)
    {
        return new Texture(_vkRes, win32Handle, width, height, format);
    }

    public TextureView CreateTextureView(Texture target)
    {
        TextureViewDescription description = new(target);

        return CreateTextureView(in description);
    }

    public TextureView CreateTextureView(ref readonly TextureViewDescription description)
    {
        return new TextureView(_vkRes, in description);
    }

    public TextureView CreateTextureView(TextureViewDescription description) => CreateTextureView(in description);

    public Framebuffer CreateFramebuffer(ref readonly FramebufferDescription description)
    {
        return new Framebuffer(_vkRes, in description, false);
    }

    public Framebuffer CreateFramebuffer(FramebufferDescription description) => CreateFramebuffer(in description);

    public Swapchain CreateSwapchain(ref readonly SwapchainDescription description)
    {
        return new Swapchain(_vkRes, in description);
    }

    public Swapchain CreateSwapchain(SwapchainDescription description) => CreateSwapchain(in description);

    public ResourceLayout CreateResourceLayout(ref readonly ResourceLayoutDescription description)
    {
        return new ResourceLayout(_vkRes, in description);
    }

    public ResourceLayout CreateResourceLayout(ResourceLayoutDescription description) => CreateResourceLayout(in description);

    public ResourceSet CreateResourceSet(ref readonly ResourceSetDescription description)
    {
        return new ResourceSet(_vkRes, in description);
    }

    public ResourceSet CreateResourceSet(ResourceSetDescription description) => CreateResourceSet(in description);

    public Sampler CreateSampler(ref readonly SamplerDescription description)
    {
        return new Sampler(_vkRes, in description);
    }

    public Sampler CreateSampler(SamplerDescription description) => CreateSampler(in description);

    public Shader CreateShader(ref readonly ShaderDescription description)
    {
        return new Shader(_vkRes, in description);
    }

    public Shader CreateShader(ShaderDescription description) => CreateShader(in description);

    public Pipeline CreateGraphicsPipeline(ref readonly GraphicsPipelineDescription description)
    {
        return new Pipeline(_vkRes, in description);
    }

    public Pipeline CreateGraphicsPipeline(GraphicsPipelineDescription description) => CreateGraphicsPipeline(in description);

    public Pipeline CreateComputePipeline(ref readonly ComputePipelineDescription description)
    {
        return new Pipeline(_vkRes, in description);
    }

    public Pipeline CreateComputePipeline(ComputePipelineDescription description) => CreateComputePipeline(in description);

    public Pipeline CreateRaytracingPipeline(ref readonly RaytracingPipelineDescription description)
    {
        return new Pipeline(_vkRes, in description);
    }

    public Pipeline CreateRaytracingPipeline(RaytracingPipelineDescription description) => CreateRaytracingPipeline(in description);

    public BottomLevelAS CreateBottomLevelAS(ref readonly BottomLevelASDescription description)
    {
        return new BottomLevelAS(_vkRes, in description);
    }

    public BottomLevelAS CreateBottomLevelAS(BottomLevelASDescription description) => CreateBottomLevelAS(in description);

    public TopLevelAS CreateTopLevelAS(ref readonly TopLevelASDescription description)
    {
        return new TopLevelAS(_vkRes, in description);
    }

    public TopLevelAS CreateTopLevelAS(TopLevelASDescription description) => CreateTopLevelAS(in description);

    public CommandList CreateGraphicsCommandList()
    {
        return new CommandList(_vkRes, _vkRes.GraphicsDevice.GraphicsExecutor, _vkRes.GraphicsDevice.GraphicsCommandPool);
    }

    public CommandList CreateComputeCommandList()
    {
        return new CommandList(_vkRes, _vkRes.GraphicsDevice.ComputeExecutor, _vkRes.GraphicsDevice.ComputeCommandPool);
    }
}
