using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct DepthStencilStateDesc
{
    /// <summary>
    /// Controls whether depth testing is enabled.
    /// </summary>
    public bool DepthEnabled { get; set; }

    /// <summary>
    /// Controls whether new depth values are written to the depth buffer.
    /// </summary>
    public bool DepthWriteEnabled { get; set; }

    /// <summary>
    /// The comparison function used to determine whether a new depth value should be written to the depth buffer.
    /// </summary>
    public ComparisonFunction DepthFunction { get; set; }

    /// <summary>
    /// Controls whether the stencil test is enabled.
    /// </summary>
    public bool StencilEnabled { get; set; }

    /// <summary>
    /// Identify a portion of the depth-stencil buffer for reading stencil data.
    /// </summary>
    public byte StencilReadMask { get; set; }

    /// <summary>
    /// Identify a portion of the depth-stencil buffer for writing stencil data.
    /// </summary>
    public byte StencilWriteMask { get; set; }

    /// <summary>
    /// Identify how to use the results of the depth test and the stencil test for pixels
    /// whose surface normal is facing towards the camera.
    /// </summary>
    public DepthStencilOperationDesc FrontFace { get; set; }

    /// <summary>
    /// Identify how to use the results of the depth test and the stencil test for pixels
    /// whose surface normal is facing away from the camera.
    /// </summary>
    public DepthStencilOperationDesc BackFace { get; set; }

    public static DepthStencilStateDesc Default(bool depthEnabled = true, bool stencilEnabled = false)
    {
        return new()
        {
            DepthEnabled = depthEnabled,
            DepthWriteEnabled = true,
            DepthFunction = ComparisonFunction.LessEqual,
            StencilEnabled = stencilEnabled,
            StencilReadMask = byte.MaxValue,
            StencilWriteMask = byte.MaxValue,
            FrontFace = DepthStencilOperationDesc.Default(),
            BackFace = DepthStencilOperationDesc.Default()
        };
    }
}
