using Graphics.Core;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;

namespace Tests.Core;

public class FramebufferObject : DisposableObject
{
    private const TextureSampleCount MaxSampleCount = TextureSampleCount.Count8;

    public FramebufferObject(GraphicsDevice device, int width, int height, TextureSampleCount? maxSampleCount = null)
    {
        ResourceFactory factory = device.Factory;

        Width = width;
        Height = height;
        SampleCount = maxSampleCount ?? MaxSampleCount;
        ColorTexture = factory.CreateTexture(TextureDescription.Texture2D((uint)width,
                                                                          (uint)height,
                                                                          1,
                                                                          PixelFormat.R8G8B8A8UNorm,
                                                                          TextureUsage.RenderTarget,
                                                                          SampleCount));

        DepthTexture = factory.CreateTexture(TextureDescription.Texture2D((uint)width,
                                                                          (uint)height,
                                                                          1,
                                                                          PixelFormat.D32FloatS8UInt,
                                                                          TextureUsage.DepthStencil,
                                                                          SampleCount));

        Framebuffer = factory.CreateFramebuffer(new FramebufferDescription(DepthTexture, ColorTexture));

        PresentTexture = factory.CreateTexture(TextureDescription.Texture2D((uint)width,
                                                                            (uint)height,
                                                                            1,
                                                                            PixelFormat.R8G8B8A8UNorm,
                                                                            TextureUsage.Sampled));
    }

    public int Width { get; }

    public int Height { get; }

    public TextureSampleCount SampleCount { get; }

    public Texture ColorTexture { get; }

    public Texture DepthTexture { get; }

    public Framebuffer Framebuffer { get; }

    public Texture PresentTexture { get; }

    public void Present(CommandList commandList)
    {
        if (SampleCount == TextureSampleCount.Count1)
        {
            commandList.CopyTexture(ColorTexture, PresentTexture);
        }
        else
        {
            commandList.ResolveTexture(ColorTexture, PresentTexture);
        }
    }

    protected override void Destroy()
    {
        PresentTexture.Dispose();
        Framebuffer.Dispose();
        DepthTexture.Dispose();
        ColorTexture.Dispose();
    }
}
