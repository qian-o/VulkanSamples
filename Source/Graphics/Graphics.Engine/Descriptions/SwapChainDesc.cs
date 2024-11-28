using Graphics.Engine.Enums;
using Silk.NET.Core.Contexts;

namespace Graphics.Engine.Descriptions;

public struct SwapChainDesc
{
    /// <summary>
    /// The render target.
    /// </summary>
    public IVkSurface Target { get; set; }

    /// <summary>
    /// The pixel format of the depth stencil target.
    /// </summary>
    public PixelFormat? DepthStencilTargetFormat { get; set; }

    /// <summary>
    /// Vertical synchronization.
    /// </summary>
    public bool VSync { get; set; }

    public static SwapChainDesc Default(IVkSurface target,
                                        PixelFormat? depthStencilTargetFormat = PixelFormat.D24UNormS8UInt,
                                        bool vSync = false)
    {
        return new()
        {
            Target = target,
            DepthStencilTargetFormat = depthStencilTargetFormat,
            VSync = vSync
        };
    }
}
