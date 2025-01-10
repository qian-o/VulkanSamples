namespace Graphics.Vulkan.Descriptions;

public record struct RaytracingShaderDescription
{
    public RaytracingShaderDescription(Shader rayGenerationShader,
                                       Shader[] missShader,
                                       HitGroupDescription[] hitGroupShader)
    {
        RayGenerationShader = rayGenerationShader;
        MissShader = missShader;
        HitGroupShader = hitGroupShader;
    }

    /// <summary>
    /// Gets or sets the Raygeneration shader program.
    /// </summary>
    public Shader RayGenerationShader { get; set; }

    /// <summary>
    /// Gets or sets the Miss shader program.
    /// </summary>
    public Shader[] MissShader { get; set; }

    /// <summary>
    /// Gets or sets the Closest hit shader program.
    /// </summary>
    public HitGroupDescription[] HitGroupShader { get; set; }

    public readonly uint GetMissShaderCount()
    {
        if (MissShader != null)
        {
            return (uint)MissShader.Length;
        }
        else
        {
            return 0;
        }
    }

    public readonly uint GetHitGroupCount()
    {
        if (HitGroupShader != null)
        {
            return (uint)HitGroupShader.Length;
        }
        else
        {
            return 0;
        }
    }
}
