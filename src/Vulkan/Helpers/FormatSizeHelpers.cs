using Graphics.Core;

namespace Graphics.Vulkan.Helpers;

internal static class FormatSizeHelpers
{
    public static uint GetSizeInBytes(PixelFormat format)
    {
        return format switch
        {
            PixelFormat.R8UNorm or
            PixelFormat.R8SNorm or
            PixelFormat.R8UInt or
            PixelFormat.R8SInt => 1,

            PixelFormat.R16UNorm or
            PixelFormat.R16SNorm or
            PixelFormat.R16UInt or
            PixelFormat.R16SInt or
            PixelFormat.R16Float or
            PixelFormat.R8G8UNorm or
            PixelFormat.R8G8SNorm or
            PixelFormat.R8G8UInt or
            PixelFormat.R8G8SInt => 2,

            PixelFormat.R32UInt or
            PixelFormat.R32SInt or
            PixelFormat.R32Float or
            PixelFormat.R16G16UNorm or
            PixelFormat.R16G16SNorm or
            PixelFormat.R16G16UInt or
            PixelFormat.R16G16SInt or
            PixelFormat.R16G16Float or
            PixelFormat.R8G8B8A8UNorm or
            PixelFormat.R8G8B8A8UNormSRgb or
            PixelFormat.R8G8B8A8SNorm or
            PixelFormat.R8G8B8A8UInt or
            PixelFormat.R8G8B8A8SInt or
            PixelFormat.B8G8R8A8UNorm or
            PixelFormat.B8G8R8A8UNormSRgb or
            PixelFormat.R10G10B10A2UNorm or
            PixelFormat.R10G10B10A2UInt or
            PixelFormat.R11G11B10Float or
            PixelFormat.D24UNormS8UInt => 4,

            PixelFormat.D32FloatS8UInt => 5,

            PixelFormat.R16G16B16A16UNorm or
            PixelFormat.R16G16B16A16SNorm or
            PixelFormat.R16G16B16A16UInt or
            PixelFormat.R16G16B16A16SInt or
            PixelFormat.R16G16B16A16Float or
            PixelFormat.R32G32UInt or
            PixelFormat.R32G32SInt or
            PixelFormat.R32G32Float => 8,

            PixelFormat.R32G32B32A32Float or
            PixelFormat.R32G32B32A32UInt or
            PixelFormat.R32G32B32A32SInt => 16,

            _ => throw new NotSupportedException("Compressed formats are not supported by this method.")
        };
    }

    public static uint GetSizeInBytes(VertexElementFormat format)
    {
        return format switch
        {
            VertexElementFormat.Byte2Norm or
            VertexElementFormat.Byte2 or
            VertexElementFormat.SByte2Norm or
            VertexElementFormat.SByte2 or
            VertexElementFormat.Half1 => 2,

            VertexElementFormat.Float1 or
            VertexElementFormat.UInt1 or
            VertexElementFormat.Int1 or
            VertexElementFormat.Byte4Norm or
            VertexElementFormat.Byte4 or
            VertexElementFormat.SByte4Norm or
            VertexElementFormat.SByte4 or
            VertexElementFormat.UShort2Norm or
            VertexElementFormat.UShort2 or
            VertexElementFormat.Short2Norm or
            VertexElementFormat.Short2 or
            VertexElementFormat.Half2 => 4,

            VertexElementFormat.Float2 or
            VertexElementFormat.UInt2 or
            VertexElementFormat.Int2 or
            VertexElementFormat.UShort4Norm or
            VertexElementFormat.UShort4 or
            VertexElementFormat.Short4Norm or
            VertexElementFormat.Short4 or
            VertexElementFormat.Half4 => 8,

            VertexElementFormat.Float3 or
            VertexElementFormat.UInt3 or
            VertexElementFormat.Int3 => 12,

            VertexElementFormat.Float4 or
            VertexElementFormat.UInt4 or
            VertexElementFormat.Int4 => 16,

            _ => throw new NotSupportedException("Unsupported vertex element format.")
        };
    }

    public static uint GetSizeInBytes(ShaderConstantType type)
    {
        return type switch
        {
            ShaderConstantType.ConstBool => 4,
            ShaderConstantType.ConstInt16 => 2,
            ShaderConstantType.ConstUInt16 => 2,
            ShaderConstantType.ConstInt32 => 4,
            ShaderConstantType.ConstUInt32 => 4,
            ShaderConstantType.ConstInt64 => 8,
            ShaderConstantType.ConstUInt64 => 8,
            ShaderConstantType.ConstFloat => 4,
            ShaderConstantType.ConstDouble => 8,
            _ => throw new NotSupportedException("Unsupported shader constant type.")
        };
    }
}
