using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct RasterizerStateDescription
{
    public static readonly RasterizerStateDescription Default = new(FaceCullMode.Back,
                                                                    PolygonFillMode.Solid,
                                                                    FrontFace.CounterClockwise,
                                                                    true,
                                                                    false);

    public static readonly RasterizerStateDescription CullNone = new(FaceCullMode.None,
                                                                     PolygonFillMode.Solid,
                                                                     FrontFace.CounterClockwise,
                                                                     true,
                                                                     false);

    public RasterizerStateDescription(FaceCullMode cullMode,
                                      PolygonFillMode fillMode,
                                      FrontFace frontFace,
                                      bool depthClipEnabled,
                                      bool scissorTestEnabled)
    {
        CullMode = cullMode;
        FillMode = fillMode;
        FrontFace = frontFace;
        DepthClipEnabled = depthClipEnabled;
        ScissorTestEnabled = scissorTestEnabled;
    }

    /// <summary>
    /// Controls which face will be culled.
    /// </summary>
    public FaceCullMode CullMode { get; set; }

    /// <summary>
    /// Controls how the rasterizer fills polygons.
    /// </summary>
    public PolygonFillMode FillMode { get; set; }

    /// <summary>
    /// Controls the winding order used to determine the front face of primitives.
    /// </summary>
    public FrontFace FrontFace { get; set; }

    /// <summary>
    /// Controls whether depth clipping is enabled.
    /// </summary>
    public bool DepthClipEnabled { get; set; }

    /// <summary>
    /// Controls whether the scissor test is enabled.
    /// </summary>
    public bool ScissorTestEnabled { get; set; }
}
