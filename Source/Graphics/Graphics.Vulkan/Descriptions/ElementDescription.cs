using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct ElementDescription
{
    public ElementDescription(string name,
                              ResourceKind kind,
                              ShaderStages stages,
                              ElementOptions options)
    {
        Name = name;
        Kind = kind;
        Stages = stages;
        Options = options;
    }

    public ElementDescription(string name,
                              ResourceKind kind,
                              ShaderStages stages) : this(name, kind, stages, ElementOptions.None)
    {
    }

    /// <summary>
    /// The name of the element.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The kind of resource.
    /// </summary>
    public ResourceKind Kind { get; set; }

    /// <summary>
    /// Use the resource of the shader stage.
    /// </summary>
    public ShaderStages Stages { get; set; }

    /// <summary>
    /// Miscellaneous resource options for this element.
    /// </summary>
    public ElementOptions Options { get; set; }
}
