using Graphics.Core;
using Silk.NET.Vulkan;
using BlendFactor = Graphics.Core.BlendFactor;
using FrontFace = Graphics.Core.FrontFace;
using PrimitiveTopology = Graphics.Core.PrimitiveTopology;

namespace Graphics.Vulkan.Helpers;

internal static class Formats
{
    #region To Vulkan Format
    public static Format GetPixelFormat(PixelFormat pixelFormat, bool depthFormat)
    {
        return pixelFormat switch
        {
            PixelFormat.R8UNorm => Format.R8Unorm,
            PixelFormat.R8SNorm => Format.R8SNorm,
            PixelFormat.R8SInt => Format.R8Sint,

            PixelFormat.R16UNorm => Format.R16Unorm,
            PixelFormat.R16SNorm => Format.R16SNorm,
            PixelFormat.R16UInt => Format.R16Uint,
            PixelFormat.R16SInt => Format.R16Sint,
            PixelFormat.R16Float => Format.R16Sfloat,

            PixelFormat.R32UInt => Format.R32Uint,
            PixelFormat.R32SInt => Format.R32Sint,
            PixelFormat.R32Float => depthFormat ? Format.D32Sfloat : Format.R32Sfloat,

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
            PixelFormat.B8G8R8A8UNorm => Format.B8G8R8A8Unorm,
            PixelFormat.B8G8R8A8UNormSRgb => Format.B8G8R8A8Srgb,
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

            PixelFormat.D32FloatS8UInt => Format.D32SfloatS8Uint,
            PixelFormat.D24UNormS8UInt => Format.D24UnormS8Uint,

            PixelFormat.R10G10B10A2UNorm => Format.A2B10G10R10UnormPack32,
            PixelFormat.R10G10B10A2UInt => Format.A2B10G10R10UintPack32,
            PixelFormat.R11G11B10Float => Format.B10G11R11UfloatPack32,

            _ => throw new ArgumentOutOfRangeException(nameof(pixelFormat))
        };
    }

    public static ImageType GetImageType(TextureType textureType)
    {
        return textureType switch
        {
            TextureType.Texture1D => ImageType.Type1D,
            TextureType.Texture2D => ImageType.Type2D,
            TextureType.Texture3D => ImageType.Type3D,
            _ => throw new ArgumentOutOfRangeException(nameof(textureType))
        };
    }

    public static ImageUsageFlags GetImageUsageFlags(TextureUsage textureUsage)
    {
        ImageUsageFlags imageUsageFlags = ImageUsageFlags.TransferDstBit | ImageUsageFlags.TransferSrcBit;

        if (textureUsage.HasFlag(TextureUsage.RenderTarget))
        {
            imageUsageFlags |= ImageUsageFlags.ColorAttachmentBit;
        }

        if (textureUsage.HasFlag(TextureUsage.DepthStencil))
        {
            imageUsageFlags |= ImageUsageFlags.DepthStencilAttachmentBit;
        }

        if (textureUsage.HasFlag(TextureUsage.Sampled))
        {
            imageUsageFlags |= ImageUsageFlags.SampledBit;
        }

        if (textureUsage.HasFlag(TextureUsage.Storage))
        {
            imageUsageFlags |= ImageUsageFlags.StorageBit;
        }

        return imageUsageFlags;
    }

    public static SampleCountFlags GetSampleCount(TextureSampleCount sampleCount)
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

    public static DescriptorType GetDescriptorType(ResourceKind kind, ElementOptions options)
    {
        bool dynamic = options.HasFlag(ElementOptions.DynamicBinding);

        return kind switch
        {
            ResourceKind.ConstantBuffer => dynamic ? DescriptorType.UniformBufferDynamic : DescriptorType.UniformBuffer,
            ResourceKind.StorageBuffer => dynamic ? DescriptorType.StorageBufferDynamic : DescriptorType.StorageBuffer,
            ResourceKind.SampledImage => DescriptorType.SampledImage,
            ResourceKind.StorageImage => DescriptorType.StorageImage,
            ResourceKind.Sampler => DescriptorType.Sampler,
            ResourceKind.AccelerationStructure => DescriptorType.AccelerationStructureKhr,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
    }

    public static ShaderStageFlags GetShaderStageFlags(ShaderStages stages)
    {
        ShaderStageFlags shaderStageFlags = ShaderStageFlags.None;

        if (stages.HasFlag(ShaderStages.Vertex))
        {
            shaderStageFlags |= ShaderStageFlags.VertexBit;
        }

        if (stages.HasFlag(ShaderStages.TessellationControl))
        {
            shaderStageFlags |= ShaderStageFlags.TessellationControlBit;
        }

        if (stages.HasFlag(ShaderStages.TessellationEvaluation))
        {
            shaderStageFlags |= ShaderStageFlags.TessellationEvaluationBit;
        }

        if (stages.HasFlag(ShaderStages.Geometry))
        {
            shaderStageFlags |= ShaderStageFlags.GeometryBit;
        }

        if (stages.HasFlag(ShaderStages.Pixel))
        {
            shaderStageFlags |= ShaderStageFlags.FragmentBit;
        }

        if (stages.HasFlag(ShaderStages.Compute))
        {
            shaderStageFlags |= ShaderStageFlags.ComputeBit;
        }

        if (stages.HasFlag(ShaderStages.RayGeneration))
        {
            shaderStageFlags |= ShaderStageFlags.RaygenBitKhr;
        }

        if (stages.HasFlag(ShaderStages.AnyHit))
        {
            shaderStageFlags |= ShaderStageFlags.AnyHitBitKhr;
        }

        if (stages.HasFlag(ShaderStages.ClosestHit))
        {
            shaderStageFlags |= ShaderStageFlags.ClosestHitBitKhr;
        }

        if (stages.HasFlag(ShaderStages.Miss))
        {
            shaderStageFlags |= ShaderStageFlags.MissBitKhr;
        }

        if (stages.HasFlag(ShaderStages.Intersection))
        {
            shaderStageFlags |= ShaderStageFlags.IntersectionBitKhr;
        }

        if (stages.HasFlag(ShaderStages.Callable))
        {
            shaderStageFlags |= ShaderStageFlags.CallableBitKhr;
        }

        return shaderStageFlags;
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

    public static CompareOp GetCompareOp(ComparisonKind comparisonKind)
    {
        return comparisonKind switch
        {
            ComparisonKind.Never => CompareOp.Never,
            ComparisonKind.Less => CompareOp.Less,
            ComparisonKind.Equal => CompareOp.Equal,
            ComparisonKind.LessEqual => CompareOp.LessOrEqual,
            ComparisonKind.Greater => CompareOp.Greater,
            ComparisonKind.NotEqual => CompareOp.NotEqual,
            ComparisonKind.GreaterEqual => CompareOp.GreaterOrEqual,
            ComparisonKind.Always => CompareOp.Always,
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

    public static VkBlendFactor GetBlendFactor(BlendFactor blendFactor)
    {
        return blendFactor switch
        {
            BlendFactor.Zero => VkBlendFactor.Zero,
            BlendFactor.One => VkBlendFactor.One,
            BlendFactor.SourceAlpha => VkBlendFactor.SrcAlpha,
            BlendFactor.InverseSourceAlpha => VkBlendFactor.OneMinusSrcAlpha,
            BlendFactor.DestinationAlpha => VkBlendFactor.DstAlpha,
            BlendFactor.InverseDestinationAlpha => VkBlendFactor.OneMinusDstAlpha,
            BlendFactor.SourceColor => VkBlendFactor.SrcColor,
            BlendFactor.InverseSourceColor => VkBlendFactor.OneMinusSrcColor,
            BlendFactor.DestinationColor => VkBlendFactor.DstColor,
            BlendFactor.InverseDestinationColor => VkBlendFactor.OneMinusDstColor,
            BlendFactor.BlendFactor => VkBlendFactor.ConstantColor,
            BlendFactor.InverseBlendFactor => VkBlendFactor.OneMinusConstantColor,
            _ => throw new ArgumentOutOfRangeException(nameof(blendFactor))
        };
    }

    public static BlendOp GetBlendOp(BlendFunction blendFunction)
    {
        return blendFunction switch
        {
            BlendFunction.Add => BlendOp.Add,
            BlendFunction.Subtract => BlendOp.Subtract,
            BlendFunction.ReverseSubtract => BlendOp.ReverseSubtract,
            BlendFunction.Minimum => BlendOp.Min,
            BlendFunction.Maximum => BlendOp.Max,
            _ => throw new ArgumentOutOfRangeException(nameof(blendFunction))
        };
    }

    public static ColorComponentFlags GetColorWriteMask(ColorWriteMask colorWriteMask)
    {
        ColorComponentFlags colorComponentFlags = ColorComponentFlags.None;

        if (colorWriteMask.HasFlag(ColorWriteMask.Red))
        {
            colorComponentFlags |= ColorComponentFlags.RBit;
        }

        if (colorWriteMask.HasFlag(ColorWriteMask.Green))
        {
            colorComponentFlags |= ColorComponentFlags.GBit;
        }

        if (colorWriteMask.HasFlag(ColorWriteMask.Blue))
        {
            colorComponentFlags |= ColorComponentFlags.BBit;
        }

        if (colorWriteMask.HasFlag(ColorWriteMask.Alpha))
        {
            colorComponentFlags |= ColorComponentFlags.ABit;
        }

        return colorComponentFlags;
    }

    public static PolygonMode GetPolygonMode(PolygonFillMode fillMode)
    {
        return fillMode switch
        {
            PolygonFillMode.Solid => PolygonMode.Fill,
            PolygonFillMode.Wireframe => PolygonMode.Line,
            _ => throw new ArgumentOutOfRangeException(nameof(fillMode))
        };
    }

    public static CullModeFlags GetCullMode(FaceCullMode cullMode)
    {
        return cullMode switch
        {
            FaceCullMode.None => CullModeFlags.None,
            FaceCullMode.Back => CullModeFlags.BackBit,
            FaceCullMode.Front => CullModeFlags.FrontBit,
            _ => throw new ArgumentOutOfRangeException(nameof(cullMode))
        };
    }

    public static VkFrontFace GetFrontFace(FrontFace frontFace)
    {
        return frontFace switch
        {
            FrontFace.Clockwise => VkFrontFace.Clockwise,
            FrontFace.CounterClockwise => VkFrontFace.CounterClockwise,
            _ => throw new ArgumentOutOfRangeException(nameof(frontFace))
        };
    }

    public static StencilOp GetStencilOp(StencilOperation fail)
    {
        return fail switch
        {
            StencilOperation.Keep => StencilOp.Keep,
            StencilOperation.Zero => StencilOp.Zero,
            StencilOperation.Replace => StencilOp.Replace,
            StencilOperation.IncrementAndClamp => StencilOp.IncrementAndClamp,
            StencilOperation.IncrementAndWrap => StencilOp.IncrementAndWrap,
            StencilOperation.DecrementAndClamp => StencilOp.DecrementAndClamp,
            StencilOperation.DecrementAndWrap => StencilOp.DecrementAndWrap,
            StencilOperation.Invert => StencilOp.Invert,
            _ => throw new ArgumentOutOfRangeException(nameof(fail))
        };
    }

    public static VkPrimitiveTopology GetPrimitiveTopology(PrimitiveTopology primitiveTopology)
    {
        return primitiveTopology switch
        {
            PrimitiveTopology.PointList => VkPrimitiveTopology.PointList,
            PrimitiveTopology.LineList => VkPrimitiveTopology.LineList,
            PrimitiveTopology.LineStrip => VkPrimitiveTopology.LineStrip,
            PrimitiveTopology.TriangleList => VkPrimitiveTopology.TriangleList,
            PrimitiveTopology.TriangleStrip => VkPrimitiveTopology.TriangleStrip,
            _ => throw new ArgumentOutOfRangeException(nameof(primitiveTopology))
        };
    }

    public static Format GetVertexElementFormat(VertexElementFormat format)
    {
        return format switch
        {
            VertexElementFormat.Float1 => Format.R32Sfloat,
            VertexElementFormat.Float2 => Format.R32G32Sfloat,
            VertexElementFormat.Float3 => Format.R32G32B32Sfloat,
            VertexElementFormat.Float4 => Format.R32G32B32A32Sfloat,
            VertexElementFormat.Byte2Norm => Format.R8G8Unorm,
            VertexElementFormat.Byte2 => Format.R8G8Uint,
            VertexElementFormat.Byte4Norm => Format.R8G8B8A8Unorm,
            VertexElementFormat.Byte4 => Format.R8G8B8A8Uint,
            VertexElementFormat.SByte2Norm => Format.R8G8SNorm,
            VertexElementFormat.SByte2 => Format.R8G8Sint,
            VertexElementFormat.SByte4Norm => Format.R8G8B8A8SNorm,
            VertexElementFormat.SByte4 => Format.R8G8B8A8Sint,
            VertexElementFormat.UShort2Norm => Format.R16G16Unorm,
            VertexElementFormat.UShort2 => Format.R16G16Uint,
            VertexElementFormat.UShort4Norm => Format.R16G16B16A16Unorm,
            VertexElementFormat.UShort4 => Format.R16G16B16A16Uint,
            VertexElementFormat.Short2Norm => Format.R16G16SNorm,
            VertexElementFormat.Short2 => Format.R16G16Sint,
            VertexElementFormat.Short4Norm => Format.R16G16B16A16SNorm,
            VertexElementFormat.Short4 => Format.R16G16B16A16Sint,
            VertexElementFormat.UInt1 => Format.R32Uint,
            VertexElementFormat.UInt2 => Format.R32G32Uint,
            VertexElementFormat.UInt3 => Format.R32G32B32Uint,
            VertexElementFormat.UInt4 => Format.R32G32B32A32Uint,
            VertexElementFormat.Int1 => Format.R32Sint,
            VertexElementFormat.Int2 => Format.R32G32Sint,
            VertexElementFormat.Int3 => Format.R32G32B32Sint,
            VertexElementFormat.Int4 => Format.R32G32B32A32Sint,
            VertexElementFormat.Half1 => Format.R16Sfloat,
            VertexElementFormat.Half2 => Format.R16G16Sfloat,
            VertexElementFormat.Half4 => Format.R16G16B16A16Sfloat,
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    public static ShaderStageFlags GetShaderStage(ShaderStages stage)
    {
        ShaderStageFlags shaderStageFlags = ShaderStageFlags.None;

        if (stage.HasFlag(ShaderStages.Vertex))
        {
            shaderStageFlags |= ShaderStageFlags.VertexBit;
        }

        if (stage.HasFlag(ShaderStages.TessellationControl))
        {
            shaderStageFlags |= ShaderStageFlags.TessellationControlBit;
        }

        if (stage.HasFlag(ShaderStages.TessellationEvaluation))
        {
            shaderStageFlags |= ShaderStageFlags.TessellationEvaluationBit;
        }

        if (stage.HasFlag(ShaderStages.Geometry))
        {
            shaderStageFlags |= ShaderStageFlags.GeometryBit;
        }

        if (stage.HasFlag(ShaderStages.Pixel))
        {
            shaderStageFlags |= ShaderStageFlags.FragmentBit;
        }

        if (stage.HasFlag(ShaderStages.Compute))
        {
            shaderStageFlags |= ShaderStageFlags.ComputeBit;
        }

        if (stage.HasFlag(ShaderStages.RayGeneration))
        {
            shaderStageFlags |= ShaderStageFlags.RaygenBitKhr;
        }

        if (stage.HasFlag(ShaderStages.AnyHit))
        {
            shaderStageFlags |= ShaderStageFlags.AnyHitBitKhr;
        }

        if (stage.HasFlag(ShaderStages.ClosestHit))
        {
            shaderStageFlags |= ShaderStageFlags.ClosestHitBitKhr;
        }

        if (stage.HasFlag(ShaderStages.Miss))
        {
            shaderStageFlags |= ShaderStageFlags.MissBitKhr;
        }

        if (stage.HasFlag(ShaderStages.Intersection))
        {
            shaderStageFlags |= ShaderStageFlags.IntersectionBitKhr;
        }

        if (stage.HasFlag(ShaderStages.Callable))
        {
            shaderStageFlags |= ShaderStageFlags.CallableBitKhr;
        }

        return shaderStageFlags;
    }

    public static IndexType GetIndexType(IndexFormat indexFormat)
    {
        return indexFormat switch
        {
            IndexFormat.U16 => IndexType.Uint16,
            IndexFormat.U32 => IndexType.Uint32,
            _ => throw new ArgumentOutOfRangeException(nameof(indexFormat))
        };
    }

    public static GeometryFlagsKHR GetGeometryFlags(AccelStructGeometryOptions options)
    {
        GeometryFlagsKHR geometryFlags = GeometryFlagsKHR.None;

        if (options.HasFlag(AccelStructGeometryOptions.Opaque))
        {
            geometryFlags |= GeometryFlagsKHR.OpaqueBitKhr;
        }

        if (options.HasFlag(AccelStructGeometryOptions.NoDuplicateAnyHitInvocation))
        {
            geometryFlags |= GeometryFlagsKHR.NoDuplicateAnyHitInvocationBitKhr;
        }

        return geometryFlags;
    }

    public static GeometryInstanceFlagsKHR GetGeometryInstanceFlags(AccelStructInstanceOptions options)
    {
        GeometryInstanceFlagsKHR geometryInstanceFlags = GeometryInstanceFlagsKHR.None;

        if (options.HasFlag(AccelStructInstanceOptions.TriangleCullDisable))
        {
            geometryInstanceFlags |= GeometryInstanceFlagsKHR.TriangleFacingCullDisableBitKhr;
        }

        if (options.HasFlag(AccelStructInstanceOptions.TriangleFrontCounterClockwise))
        {
            geometryInstanceFlags |= GeometryInstanceFlagsKHR.TriangleFrontCounterclockwiseBitKhr;
        }

        if (options.HasFlag(AccelStructInstanceOptions.ForceOpaque))
        {
            geometryInstanceFlags |= GeometryInstanceFlagsKHR.ForceOpaqueBitKhr;
        }

        if (options.HasFlag(AccelStructInstanceOptions.ForceNoOpaque))
        {
            geometryInstanceFlags |= GeometryInstanceFlagsKHR.ForceNoOpaqueBitKhr;
        }

        return geometryInstanceFlags;
    }

    public static BuildAccelerationStructureFlagsKHR GetBuildAccelerationStructureFlags(AccelStructBuildOptions options)
    {
        BuildAccelerationStructureFlagsKHR buildAccelerationStructureFlags = BuildAccelerationStructureFlagsKHR.None;

        if (options.HasFlag(AccelStructBuildOptions.AllowUpdate) || options.HasFlag(AccelStructBuildOptions.PerformUpdate))
        {
            buildAccelerationStructureFlags |= BuildAccelerationStructureFlagsKHR.AllowUpdateBitKhr;
        }

        if (options.HasFlag(AccelStructBuildOptions.AllowCompactation))
        {
            buildAccelerationStructureFlags |= BuildAccelerationStructureFlagsKHR.AllowCompactionBitKhr;
        }

        if (options.HasFlag(AccelStructBuildOptions.PreferFastTrace))
        {
            buildAccelerationStructureFlags |= BuildAccelerationStructureFlagsKHR.PreferFastTraceBitKhr;
        }

        if (options.HasFlag(AccelStructBuildOptions.PreferFastBuild))
        {
            buildAccelerationStructureFlags |= BuildAccelerationStructureFlagsKHR.PreferFastBuildBitKhr;
        }

        if (options.HasFlag(AccelStructBuildOptions.MinimizeMemory))
        {
            buildAccelerationStructureFlags |= BuildAccelerationStructureFlagsKHR.LowMemoryBitKhr;
        }

        return buildAccelerationStructureFlags;
    }
    #endregion

    #region From Vulkan Format
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
            Format.D32Sfloat => PixelFormat.R32Float,

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
            Format.R8G8B8A8Srgb => PixelFormat.R8G8B8A8UNormSRgb,
            Format.B8G8R8A8Unorm => PixelFormat.B8G8R8A8UNorm,
            Format.B8G8R8A8Srgb => PixelFormat.B8G8R8A8UNormSRgb,
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

            Format.Etc2R8G8B8UnormBlock => PixelFormat.ETC2R8G8B8UNorm,
            Format.Etc2R8G8B8A1UnormBlock => PixelFormat.ETC2R8G8B8A1UNorm,
            Format.Etc2R8G8B8A8UnormBlock => PixelFormat.ETC2R8G8B8A8UNorm,

            Format.D32SfloatS8Uint => PixelFormat.D32FloatS8UInt,
            Format.D24UnormS8Uint => PixelFormat.D24UNormS8UInt,

            Format.A2B10G10R10UnormPack32 => PixelFormat.R10G10B10A2UNorm,
            Format.A2B10G10R10UintPack32 => PixelFormat.R10G10B10A2UInt,
            Format.B10G11R11UfloatPack32 => PixelFormat.R11G11B10Float,

            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }
    #endregion
}