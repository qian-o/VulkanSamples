using System.Numerics;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan.Helpers;

internal static unsafe class Util
{
    public static void GetMipDimensions(Texture texture, uint mipLevel, out uint width, out uint height, out uint depth)
    {
        width = GetDimension(texture.Width, mipLevel);
        height = GetDimension(texture.Height, mipLevel);
        depth = GetDimension(texture.Depth, mipLevel);
    }

    public static DeviceBufferRange GetBufferRange(IBindableResource bindableResource, uint additionalOffset)
    {
        if (bindableResource is DeviceBuffer buffer)
        {
            return new DeviceBufferRange(buffer, additionalOffset, buffer.SizeInBytes);
        }

        if (bindableResource is DeviceBufferRange bufferRange)
        {
            return new DeviceBufferRange(bufferRange.Buffer, bufferRange.Offset + additionalOffset, bufferRange.SizeInBytes - additionalOffset);
        }

        throw new InvalidOperationException("Invalid bindable resource type");
    }

    public static T AlignedSize<T>(T size, T alignment) where T : INumberBase<T>, IBitwiseOperators<T, T, T>
    {
        return (size + alignment - T.One) & ~(alignment - T.One);
    }

    public static T Lerp<T>(T start, T end, T value) where T : INumberBase<T>
    {
        return start + ((end - start) * value);
    }

    public static TransformMatrixKHR GetTransformMatrix(Matrix4x4 transform)
    {
        TransformMatrixKHR transformMatrix = new();
        transformMatrix.Matrix[0] = transform.M11;
        transformMatrix.Matrix[1] = transform.M12;
        transformMatrix.Matrix[2] = transform.M13;
        transformMatrix.Matrix[3] = transform.M14;
        transformMatrix.Matrix[4] = transform.M21;
        transformMatrix.Matrix[5] = transform.M22;
        transformMatrix.Matrix[6] = transform.M23;
        transformMatrix.Matrix[7] = transform.M24;
        transformMatrix.Matrix[8] = transform.M31;
        transformMatrix.Matrix[9] = transform.M32;
        transformMatrix.Matrix[10] = transform.M33;
        transformMatrix.Matrix[11] = transform.M34;

        return transformMatrix;
    }

    private static uint GetDimension(uint largestLevelDimension, uint mipLevel)
    {
        uint ret = largestLevelDimension;
        for (uint i = 0; i < mipLevel; i++)
        {
            ret /= 2;
        }

        return Math.Max(1, ret);
    }
}
