using System.Numerics;

namespace Graphics.Engine.Descriptions;

public struct RenderStateDesc
{
    /// <summary>
    /// Gets or sets the Rasterizer State.
    /// </summary>
    public RasterizerStateDesc RasterizerState { get; set; }

    /// <summary>
    /// Gets or sets the DepthStencil state.
    /// </summary>
    public DepthStencilStateDesc DepthStencilState { get; set; }

    /// <summary>
    /// Gets or sets the BlendState state.
    /// </summary>
    public BlendStateDesc BlendState { get; set; }

    /// <summary>
    /// The reference value to use when doing a stencil test.
    /// </summary>
    public int StencilReference { get; set; }

    /// <summary>
    /// Array of blend factors, one for each RGBA component.
    /// </summary>
    public Vector4? BlendFactor { get; set; }

    public static RenderStateDesc Default()
    {
        return new()
        {
            RasterizerState = RasterizerStateDesc.Default(),
            DepthStencilState = DepthStencilStateDesc.Default(),
            BlendState = BlendStateDesc.Default(),
            StencilReference = 0,
            BlendFactor = null
        };
    }
}
