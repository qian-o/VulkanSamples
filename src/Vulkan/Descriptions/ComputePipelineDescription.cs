using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct ComputePipelineDescription
{
    public ComputePipelineDescription(Shader shader,
                                      ResourceLayout[] resourceLayouts,
                                      SpecializationConstant[] specializations)
    {
        Shader = shader;
        ResourceLayouts = resourceLayouts;
        Specializations = specializations;
    }

    public ComputePipelineDescription(Shader shader,
                                      ResourceLayout[] resourceLayouts) : this(shader, resourceLayouts, [])
    {
    }

    /// <summary>
    /// The compute shader to be used.
    /// </summary>
    public Shader Shader { get; set; }

    /// <summary>
    /// This controls the resource layout of the compute shader.
    /// </summary>
    public ResourceLayout[] ResourceLayouts { get; set; }

    /// <summary>
    /// An array describing the value of each specialization constant.
    /// </summary>
    public SpecializationConstant[] Specializations { get; set; }
}
