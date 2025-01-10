namespace Graphics.Vulkan.Descriptions;

public record struct FramebufferDescription
{
    public FramebufferDescription(FramebufferAttachmentDescription? depthTarget,
                                  params FramebufferAttachmentDescription[] colorTargets)
    {
        DepthTarget = depthTarget;
        ColorTargets = colorTargets;
    }

    public FramebufferDescription(Texture? depthTarget,
                                  params Texture[] colorTargets) : this(depthTarget != null ? new FramebufferAttachmentDescription(depthTarget) : null,
                                                                        colorTargets.Select(item => new FramebufferAttachmentDescription(item)).ToArray())
    {
    }

    /// <summary>
    /// The depth texture attachment.
    /// </summary>
    public FramebufferAttachmentDescription? DepthTarget { get; set; }

    /// <summary>
    /// An array of color texture attachments.
    /// </summary>
    public FramebufferAttachmentDescription[] ColorTargets { get; set; }
}
