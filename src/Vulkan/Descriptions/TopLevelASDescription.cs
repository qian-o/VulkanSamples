using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct TopLevelASDescription
{
    public TopLevelASDescription(AccelStructBuildOptions options, AccelStructInstance[] instances, uint offset)
    {
        Options = options;
        Instances = instances;
        Offset = offset;
    }

    public TopLevelASDescription(AccelStructBuildOptions options, params AccelStructInstance[] instances) : this(options, instances, 0)
    {
    }

    public AccelStructBuildOptions Options { get; set; }

    public AccelStructInstance[] Instances { get; set; }

    public uint Offset { get; set; }
}
