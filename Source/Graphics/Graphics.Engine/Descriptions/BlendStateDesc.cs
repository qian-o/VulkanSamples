namespace Graphics.Engine.Descriptions;

public struct BlendStateDesc
{
    /// <summary>
    /// Specifies whether to use alpha-to-coverage as a multisampling technique when
    /// setting a pixel to a render target.
    /// </summary>
    public bool AlphaToCoverageEnabled { get; set; }

    /// <summary>
    /// Specifies whether to enable independent blending in simultaneous render targets.
    /// Set to TRUE to enable independent blending. If set to FALSE, only the RenderTarget[0]
    /// members are used; RenderTarget[1..7] are ignored.
    /// </summary>
    public bool IndependentBlendEnabled { get; set; }

    /// <summary>
    /// RenderTarget blend description 0 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget0 { get; set; }

    /// <summary>
    /// RenderTarget blend description 1 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget1 { get; set; }

    /// <summary>
    /// RenderTarget blend description 2 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget2 { get; set; }

    /// <summary>
    /// RenderTarget blend description 3 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget3 { get; set; }

    /// <summary>
    /// RenderTarget blend description 4 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget4 { get; set; }

    /// <summary>
    /// RenderTarget blend description 5 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget5 { get; set; }

    /// <summary>
    /// RenderTarget blend description 6 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget6 { get; set; }

    /// <summary>
    /// RenderTarget blend description 7 / 7.
    /// </summary>
    public BlendStateRenderTargetDesc RenderTarget7 { get; set; }

    public static BlendStateDesc Default()
    {
        return new()
        {
            AlphaToCoverageEnabled = false,
            IndependentBlendEnabled = false,
            RenderTarget0 = BlendStateRenderTargetDesc.Default(),
            RenderTarget1 = BlendStateRenderTargetDesc.Default(),
            RenderTarget2 = BlendStateRenderTargetDesc.Default(),
            RenderTarget3 = BlendStateRenderTargetDesc.Default(),
            RenderTarget4 = BlendStateRenderTargetDesc.Default(),
            RenderTarget5 = BlendStateRenderTargetDesc.Default(),
            RenderTarget6 = BlendStateRenderTargetDesc.Default(),
            RenderTarget7 = BlendStateRenderTargetDesc.Default()
        };
    }
}
