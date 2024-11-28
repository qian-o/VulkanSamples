using Graphics.Engine.Enums;

namespace Graphics.Engine;

public readonly struct MappedResource(Buffer buffer,
                                      MapMode mode,
                                      nint data,
                                      uint sizeInBytes)
{
    public Buffer Buffer { get; } = buffer;

    public MapMode Mode { get; } = mode;

    public nint Data { get; } = data;

    public uint SizeInBytes { get; } = sizeInBytes;
}
