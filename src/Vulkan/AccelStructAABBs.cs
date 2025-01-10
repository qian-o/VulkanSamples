namespace Graphics.Vulkan;

public class AccelStructAABBs : AccelStructGeometry
{
    public DeviceBuffer? AABBs { get; set; }

    public ulong Count { get; set; }

    public uint Stride { get; set; }

    public uint Offset { get; set; }
}
