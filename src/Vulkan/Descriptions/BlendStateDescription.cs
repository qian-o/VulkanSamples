using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct BlendStateDescription
{
    public static readonly BlendStateDescription SingleOverrideBlend = new(default,
                                                                           BlendAttachmentDescription.OverrideBlend);

    public static readonly BlendStateDescription SingleAlphaBlend = new(default,
                                                                        BlendAttachmentDescription.AlphaBlend);

    public static readonly BlendStateDescription SingleAdditiveBlend = new(default,
                                                                           BlendAttachmentDescription.AdditiveBlend);

    public static readonly BlendStateDescription SingleDisabled = new(default,
                                                                      BlendAttachmentDescription.Disabled);

    public static readonly BlendStateDescription Empty = new(default, []);

    public BlendStateDescription(RgbaFloat blendFactor,
                                 BlendAttachmentDescription[] attachmentStates,
                                 bool alphaToCoverageEnabled)
    {
        BlendFactor = blendFactor;
        AttachmentStates = attachmentStates;
        AlphaToCoverageEnabled = alphaToCoverageEnabled;
    }

    public BlendStateDescription(RgbaFloat blendFactor,
                                 params BlendAttachmentDescription[] attachmentStates) : this(blendFactor,
                                                                                              attachmentStates,
                                                                                              false)
    {
    }

    public BlendStateDescription(RgbaFloat blendFactor,
                                 bool alphaToCoverageEnabled,
                                 params BlendAttachmentDescription[] attachmentStates) : this(blendFactor,
                                                                                              attachmentStates,
                                                                                              alphaToCoverageEnabled)
    {
    }

    /// <summary>
    /// A constant blend color used by all blend operations.
    /// </summary>
    public RgbaFloat BlendFactor { get; set; }

    /// <summary>
    /// The Array describes the blend state for each render target.
    /// </summary>
    public BlendAttachmentDescription[] AttachmentStates { get; set; }

    /// <summary>
    /// Enables alpha-to-coverage, which causes a fragment's alpha value to be used when determining multi-sample coverage.
    /// </summary>
    public bool AlphaToCoverageEnabled { get; set; }
}
