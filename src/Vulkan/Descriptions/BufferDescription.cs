using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct BufferDescription
{
    public BufferDescription(uint sizeInBytes, BufferUsage usage)
    {
        SizeInBytes = sizeInBytes;
        Usage = usage;
    }

    /// <summary>
    /// The desired capacity, in bytes.
    /// </summary>
    public uint SizeInBytes { get; set; }

    /// <summary>
    /// Indicates the intended use of the buffer.
    /// </summary>
    public BufferUsage Usage { get; set; }

    public static unsafe BufferDescription Buffer<T>(int length, BufferUsage usage) where T : unmanaged
    {
        return new BufferDescription((uint)(length * sizeof(T)), usage);
    }
}
