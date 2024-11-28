using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct OutputDescription
{
    public OutputDescription(OutputAttachmentDescription? depthAttachment,
                             OutputAttachmentDescription[] colorAttachments,
                             TextureSampleCount sampleCount)
    {
        DepthAttachment = depthAttachment;
        ColorAttachments = colorAttachments;
        SampleCount = sampleCount;
    }

    public OutputDescription(OutputAttachmentDescription? depthAttachment,
                             params OutputAttachmentDescription[] colorAttachments)
    {
        DepthAttachment = depthAttachment;
        ColorAttachments = colorAttachments;
        SampleCount = TextureSampleCount.Count1;
    }

    /// <summary>
    /// A description of the depth attachment, or null if none exists.
    /// </summary>
    public OutputAttachmentDescription? DepthAttachment { get; set; }

    /// <summary>
    /// An array of attachment descriptions, one for each color attachment. May be empty.
    /// </summary>
    public OutputAttachmentDescription[] ColorAttachments { get; set; }

    /// <summary>
    /// The number of samples in each target attachment.
    /// </summary>
    public TextureSampleCount SampleCount { get; set; }

    internal static OutputDescription CreateFromFramebufferDescription(ref readonly FramebufferDescription description)
    {
        OutputAttachmentDescription? depthAttachment = null;
        OutputAttachmentDescription[] colorAttachments = new OutputAttachmentDescription[description.ColorTargets.Length];
        TextureSampleCount sampleCount = TextureSampleCount.Count1;

        if (description.DepthTarget != null)
        {
            depthAttachment = new OutputAttachmentDescription(description.DepthTarget.Value.Target.Format);
            sampleCount = description.DepthTarget.Value.Target.SampleCount;
        }

        for (int i = 0; i < description.ColorTargets.Length; i++)
        {
            colorAttachments[i] = new OutputAttachmentDescription(description.ColorTargets[i].Target.Format);

            if (i == 0)
            {
                sampleCount = description.ColorTargets[i].Target.SampleCount;
            }
        }

        return new OutputDescription(depthAttachment, colorAttachments, sampleCount);
    }
}
