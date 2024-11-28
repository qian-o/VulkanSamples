using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct SamplerDesc
{
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
    /// A function that compares sampled data against existing sampled data.
    /// If null, comparison sampling is not used.
    /// </summary>
    public ComparisonFunction? ComparisonFunction { get; set; }

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

    public static SamplerDesc Default(bool isLinear, uint anisotropy = 0)
    {
        SamplerFilter filter;

        if (anisotropy > 0)
        {
            filter = SamplerFilter.Anisotropic;
        }
        else
        {
            filter = isLinear ? SamplerFilter.MinLinearMagLinearMipLinear : SamplerFilter.MinPointMagPointMipPoint;
        }

        return new()
        {
            AddressModeU = AddressMode.Wrap,
            AddressModeV = AddressMode.Wrap,
            AddressModeW = AddressMode.Wrap,
            Filter = filter,
            ComparisonFunction = null,
            MaximumAnisotropy = anisotropy,
            MinimumLod = 0,
            MaximumLod = uint.MaxValue,
            LodBias = 0,
            BorderColor = SamplerBorderColor.TransparentBlack
        };
    }
}
