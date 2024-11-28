using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct SamplerDescription
{
    public static readonly SamplerDescription Point = new(AddressMode.Wrap,
                                                          AddressMode.Wrap,
                                                          AddressMode.Wrap,
                                                          SamplerFilter.MinPointMagPointMipPoint,
                                                          null,
                                                          0,
                                                          0,
                                                          uint.MaxValue,
                                                          0,
                                                          SamplerBorderColor.TransparentBlack);

    public static readonly SamplerDescription Linear = new(AddressMode.Wrap,
                                                           AddressMode.Wrap,
                                                           AddressMode.Wrap,
                                                           SamplerFilter.MinLinearMagLinearMipLinear,
                                                           null,
                                                           0,
                                                           0,
                                                           uint.MaxValue,
                                                           0,
                                                           SamplerBorderColor.TransparentBlack);

    public static readonly SamplerDescription Aniso4x = new(AddressMode.Wrap,
                                                            AddressMode.Wrap,
                                                            AddressMode.Wrap,
                                                            SamplerFilter.Anisotropic,
                                                            null,
                                                            4,
                                                            0,
                                                            uint.MaxValue,
                                                            0,
                                                            SamplerBorderColor.TransparentBlack);

    public SamplerDescription(AddressMode addressModeU,
                              AddressMode addressModeV,
                              AddressMode addressModeW,
                              SamplerFilter filter,
                              ComparisonKind? comparisonKind,
                              uint maximumAnisotropy,
                              uint minimumLod,
                              uint maximumLod,
                              int lodBias,
                              SamplerBorderColor borderColor)
    {
        AddressModeU = addressModeU;
        AddressModeV = addressModeV;
        AddressModeW = addressModeW;
        Filter = filter;
        ComparisonKind = comparisonKind;
        MaximumAnisotropy = maximumAnisotropy;
        MinimumLod = minimumLod;
        MaximumLod = maximumLod;
        LodBias = lodBias;
        BorderColor = borderColor;
    }

    /// <summary>
    /// Mode to use for the U (or S) coordinate.
    /// </summary>
    public AddressMode AddressModeU { get; set; }

    /// <summary>
    /// Mode to use for the V (or T) coordinate.
    /// </summary>
    public AddressMode AddressModeV { get; set; }

    /// <summary>
    /// Mode to use for the W (or R) coordinate.
    /// </summary>
    public AddressMode AddressModeW { get; set; }

    /// <summary>
    /// The filter used when sampling.
    /// </summary>
    public SamplerFilter Filter { get; set; }

    /// <summary>
    /// An optional value controlling the kind of comparison to use when sampling. If null, comparison sampling is not used.
    /// </summary>
    public ComparisonKind? ComparisonKind { get; set; }

    /// <summary>
    /// The maximum anisotropy of the filter.
    /// </summary>
    public uint MaximumAnisotropy { get; set; }

    /// <summary>
    /// The minimum level of detail.
    /// </summary>
    public uint MinimumLod { get; set; }

    /// <summary>
    /// The maximum level of detail.
    /// </summary>
    public uint MaximumLod { get; set; }

    /// <summary>
    /// The level of detail bias.
    /// </summary>
    public int LodBias { get; set; }

    /// <summary>
    /// The border color to use when sampling outside the texture.
    /// </summary>
    public SamplerBorderColor BorderColor { get; set; }
}
