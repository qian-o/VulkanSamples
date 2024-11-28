using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct DepthStencilOperationDesc
{
    /// <summary>
    /// The stencil operation to perform when stencil testing fails.
    /// </summary>
    public StencilOperation StencilFailOperation { get; set; }

    /// <summary>
    /// The stencil operation to perform when stencil testing passes and depth testing fails.
    /// </summary>
    public StencilOperation StencilDepthFailOperation { get; set; }

    /// <summary>
    /// The stencil operation to perform when stencil testing and depth testing both pass.
    /// </summary>
    public StencilOperation StencilPassOperation { get; set; }

    /// <summary>
    /// The comparison operator used in the stencil test.
    /// </summary>
    public ComparisonFunction StencilFunction { get; set; }

    public static DepthStencilOperationDesc Default()
    {
        return new()
        {
            StencilFailOperation = StencilOperation.Keep,
            StencilDepthFailOperation = StencilOperation.Keep,
            StencilPassOperation = StencilOperation.Keep,
            StencilFunction = ComparisonFunction.Always
        };
    }
}
