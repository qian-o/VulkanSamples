namespace Graphics.Engine.Descriptions;

public struct FrameBufferDesc
{
    /// <summary>
    /// The depth/stencil texture attachment.
    /// </summary>
    public FrameBufferAttachmentDesc? DepthStencilTarget { get; set; }

    /// <summary>
    /// An array of color texture attachments.
    /// </summary>
    public FrameBufferAttachmentDesc[] ColorTargets { get; set; }

    public static FrameBufferDesc Default(Texture? depthStencilTarget,
                                          params Texture[] colorTargets)
    {
        return new()
        {
            DepthStencilTarget = depthStencilTarget != null ? FrameBufferAttachmentDesc.Default(depthStencilTarget) : null,
            ColorTargets = colorTargets.Select(FrameBufferAttachmentDesc.Default).ToArray()
        };
    }
}
