using Graphics.Core;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Sampler : VulkanObject<VkSampler>, IBindableResource
{
    internal Sampler(VulkanResources vkRes, ref readonly SamplerDescription description) : base(vkRes, ObjectType.Sampler)
    {
        Formats.GetFilter(description.Filter, out Filter minFilter, out Filter magFilter, out SamplerMipmapMode mipFilter);

        SamplerCreateInfo samplerCreateInfo = new()
        {
            SType = StructureType.SamplerCreateInfo,
            MagFilter = magFilter,
            MinFilter = minFilter,
            MipmapMode = mipFilter,
            AddressModeU = Formats.GetSamplerAddressMode(description.AddressModeU),
            AddressModeV = Formats.GetSamplerAddressMode(description.AddressModeV),
            AddressModeW = Formats.GetSamplerAddressMode(description.AddressModeW),
            CompareEnable = description.ComparisonKind.HasValue,
            CompareOp = description.ComparisonKind.HasValue ? Formats.GetCompareOp(description.ComparisonKind.Value) : CompareOp.Never,
            AnisotropyEnable = description.Filter == SamplerFilter.Anisotropic,
            MaxAnisotropy = description.MaximumAnisotropy,
            MinLod = description.MinimumLod,
            MaxLod = description.MaximumLod,
            MipLodBias = description.LodBias,
            BorderColor = Formats.GetBorderColor(description.BorderColor),
        };

        VkSampler sampler;
        VkRes.Vk.CreateSampler(VkRes.VkDevice, &samplerCreateInfo, null, &sampler).ThrowCode();

        Handle = sampler;
    }

    internal override VkSampler Handle { get; }

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    internal override void DestroyObject()
    {
        VkRes.Vk.DestroySampler(VkRes.VkDevice, Handle, null);
    }
}
