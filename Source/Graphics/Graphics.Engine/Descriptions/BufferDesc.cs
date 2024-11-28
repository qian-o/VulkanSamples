using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public unsafe struct BufferDesc
{
    /// <summary>
    /// The desired capacity, in bytes.
    /// </summary>
    public uint SizeInBytes { get; set; }

    /// <summary>
    /// Indicates the intended use of the buffer.
    /// </summary>
    public BufferUsage Usage { get; set; }

    public static BufferDesc Default(uint sizeInBytes, BufferUsage usage = BufferUsage.Dynamic)
    {
        return new()
        {
            SizeInBytes = sizeInBytes,
            Usage = usage
        };
    }

    public static BufferDesc Default<T>(int length, BufferUsage usage) where T : unmanaged
    {
        return new()
        {
            SizeInBytes = (uint)(length * sizeof(T)),
            Usage = usage
        };
    }
}
