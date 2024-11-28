using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct OutputDesc
{
    /// <summary>
    /// Depth stencil attachment format.
    /// </summary>
    public PixelFormat? DepthStencilAttachment { get; set; }

    /// <summary>
    /// Color attachment formats.
    /// </summary>
    public PixelFormat[] ColorAttachments { get; set; }

    /// <summary>
    /// The number of samples in each target attachment.
    /// </summary>
    public TextureSampleCount SampleCount { get; set; }

    public static OutputDesc Default(TextureSampleCount sampleCount = TextureSampleCount.Count1,
                                     PixelFormat? depthStencilAttachment = null,
                                     params PixelFormat[] colorAttachments)
    {
        return new()
        {
            DepthStencilAttachment = depthStencilAttachment,
            ColorAttachments = colorAttachments,
            SampleCount = sampleCount
        };
    }
}
