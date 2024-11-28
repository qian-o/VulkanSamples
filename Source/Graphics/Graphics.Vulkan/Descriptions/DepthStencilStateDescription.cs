using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct DepthStencilStateDescription
{
    public static readonly DepthStencilStateDescription DepthOnlyLessEqual = new(true, true, ComparisonKind.LessEqual);

    public static readonly DepthStencilStateDescription DepthOnlyLessEqualRead = new(true, false, ComparisonKind.LessEqual);

    public static readonly DepthStencilStateDescription DepthOnlyGreaterEqual = new(true, true, ComparisonKind.GreaterEqual);

    public static readonly DepthStencilStateDescription DepthOnlyGreaterEqualRead = new(true, false, ComparisonKind.GreaterEqual);

    public static readonly DepthStencilStateDescription Disabled = new(false, false, ComparisonKind.Always);

    public DepthStencilStateDescription(bool depthTestEnabled,
                                        bool depthWriteEnabled,
                                        ComparisonKind depthComparison,
                                        bool stencilTestEnabled,
                                        StencilBehaviorDescription stencilFront,
                                        StencilBehaviorDescription stencilBack,
                                        byte stencilReadMask,
                                        byte stencilWriteMask,
                                        uint stencilReference)
    {
        DepthTestEnabled = depthTestEnabled;
        DepthWriteEnabled = depthWriteEnabled;
        DepthComparison = depthComparison;
        StencilTestEnabled = stencilTestEnabled;
        StencilFront = stencilFront;
        StencilBack = stencilBack;
        StencilReadMask = stencilReadMask;
        StencilWriteMask = stencilWriteMask;
        StencilReference = stencilReference;
    }

    public DepthStencilStateDescription(bool depthTestEnabled,
                                        bool depthWriteEnabled,
                                        ComparisonKind depthComparison) : this(depthTestEnabled,
                                                                               depthWriteEnabled,
                                                                               depthComparison,
                                                                               false,
                                                                               default,
                                                                               default,
                                                                               0,
                                                                               0,
                                                                               0)
    {
    }

    /// <summary>
    /// Controls whether depth testing is enabled.
    /// </summary>
    public bool DepthTestEnabled { get; set; }

    /// <summary>
    /// Controls whether new depth values are written to the depth buffer.
    /// </summary>
    public bool DepthWriteEnabled { get; set; }

    /// <summary>
    /// The comparison function used to determine whether a new depth value should be written to the depth buffer.
    /// </summary>
    public ComparisonKind DepthComparison { get; set; }

    /// <summary>
    /// Controls whether the stencil test is enabled.
    /// </summary>
    public bool StencilTestEnabled { get; set; }

    /// <summary>
    /// Controls how stencil tests are handled for pixels whose surface faces towards the camera.
    /// </summary>
    public StencilBehaviorDescription StencilFront { get; set; }

    /// <summary>
    /// Controls how stencil tests are handled for pixels whose surface faces away from the camera.
    /// </summary>
    public StencilBehaviorDescription StencilBack { get; set; }

    /// <summary>
    /// Controls the portion of the stencil buffer used for reading.
    /// </summary>
    public byte StencilReadMask { get; set; }

    /// <summary>
    /// Controls the portion of the stencil buffer used for writing.
    /// </summary>
    public byte StencilWriteMask { get; set; }

    /// <summary>
    /// The reference value to use when doing a stencil test.
    /// </summary>
    public uint StencilReference { get; set; }
}
