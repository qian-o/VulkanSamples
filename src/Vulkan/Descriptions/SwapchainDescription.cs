using Graphics.Core;
using Silk.NET.Core.Contexts;

namespace Graphics.Vulkan.Descriptions;

public record struct SwapchainDescription
{
    public SwapchainDescription(IVkSurface target, PixelFormat? depthFormat)
    {
        Target = target;
        DepthFormat = depthFormat;
    }

    /// <summary>
    /// The render target.
    /// </summary>
    public IVkSurface Target { get; set; }

    /// <summary>
    /// The optional format of the depth target of the Swapchain's Framebuffer.
    /// If null, the Swapchain will not have a depth target.
    /// </summary>
    public PixelFormat? DepthFormat { get; set; }
}
