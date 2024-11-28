namespace Graphics.Vulkan;

public record struct DeviceBufferRange : IBindableResource
{
    public DeviceBufferRange(DeviceBuffer buffer, uint offset, uint sizeInBytes)
    {
        Buffer = buffer;
        Offset = offset;
        SizeInBytes = sizeInBytes;
    }

    /// <summary>
    /// The buffer that this range is within.
    /// </summary>
    public DeviceBuffer Buffer { get; set; }

    /// <summary>
    /// The offset, in bytes, from the beginning of the buffer that this range starts at.
    /// </summary>
    public uint Offset { get; set; }

    /// <summary>
    /// The total number of bytes that this range encompasses.
    /// </summary>
    public uint SizeInBytes { get; set; }
}
