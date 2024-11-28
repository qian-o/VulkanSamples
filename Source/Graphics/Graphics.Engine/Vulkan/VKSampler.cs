using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKSampler : Sampler
{
    public VKSampler(Context context,
                     ref readonly SamplerDesc desc) : base(context, in desc)
    {
        Formats.GetFilter(desc.Filter,
                          out Filter minFilter,
                          out Filter magFilter,
                          out SamplerMipmapMode mipFilter);

        bool compareEnable = desc.ComparisonFunction.HasValue;

        SamplerCreateInfo createInfo = new()
        {
            SType = StructureType.SamplerCreateInfo,
            MagFilter = magFilter,
            MinFilter = minFilter,
            MipmapMode = mipFilter,
            AddressModeU = Formats.GetSamplerAddressMode(desc.AddressModeU),
            AddressModeV = Formats.GetSamplerAddressMode(desc.AddressModeV),
            AddressModeW = Formats.GetSamplerAddressMode(desc.AddressModeW),
            CompareEnable = compareEnable,
            CompareOp = compareEnable ? Formats.GetCompareOp(desc.ComparisonFunction!.Value) : CompareOp.Never,
            AnisotropyEnable = desc.Filter == SamplerFilter.Anisotropic,
            MaxAnisotropy = desc.MaximumAnisotropy,
            MinLod = desc.MinimumLod,
            MaxLod = desc.MaximumLod,
            MipLodBias = desc.LodBias,
            BorderColor = Formats.GetBorderColor(desc.BorderColor)
        };

        VkSampler sampler;
        Context.Vk.CreateSampler(Context.Device, &createInfo, null, &sampler).ThrowCode();

        Sampler = sampler;
    }

    public new VKContext Context => (VKContext)base.Context;

    public VkSampler Sampler { get; }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.Sampler, Sampler.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroySampler(Context.Device, Sampler, null);
    }
}
