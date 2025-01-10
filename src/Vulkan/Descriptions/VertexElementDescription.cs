using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct VertexElementDescription
{
    public VertexElementDescription(string name, VertexElementFormat format, uint offset)
    {
        Name = name;
        Format = format;
        Offset = offset;
    }

    public VertexElementDescription(string name, VertexElementFormat format) : this(name, format, 0)
    {
    }

    /// <summary>
    /// The name of the element.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The format of the element.
    /// </summary>
    public VertexElementFormat Format { get; set; }

    /// <summary>
    /// The offset in bytes from the beginning of the vertex.
    /// </summary>
    public uint Offset { get; set; }
}
