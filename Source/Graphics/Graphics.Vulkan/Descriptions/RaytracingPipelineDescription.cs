namespace Graphics.Vulkan.Descriptions;

public record struct RaytracingPipelineDescription
{
    public RaytracingPipelineDescription(RaytracingShaderDescription shaders,
                                         ResourceLayout[] resourceLayouts,
                                         uint maxTraceRecursionDepth)
    {
        Shaders = shaders;
        ResourceLayouts = resourceLayouts;
        MaxTraceRecursionDepth = maxTraceRecursionDepth;
    }

    /// <summary>
    ///  Gets or sets the raytracing shader program.
    /// </summary>
    public RaytracingShaderDescription Shaders { get; set; }

    /// <summary>
    /// Describes the resources layout input.
    /// </summary>
    public ResourceLayout[] ResourceLayouts { get; set; }

    /// <summary>
    /// Limit on ray recursion for the raytracing pipeline. It must be in the range of
    /// 0 to 31. Below the maximum recursion depth, shader invocations such as closest
    /// depth, TraceRay calls result in the device going into removed state.
    /// </summary>
    public uint MaxTraceRecursionDepth { get; set; }
}
