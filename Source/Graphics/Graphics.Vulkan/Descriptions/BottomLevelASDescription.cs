namespace Graphics.Vulkan.Descriptions;

public record struct BottomLevelASDescription
{
    public BottomLevelASDescription(params AccelStructGeometry[] geometries)
    {
        Geometries = geometries;
    }

    public AccelStructGeometry[] Geometries { get; set; }
}
