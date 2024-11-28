namespace Graphics.Vulkan.Descriptions;

public record struct HitGroupDescription
{
    /// <summary>
    /// Gets or sets the closestHit shader program.
    /// </summary>
    public Shader? ClosestHitShader { get; set; }

    /// <summary>
    /// Gets or sets the AnyHit shader program.
    /// </summary>
    public Shader? AnyHitShader { get; set; }

    /// <summary>
    /// Gets or sets the Intersection shader program.
    /// </summary>
    public Shader? IntersectionShader { get; set; }
}
