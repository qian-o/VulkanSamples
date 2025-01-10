using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct BlendAttachmentDescription
{
    public static readonly BlendAttachmentDescription OverrideBlend = new(false,
                                                                          BlendFactor.One,
                                                                          BlendFactor.Zero,
                                                                          BlendFunction.Add,
                                                                          BlendFactor.One,
                                                                          BlendFactor.Zero,
                                                                          BlendFunction.Add);

    public static readonly BlendAttachmentDescription AlphaBlend = new(true,
                                                                       BlendFactor.SourceAlpha,
                                                                       BlendFactor.InverseSourceAlpha,
                                                                       BlendFunction.Add,
                                                                       BlendFactor.SourceAlpha,
                                                                       BlendFactor.InverseSourceAlpha,
                                                                       BlendFunction.Add);

    public static readonly BlendAttachmentDescription AdditiveBlend = new(true,
                                                                          BlendFactor.SourceAlpha,
                                                                          BlendFactor.One,
                                                                          BlendFunction.Add,
                                                                          BlendFactor.SourceAlpha,
                                                                          BlendFactor.One,
                                                                          BlendFunction.Add);

    public static readonly BlendAttachmentDescription Disabled = new(false,
                                                                     BlendFactor.One,
                                                                     BlendFactor.Zero,
                                                                     BlendFunction.Add,
                                                                     BlendFactor.One,
                                                                     BlendFactor.Zero,
                                                                     BlendFunction.Add);

    public BlendAttachmentDescription(bool blendEnabled,
                                      ColorWriteMask colorWriteMask,
                                      BlendFactor sourceColorFactor,
                                      BlendFactor destinationColorFactor,
                                      BlendFunction colorFunction,
                                      BlendFactor sourceAlphaFactor,
                                      BlendFactor destinationAlphaFactor,
                                      BlendFunction alphaFunction)
    {
        BlendEnabled = blendEnabled;
        ColorWriteMask = colorWriteMask;
        SourceColorFactor = sourceColorFactor;
        DestinationColorFactor = destinationColorFactor;
        ColorFunction = colorFunction;
        SourceAlphaFactor = sourceAlphaFactor;
        DestinationAlphaFactor = destinationAlphaFactor;
        AlphaFunction = alphaFunction;
    }

    public BlendAttachmentDescription(bool blendEnabled,
                                      BlendFactor sourceColorFactor,
                                      BlendFactor destinationColorFactor,
                                      BlendFunction colorFunction,
                                      BlendFactor sourceAlphaFactor,
                                      BlendFactor destinationAlphaFactor,
                                      BlendFunction alphaFunction) : this(blendEnabled,
                                                                          ColorWriteMask.All,
                                                                          sourceColorFactor,
                                                                          destinationColorFactor,
                                                                          colorFunction,
                                                                          sourceAlphaFactor,
                                                                          destinationAlphaFactor,
                                                                          alphaFunction)
    {
    }

    /// <summary>
    /// Controls whether blending is enabled for the color attachment.
    /// </summary>
    public bool BlendEnabled { get; set; }

    /// <summary>
    /// Controls which components of the color will be written to the framebuffer.
    /// </summary>
    public ColorWriteMask ColorWriteMask { get; set; }

    /// <summary>
    /// Controls the source color's influence on the blend result.
    /// </summary>
    public BlendFactor SourceColorFactor { get; set; }

    /// <summary>
    /// Controls the destination color's influence on the blend result.
    /// </summary>
    public BlendFactor DestinationColorFactor { get; set; }

    /// <summary>
    /// Controls the function used to combine the source and destination color factors.
    /// </summary>
    public BlendFunction ColorFunction { get; set; }

    /// <summary>
    /// Controls the source alpha's influence on the blend result.
    /// </summary>
    public BlendFactor SourceAlphaFactor { get; set; }

    /// <summary>
    /// Controls the destination alpha's influence on the blend result.
    /// </summary>
    public BlendFactor DestinationAlphaFactor { get; set; }

    /// <summary>
    /// Controls the function used to combine the source and destination alpha factors.
    /// </summary>
    public BlendFunction AlphaFunction { get; set; }
}
