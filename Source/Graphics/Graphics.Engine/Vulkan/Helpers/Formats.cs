using Graphics.Engine.Enums;
using Silk.NET.Vulkan;
using FrontFace = Graphics.Engine.Enums.FrontFace;
using PrimitiveTopology = Graphics.Engine.Enums.PrimitiveTopology;
using VkFrontFace = Silk.NET.Vulkan.FrontFace;
using VkPrimitiveTopology = Silk.NET.Vulkan.PrimitiveTopology;

namespace Graphics.Engine.Vulkan.Helpers;

internal sealed class Formats
{
    public static ShaderStageFlags GetShaderStageFlags(ShaderStages stages)
    {
        ShaderStageFlags flags = ShaderStageFlags.None;

        if (stages.HasFlag(ShaderStages.Vertex))
        {
            flags |= ShaderStageFlags.VertexBit;
        }

        if (stages.HasFlag(ShaderStages.Hull))
        {
            flags |= ShaderStageFlags.TessellationControlBit;
        }

        if (stages.HasFlag(ShaderStages.Domain))
        {
            flags |= ShaderStageFlags.TessellationEvaluationBit;
        }

        if (stages.HasFlag(ShaderStages.Geometry))
        {
            flags |= ShaderStageFlags.GeometryBit;
        }

        if (stages.HasFlag(ShaderStages.Pixel))
        {
            flags |= ShaderStageFlags.FragmentBit;
        }

        if (stages.HasFlag(ShaderStages.Compute))
        {
            flags |= ShaderStageFlags.ComputeBit;
        }

        if (stages.HasFlag(ShaderStages.RayGeneration))
        {
            flags |= ShaderStageFlags.RaygenBitKhr;
        }

        if (stages.HasFlag(ShaderStages.Miss))
        {
            flags |= ShaderStageFlags.MissBitKhr;
        }

        if (stages.HasFlag(ShaderStages.ClosestHit))
        {
            flags |= ShaderStageFlags.ClosestHitBitKhr;
        }

        if (stages.HasFlag(ShaderStages.AnyHit))
        {
            flags |= ShaderStageFlags.AnyHitBitKhr;
        }

        if (stages.HasFlag(ShaderStages.Intersection))
        {
            flags |= ShaderStageFlags.IntersectionBitKhr;
        }

        if (stages.HasFlag(ShaderStages.Callable))
        {
            flags |= ShaderStageFlags.CallableBitKhr;
        }

        return flags;
    }

    public static ImageType GetImageType(TextureType type)
    {
        return type switch
        {
            TextureType.Texture1D => ImageType.Type1D,
            TextureType.Texture2D or TextureType.TextureCube => ImageType.Type2D,
            TextureType.Texture3D => ImageType.Type3D,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public static Format GetPixelFormat(PixelFormat pixelFormat)
    {
        return pixelFormat switch
        {
            PixelFormat.R8UNorm => Format.R8Unorm,
            PixelFormat.R8SNorm => Format.R8SNorm,
            PixelFormat.R8UInt => Format.R8Uint,
            PixelFormat.R8SInt => Format.R8Sint,

            PixelFormat.R16UNorm => Format.R16Unorm,
            PixelFormat.R16SNorm => Format.R16SNorm,
            PixelFormat.R16UInt => Format.R16Uint,
            PixelFormat.R16SInt => Format.R16Sint,
            PixelFormat.R16Float => Format.R16Sfloat,

            PixelFormat.R32UInt => Format.R32Uint,
            PixelFormat.R32SInt => Format.R32Sint,
            PixelFormat.R32Float => Format.R32Sfloat,

            PixelFormat.R8G8UNorm => Format.R8G8Unorm,
            PixelFormat.R8G8SNorm => Format.R8G8SNorm,
            PixelFormat.R8G8UInt => Format.R8G8Uint,
            PixelFormat.R8G8SInt => Format.R8G8Sint,

            PixelFormat.R16G16UNorm => Format.R16G16Unorm,
            PixelFormat.R16G16SNorm => Format.R16G16SNorm,
            PixelFormat.R16G16UInt => Format.R16G16Uint,
            PixelFormat.R16G16SInt => Format.R16G16Sint,
            PixelFormat.R16G16Float => Format.R16G16Sfloat,

            PixelFormat.R32G32UInt => Format.R32G32Uint,
            PixelFormat.R32G32SInt => Format.R32G32Sint,
            PixelFormat.R32G32Float => Format.R32G32Sfloat,

            PixelFormat.R8G8B8UNorm => Format.R8G8B8Unorm,
            PixelFormat.R8G8B8SNorm => Format.R8G8B8SNorm,
            PixelFormat.R8G8B8UInt => Format.R8G8B8Uint,
            PixelFormat.R8G8B8SInt => Format.R8G8B8Sint,

            PixelFormat.R16G16B16UNorm => Format.R16G16B16Unorm,
            PixelFormat.R16G16B16SNorm => Format.R16G16B16SNorm,
            PixelFormat.R16G16B16UInt => Format.R16G16B16Uint,
            PixelFormat.R16G16B16SInt => Format.R16G16B16Sint,
            PixelFormat.R16G16B16Float => Format.R16G16B16Sfloat,

            PixelFormat.R32G32B32UInt => Format.R32G32B32Uint,
            PixelFormat.R32G32B32SInt => Format.R32G32B32Sint,
            PixelFormat.R32G32B32Float => Format.R32G32B32Sfloat,

            PixelFormat.R8G8B8A8UNorm => Format.R8G8B8A8Unorm,
            PixelFormat.R8G8B8A8UNormSRgb => Format.R8G8B8A8Srgb,
            PixelFormat.R8G8B8A8SNorm => Format.R8G8B8A8SNorm,
            PixelFormat.R8G8B8A8UInt => Format.R8G8B8A8Uint,
            PixelFormat.R8G8B8A8SInt => Format.R8G8B8A8Sint,

            PixelFormat.R16G16B16A16UNorm => Format.R16G16B16A16Unorm,
            PixelFormat.R16G16B16A16SNorm => Format.R16G16B16A16SNorm,
            PixelFormat.R16G16B16A16UInt => Format.R16G16B16A16Uint,
            PixelFormat.R16G16B16A16SInt => Format.R16G16B16A16Sint,
            PixelFormat.R16G16B16A16Float => Format.R16G16B16A16Sfloat,

            PixelFormat.R32G32B32A32UInt => Format.R32G32B32A32Uint,
            PixelFormat.R32G32B32A32SInt => Format.R32G32B32A32Sint,
            PixelFormat.R32G32B32A32Float => Format.R32G32B32A32Sfloat,

            PixelFormat.B8G8R8A8UNorm => Format.B8G8R8A8Unorm,
            PixelFormat.B8G8R8A8UNormSRgb => Format.B8G8R8A8Srgb,

            PixelFormat.BC1RgbUNorm => Format.BC1RgbUnormBlock,
            PixelFormat.BC1RgbUNormSRgb => Format.BC1RgbSrgbBlock,
            PixelFormat.BC1RgbaUNorm => Format.BC1RgbaUnormBlock,
            PixelFormat.BC1RgbaUNormSRgb => Format.BC1RgbaSrgbBlock,
            PixelFormat.BC2UNorm => Format.BC2UnormBlock,
            PixelFormat.BC2UNormSRgb => Format.BC2SrgbBlock,
            PixelFormat.BC3UNorm => Format.BC3UnormBlock,
            PixelFormat.BC3UNormSRgb => Format.BC3SrgbBlock,
            PixelFormat.BC4UNorm => Format.BC4UnormBlock,
            PixelFormat.BC4SNorm => Format.BC4SNormBlock,
            PixelFormat.BC5UNorm => Format.BC5UnormBlock,
            PixelFormat.BC5SNorm => Format.BC5SNormBlock,
            PixelFormat.BC7UNorm => Format.BC7UnormBlock,
            PixelFormat.BC7UNormSRgb => Format.BC7SrgbBlock,

            PixelFormat.ETC2R8G8B8UNorm => Format.Etc2R8G8B8UnormBlock,
            PixelFormat.ETC2R8G8B8A1UNorm => Format.Etc2R8G8B8A1UnormBlock,
            PixelFormat.ETC2R8G8B8A8UNorm => Format.Etc2R8G8B8A8UnormBlock,

            PixelFormat.D16UNormS8UInt => Format.D16UnormS8Uint,
            PixelFormat.D24UNormS8UInt => Format.D24UnormS8Uint,
            PixelFormat.D32FloatS8UInt => Format.D32SfloatS8Uint,

            PixelFormat.R10G10B10A2UNorm => Format.A2B10G10R10UnormPack32,
            PixelFormat.R10G10B10A2UInt => Format.A2B10G10R10UintPack32,
            PixelFormat.R11G11B10Float => Format.B10G11R11UfloatPack32,

            _ => throw new ArgumentOutOfRangeException(nameof(pixelFormat))
        };
    }

    public static PixelFormat GetPixelFormat(Format format)
    {
        return format switch
        {
            Format.R8Unorm => PixelFormat.R8UNorm,
            Format.R8SNorm => PixelFormat.R8SNorm,
            Format.R8Uint => PixelFormat.R8UInt,
            Format.R8Sint => PixelFormat.R8SInt,

            Format.R16Unorm => PixelFormat.R16UNorm,
            Format.R16SNorm => PixelFormat.R16SNorm,
            Format.R16Uint => PixelFormat.R16UInt,
            Format.R16Sint => PixelFormat.R16SInt,
            Format.R16Sfloat => PixelFormat.R16Float,

            Format.R32Uint => PixelFormat.R32UInt,
            Format.R32Sint => PixelFormat.R32SInt,
            Format.R32Sfloat => PixelFormat.R32Float,

            Format.R8G8Unorm => PixelFormat.R8G8UNorm,
            Format.R8G8SNorm => PixelFormat.R8G8SNorm,
            Format.R8G8Uint => PixelFormat.R8G8UInt,
            Format.R8G8Sint => PixelFormat.R8G8SInt,

            Format.R16G16Unorm => PixelFormat.R16G16UNorm,
            Format.R16G16SNorm => PixelFormat.R16G16SNorm,
            Format.R16G16Uint => PixelFormat.R16G16UInt,
            Format.R16G16Sint => PixelFormat.R16G16SInt,
            Format.R16G16Sfloat => PixelFormat.R16G16Float,

            Format.R32G32Uint => PixelFormat.R32G32UInt,
            Format.R32G32Sint => PixelFormat.R32G32SInt,
            Format.R32G32Sfloat => PixelFormat.R32G32Float,

            Format.R8G8B8Unorm => PixelFormat.R8G8B8UNorm,
            Format.R8G8B8SNorm => PixelFormat.R8G8B8SNorm,
            Format.R8G8B8Uint => PixelFormat.R8G8B8UInt,
            Format.R8G8B8Sint => PixelFormat.R8G8B8SInt,

            Format.R16G16B16Unorm => PixelFormat.R16G16B16UNorm,
            Format.R16G16B16SNorm => PixelFormat.R16G16B16SNorm,
            Format.R16G16B16Uint => PixelFormat.R16G16B16UInt,
            Format.R16G16B16Sint => PixelFormat.R16G16B16SInt,
            Format.R16G16B16Sfloat => PixelFormat.R16G16B16Float,

            Format.R32G32B32Uint => PixelFormat.R32G32B32UInt,
            Format.R32G32B32Sint => PixelFormat.R32G32B32SInt,
            Format.R32G32B32Sfloat => PixelFormat.R32G32B32Float,

            Format.R8G8B8A8Unorm => PixelFormat.R8G8B8A8UNorm,
            Format.R8G8B8A8SNorm => PixelFormat.R8G8B8A8SNorm,
            Format.R8G8B8A8Uint => PixelFormat.R8G8B8A8UInt,
            Format.R8G8B8A8Sint => PixelFormat.R8G8B8A8SInt,

            Format.R16G16B16A16Unorm => PixelFormat.R16G16B16A16UNorm,
            Format.R16G16B16A16SNorm => PixelFormat.R16G16B16A16SNorm,
            Format.R16G16B16A16Uint => PixelFormat.R16G16B16A16UInt,
            Format.R16G16B16A16Sint => PixelFormat.R16G16B16A16SInt,
            Format.R16G16B16A16Sfloat => PixelFormat.R16G16B16A16Float,

            Format.R32G32B32A32Uint => PixelFormat.R32G32B32A32UInt,
            Format.R32G32B32A32Sint => PixelFormat.R32G32B32A32SInt,
            Format.R32G32B32A32Sfloat => PixelFormat.R32G32B32A32Float,

            Format.B8G8R8A8Unorm => PixelFormat.B8G8R8A8UNorm,
            Format.B8G8R8A8Srgb => PixelFormat.B8G8R8A8UNormSRgb,

            Format.BC1RgbUnormBlock => PixelFormat.BC1RgbUNorm,
            Format.BC1RgbSrgbBlock => PixelFormat.BC1RgbUNormSRgb,
            Format.BC1RgbaUnormBlock => PixelFormat.BC1RgbaUNorm,
            Format.BC1RgbaSrgbBlock => PixelFormat.BC1RgbaUNormSRgb,

            Format.BC2UnormBlock => PixelFormat.BC2UNorm,
            Format.BC2SrgbBlock => PixelFormat.BC2UNormSRgb,

            Format.BC3UnormBlock => PixelFormat.BC3UNorm,
            Format.BC3SrgbBlock => PixelFormat.BC3UNormSRgb,

            Format.BC4UnormBlock => PixelFormat.BC4UNorm,
            Format.BC4SNormBlock => PixelFormat.BC4SNorm,

            Format.BC5UnormBlock => PixelFormat.BC5UNorm,
            Format.BC5SNormBlock => PixelFormat.BC5SNorm,

            Format.BC7UnormBlock => PixelFormat.BC7UNorm,
            Format.BC7SrgbBlock => PixelFormat.BC7UNormSRgb,

            Format.D16Unorm => PixelFormat.D16UNormS8UInt,
            Format.D24UnormS8Uint => PixelFormat.D24UNormS8UInt,
            Format.D32SfloatS8Uint => PixelFormat.D32FloatS8UInt,

            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    public static SampleCountFlags GetSampleCountFlags(TextureSampleCount sampleCount)
    {
        return sampleCount switch
        {
            TextureSampleCount.Count1 => SampleCountFlags.Count1Bit,
            TextureSampleCount.Count2 => SampleCountFlags.Count2Bit,
            TextureSampleCount.Count4 => SampleCountFlags.Count4Bit,
            TextureSampleCount.Count8 => SampleCountFlags.Count8Bit,
            TextureSampleCount.Count16 => SampleCountFlags.Count16Bit,
            TextureSampleCount.Count32 => SampleCountFlags.Count32Bit,
            _ => throw new ArgumentOutOfRangeException(nameof(sampleCount))
        };
    }

    public static ImageUsageFlags GetImageUsageFlags(TextureUsage usage)
    {
        ImageUsageFlags imageUsageFlags = ImageUsageFlags.TransferDstBit | ImageUsageFlags.TransferSrcBit;

        if (usage.HasFlag(TextureUsage.Sampled))
        {
            imageUsageFlags |= ImageUsageFlags.SampledBit;
        }

        if (usage.HasFlag(TextureUsage.Storage))
        {
            imageUsageFlags |= ImageUsageFlags.StorageBit;
        }

        if (usage.HasFlag(TextureUsage.RenderTarget))
        {
            imageUsageFlags |= ImageUsageFlags.ColorAttachmentBit;
        }

        if (usage.HasFlag(TextureUsage.DepthStencil))
        {
            imageUsageFlags |= ImageUsageFlags.DepthStencilAttachmentBit;
        }

        return imageUsageFlags;
    }

    public static ImageAspectFlags GetImageAspectFlags(TextureUsage usage)
    {
        ImageAspectFlags flags = ImageAspectFlags.None;

        if (usage.HasFlag(TextureUsage.DepthStencil))
        {
            flags |= ImageAspectFlags.DepthBit | ImageAspectFlags.StencilBit;
        }
        else
        {
            flags |= ImageAspectFlags.ColorBit;
        }

        return flags;
    }

    public static ImageAspectFlags GetImageAspectFlags(ImageLayout imageLayout)
    {
        ImageAspectFlags flags = ImageAspectFlags.None;

        if (imageLayout == ImageLayout.DepthStencilAttachmentOptimal)
        {
            flags |= ImageAspectFlags.DepthBit | ImageAspectFlags.StencilBit;
        }
        else
        {
            flags |= ImageAspectFlags.ColorBit;
        }

        return flags;
    }

    public static ImageViewType GetImageViewType(TextureType type)
    {
        return type switch
        {
            TextureType.Texture1D => ImageViewType.Type1D,
            TextureType.Texture2D => ImageViewType.Type2D,
            TextureType.Texture3D => ImageViewType.Type3D,
            TextureType.TextureCube => ImageViewType.TypeCube,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public static void GetFilter(SamplerFilter filter,
                                 out Filter minFilter,
                                 out Filter magFilter,
                                 out SamplerMipmapMode mipmapMode)
    {
        switch (filter)
        {
            case SamplerFilter.MinPointMagPointMipPoint:
                minFilter = Filter.Nearest;
                magFilter = Filter.Nearest;
                mipmapMode = SamplerMipmapMode.Nearest;
                break;
            case SamplerFilter.MinPointMagPointMipLinear:
                minFilter = Filter.Nearest;
                magFilter = Filter.Nearest;
                mipmapMode = SamplerMipmapMode.Linear;
                break;
            case SamplerFilter.MinPointMagLinearMipPoint:
                minFilter = Filter.Nearest;
                magFilter = Filter.Linear;
                mipmapMode = SamplerMipmapMode.Nearest;
                break;
            case SamplerFilter.MinPointMagLinearMipLinear:
                minFilter = Filter.Nearest;
                magFilter = Filter.Linear;
                mipmapMode = SamplerMipmapMode.Linear;
                break;
            case SamplerFilter.MinLinearMagPointMipPoint:
                minFilter = Filter.Linear;
                magFilter = Filter.Nearest;
                mipmapMode = SamplerMipmapMode.Nearest;
                break;
            case SamplerFilter.MinLinearMagPointMipLinear:
                minFilter = Filter.Linear;
                magFilter = Filter.Nearest;
                mipmapMode = SamplerMipmapMode.Linear;
                break;
            case SamplerFilter.MinLinearMagLinearMipPoint:
                minFilter = Filter.Linear;
                magFilter = Filter.Linear;
                mipmapMode = SamplerMipmapMode.Nearest;
                break;
            case SamplerFilter.MinLinearMagLinearMipLinear:
                minFilter = Filter.Linear;
                magFilter = Filter.Linear;
                mipmapMode = SamplerMipmapMode.Linear;
                break;
            case SamplerFilter.Anisotropic:
                minFilter = Filter.Linear;
                magFilter = Filter.Linear;
                mipmapMode = SamplerMipmapMode.Linear;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(filter));
        }
    }

    public static SamplerAddressMode GetSamplerAddressMode(AddressMode addressMode)
    {
        return addressMode switch
        {
            AddressMode.Wrap => SamplerAddressMode.Repeat,
            AddressMode.Mirror => SamplerAddressMode.MirroredRepeat,
            AddressMode.Clamp => SamplerAddressMode.ClampToEdge,
            AddressMode.Border => SamplerAddressMode.ClampToBorder,
            _ => throw new ArgumentOutOfRangeException(nameof(addressMode))
        };
    }

    public static CompareOp GetCompareOp(ComparisonFunction comparisonKind)
    {
        return comparisonKind switch
        {
            ComparisonFunction.Never => CompareOp.Never,
            ComparisonFunction.Less => CompareOp.Less,
            ComparisonFunction.Equal => CompareOp.Equal,
            ComparisonFunction.LessEqual => CompareOp.LessOrEqual,
            ComparisonFunction.Greater => CompareOp.Greater,
            ComparisonFunction.NotEqual => CompareOp.NotEqual,
            ComparisonFunction.GreaterEqual => CompareOp.GreaterOrEqual,
            ComparisonFunction.Always => CompareOp.Always,
            _ => throw new ArgumentOutOfRangeException(nameof(comparisonKind))
        };
    }

    public static BorderColor GetBorderColor(SamplerBorderColor borderColor)
    {
        return borderColor switch
        {
            SamplerBorderColor.TransparentBlack => BorderColor.FloatTransparentBlack,
            SamplerBorderColor.OpaqueBlack => BorderColor.FloatOpaqueBlack,
            SamplerBorderColor.OpaqueWhite => BorderColor.FloatOpaqueWhite,
            _ => throw new ArgumentOutOfRangeException(nameof(borderColor))
        };
    }

    public static DescriptorType GetDescriptorType(ResourceType type, ElementOptions options)
    {
        bool dynamic = options.HasFlag(ElementOptions.DynamicBinding);

        return type switch
        {
            ResourceType.ConstantBuffer => dynamic ? DescriptorType.UniformBufferDynamic : DescriptorType.UniformBuffer,

            ResourceType.StructuredBuffer or
            ResourceType.StructuredBufferReadWrite => dynamic ? DescriptorType.StorageBufferDynamic : DescriptorType.StorageBuffer,

            ResourceType.Texture => DescriptorType.SampledImage,
            ResourceType.TextureReadWrite => DescriptorType.StorageImage,
            ResourceType.Sampler => DescriptorType.Sampler,
            ResourceType.AccelerationStructure => DescriptorType.AccelerationStructureKhr,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public static CullModeFlags GetCullModeFlags(CullMode cullMode)
    {
        return cullMode switch
        {
            CullMode.None => CullModeFlags.None,
            CullMode.Front => CullModeFlags.FrontBit,
            CullMode.Back => CullModeFlags.BackBit,
            _ => throw new ArgumentOutOfRangeException(nameof(cullMode))
        };
    }

    public static PolygonMode GetPolygonMode(FillMode fillMode)
    {
        return fillMode switch
        {
            FillMode.Solid => PolygonMode.Fill,
            FillMode.Wireframe => PolygonMode.Line,
            _ => throw new ArgumentOutOfRangeException(nameof(fillMode))
        };
    }

    public static VkFrontFace GetFrontFace(FrontFace frontFace)
    {
        return frontFace switch
        {
            FrontFace.CounterClockwise => VkFrontFace.CounterClockwise,
            FrontFace.Clockwise => VkFrontFace.Clockwise,
            _ => throw new ArgumentOutOfRangeException(nameof(frontFace))
        };
    }

    public static StencilOp GetStencilOp(StencilOperation operation)
    {
        return operation switch
        {
            StencilOperation.Keep => StencilOp.Keep,
            StencilOperation.Zero => StencilOp.Zero,
            StencilOperation.Replace => StencilOp.Replace,
            StencilOperation.IncrementAndClamp => StencilOp.IncrementAndClamp,
            StencilOperation.DecrementAndClamp => StencilOp.DecrementAndClamp,
            StencilOperation.Invert => StencilOp.Invert,
            StencilOperation.IncrementAndWrap => StencilOp.IncrementAndWrap,
            StencilOperation.DecrementAndWrap => StencilOp.DecrementAndWrap,
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
    }

    public static BlendFactor GetBlendFactor(Blend blend)
    {
        return blend switch
        {
            Blend.Zero => BlendFactor.Zero,
            Blend.One => BlendFactor.One,
            Blend.SourceAlpha => BlendFactor.SrcAlpha,
            Blend.InverseSourceAlpha => BlendFactor.OneMinusSrcAlpha,
            Blend.DestinationAlpha => BlendFactor.DstAlpha,
            Blend.InverseDestinationAlpha => BlendFactor.OneMinusDstAlpha,
            Blend.SourceColor => BlendFactor.SrcColor,
            Blend.InverseSourceColor => BlendFactor.OneMinusSrcColor,
            Blend.DestinationColor => BlendFactor.DstColor,
            Blend.InverseDestinationColor => BlendFactor.OneMinusDstColor,
            Blend.BlendFactor => BlendFactor.ConstantColor,
            Blend.InverseBlendFactor => BlendFactor.OneMinusConstantColor,
            _ => throw new ArgumentOutOfRangeException(nameof(blend))
        };
    }

    public static BlendOp GetBlendOp(BlendOperation operation)
    {
        return operation switch
        {
            BlendOperation.Add => BlendOp.Add,
            BlendOperation.Subtract => BlendOp.Subtract,
            BlendOperation.ReverseSubtract => BlendOp.ReverseSubtract,
            BlendOperation.Min => BlendOp.Min,
            BlendOperation.Max => BlendOp.Max,
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
    }

    public static ColorComponentFlags GetColorComponentFlags(ColorWriteChannels channels)
    {
        ColorComponentFlags flags = ColorComponentFlags.None;

        if (channels.HasFlag(ColorWriteChannels.Red))
        {
            flags |= ColorComponentFlags.RBit;
        }

        if (channels.HasFlag(ColorWriteChannels.Green))
        {
            flags |= ColorComponentFlags.GBit;
        }

        if (channels.HasFlag(ColorWriteChannels.Blue))
        {
            flags |= ColorComponentFlags.BBit;
        }

        if (channels.HasFlag(ColorWriteChannels.Alpha))
        {
            flags |= ColorComponentFlags.ABit;
        }

        return flags;
    }

    public static Format GetElementFormat(ElementFormat format)
    {
        return format switch
        {
            ElementFormat.UByte1 => Format.R8Uint,
            ElementFormat.UByte2 => Format.R8G8Uint,
            ElementFormat.UByte3 => Format.R8G8B8Uint,
            ElementFormat.UByte4 => Format.R8G8B8A8Uint,
            ElementFormat.Byte1 => Format.R8Sint,
            ElementFormat.Byte2 => Format.R8G8Sint,
            ElementFormat.Byte3 => Format.R8G8B8Sint,
            ElementFormat.Byte4 => Format.R8G8B8A8Sint,
            ElementFormat.UByte1Normalized => Format.R8Unorm,
            ElementFormat.UByte2Normalized => Format.R8G8Unorm,
            ElementFormat.UByte3Normalized => Format.R8G8B8Unorm,
            ElementFormat.UByte4Normalized => Format.R8G8B8A8Unorm,
            ElementFormat.Byte1Normalized => Format.R8SNorm,
            ElementFormat.Byte2Normalized => Format.R8G8SNorm,
            ElementFormat.Byte3Normalized => Format.R8G8B8SNorm,
            ElementFormat.Byte4Normalized => Format.R8G8B8A8SNorm,
            ElementFormat.UShort1 => Format.R16Uint,
            ElementFormat.UShort2 => Format.R16G16Uint,
            ElementFormat.UShort3 => Format.R16G16B16Uint,
            ElementFormat.UShort4 => Format.R16G16B16A16Uint,
            ElementFormat.Short1 => Format.R16Sint,
            ElementFormat.Short2 => Format.R16G16Sint,
            ElementFormat.Short3 => Format.R16G16B16Sint,
            ElementFormat.Short4 => Format.R16G16B16A16Sint,
            ElementFormat.UShort1Normalized => Format.R16Unorm,
            ElementFormat.UShort2Normalized => Format.R16G16Unorm,
            ElementFormat.UShort3Normalized => Format.R16G16B16Unorm,
            ElementFormat.UShort4Normalized => Format.R16G16B16A16Unorm,
            ElementFormat.Short1Normalized => Format.R16SNorm,
            ElementFormat.Short2Normalized => Format.R16G16SNorm,
            ElementFormat.Short3Normalized => Format.R16G16B16SNorm,
            ElementFormat.Short4Normalized => Format.R16G16B16A16SNorm,
            ElementFormat.Half1 => Format.R16Sfloat,
            ElementFormat.Half2 => Format.R16G16Sfloat,
            ElementFormat.Half3 => Format.R16G16B16Sfloat,
            ElementFormat.Half4 => Format.R16G16B16A16Sfloat,
            ElementFormat.Float1 => Format.R32Sfloat,
            ElementFormat.Float2 => Format.R32G32Sfloat,
            ElementFormat.Float3 => Format.R32G32B32Sfloat,
            ElementFormat.Float4 => Format.R32G32B32A32Sfloat,
            ElementFormat.UInt1 => Format.R32Uint,
            ElementFormat.UInt2 => Format.R32G32Uint,
            ElementFormat.UInt3 => Format.R32G32B32Uint,
            ElementFormat.UInt4 => Format.R32G32B32A32Uint,
            ElementFormat.Int1 => Format.R32Sint,
            ElementFormat.Int2 => Format.R32G32Sint,
            ElementFormat.Int3 => Format.R32G32B32Sint,
            ElementFormat.Int4 => Format.R32G32B32A32Sint,
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    public static VkPrimitiveTopology GetPrimitiveTopology(PrimitiveTopology topology)
    {
        return topology switch
        {
            PrimitiveTopology.PointList => VkPrimitiveTopology.PointList,
            PrimitiveTopology.LineList => VkPrimitiveTopology.LineList,
            PrimitiveTopology.LineStrip => VkPrimitiveTopology.LineStrip,
            PrimitiveTopology.TriangleList => VkPrimitiveTopology.TriangleList,
            PrimitiveTopology.TriangleStrip => VkPrimitiveTopology.TriangleStrip,
            PrimitiveTopology.LineListWithAdjacency => VkPrimitiveTopology.LineListWithAdjacency,
            PrimitiveTopology.LineStripWithAdjacency => VkPrimitiveTopology.LineStripWithAdjacency,
            PrimitiveTopology.TriangleListWithAdjacency => VkPrimitiveTopology.TriangleListWithAdjacency,
            PrimitiveTopology.TriangleStripWithAdjacency => VkPrimitiveTopology.TriangleStripWithAdjacency,
            PrimitiveTopology.PatchList => VkPrimitiveTopology.PatchList,
            _ => throw new ArgumentOutOfRangeException(nameof(topology))
        };
    }
}
