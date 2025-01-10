using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct OutputAttachmentDescription
{
    public OutputAttachmentDescription(PixelFormat format)
    {
        Format = format;
    }

    /// <summary>
    /// The format of the attachment.
    /// </summary>
    public PixelFormat Format { get; set; }
}
