using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct BlendStateRenderTargetDesc
{
    /// <summary>
    /// Controls whether blending is enabled for the color attachment.
    /// </summary>
    public bool BlendEnabled { get; set; }

    /// <summary>
    /// This blend option specifies the operation to perform on the RGB value that the
    /// pixel shader outputs. The BlendOp member defines how to combine the SrcBlend
    /// and DestBlend operations.
    /// </summary>
    public Blend SourceBlendColor { get; set; }

    /// <summary>
    /// This blend option specifies the operation to perform on the current RGB value
    /// in the render target. The BlendOp member defines how to combine the SrcBlend
    /// and DestBlend operations.
    /// </summary>
    public Blend DestinationBlendColor { get; set; }

    /// <summary>
    /// This blend operation defines how to combine the SrcBlend and DestBlend operations.
    /// </summary>
    public BlendOperation BlendOperationColor { get; set; }

    /// <summary>
    /// This blend option specifies the operation to perform on the alpha value that
    /// the pixel shader outputs. Blend options that end in _COLOR are not allowed. The
    /// BlendOpAlpha member defines how to combine the SrcBlendAlpha and DestBlendAlpha
    /// operations.
    /// </summary>
    public Blend SourceBlendAlpha { get; set; }

    /// <summary>
    /// This blend option specifies the operation to perform on the current alpha value
    /// in the render target. Blend options that end in _COLOR are not allowed. The BlendOpAlpha
    /// member defines how to combine the SrcBlendAlpha and DestBlendAlpha operations.
    /// </summary>
    public Blend DestinationBlendAlpha { get; set; }

    /// <summary>
    /// This blend operation defines how to combine the SrcBlendAlpha and DestBlendAlpha operations.
    /// </summary>
    public BlendOperation BlendOperationAlpha { get; set; }

    /// <summary>
    /// A write mask.
    /// </summary>
    public ColorWriteChannels ColorWriteChannels { get; set; }

    public static BlendStateRenderTargetDesc Default(bool blendEnabled = false)
    {
        return new()
        {
            BlendEnabled = blendEnabled,
            SourceBlendColor = Blend.One,
            DestinationBlendColor = Blend.Zero,
            BlendOperationColor = BlendOperation.Add,
            SourceBlendAlpha = Blend.One,
            DestinationBlendAlpha = Blend.Zero,
            BlendOperationAlpha = BlendOperation.Add,
            ColorWriteChannels = ColorWriteChannels.All
        };
    }
}
