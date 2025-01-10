using System.Numerics;
using Graphics.Core;

namespace Graphics.Vulkan;

public class AccelStructTriangles : AccelStructGeometry
{
    public DeviceBuffer? VertexBuffer { get; set; }

    public PixelFormat VertexFormat { get; set; }

    public uint VertexStrideInBytes { get; set; }

    public uint VertexCount { get; set; }

    public uint VertexOffsetInBytes { get; set; }

    public DeviceBuffer? IndexBuffer { get; set; }

    public IndexFormat IndexFormat { get; set; }

    public uint IndexCount { get; set; }

    public uint IndexOffsetInBytes { get; set; }

    public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;
}
