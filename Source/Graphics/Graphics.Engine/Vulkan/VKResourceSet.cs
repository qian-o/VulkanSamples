using Graphics.Core.Helpers;
using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKResourceSet : ResourceSet
{
    public VKResourceSet(Context context,
                         ref readonly ResourceSetDesc desc) : base(context, in desc)
    {
        VKResourceLayout layout = desc.Layout.VK();
        VkDescriptorSetLayout vkLayout = layout.DescriptorSetLayout;

        DescriptorPoolSize[] poolSizes = new DescriptorPoolSize[layout.Desc.Elements.Length];

        for (uint i = 0; i < layout.Desc.Elements.Length; i++)
        {
            LayoutElementDesc element = layout.Desc.Elements[i];

            DescriptorPoolSize poolSize = new()
            {
                Type = Formats.GetDescriptorType(element.Type, element.Options),
                DescriptorCount = 1
            };

            poolSizes[i] = poolSize;
        }

        DescriptorPoolCreateInfo createInfo = new()
        {
            SType = StructureType.DescriptorPoolCreateInfo,
            MaxSets = 1,
            PoolSizeCount = (uint)poolSizes.Length,
            PPoolSizes = poolSizes.AsPointer()
        };

        VkDescriptorPool descriptorPool;
        Context.Vk.CreateDescriptorPool(Context.Device, &createInfo, null, &descriptorPool).ThrowCode();

        DescriptorSetAllocateInfo allocateInfo = new()
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = descriptorPool,
            DescriptorSetCount = 1,
            PSetLayouts = &vkLayout
        };

        VkDescriptorSet descriptorSet;
        Context.Vk.AllocateDescriptorSets(Context.Device, &allocateInfo, &descriptorSet).ThrowCode();

        WriteDescriptorSet[] writes = new WriteDescriptorSet[desc.Resources.Length];

        for (uint i = 0; i < writes.Length; i++)
        {
            LayoutElementDesc element = layout.Desc.Elements[i];
            DeviceResource resource = desc.Resources[i];

            WriteDescriptorSet write = new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = descriptorSet,
                DstBinding = VKHelpers.GetBinding(element),
                DescriptorCount = 1,
                DescriptorType = Formats.GetDescriptorType(element.Type, element.Options)
            };

            if (resource is VKBuffer buffer)
            {
                DescriptorBufferInfo bufferInfo = new()
                {
                    Buffer = buffer.Buffer,
                    Offset = 0,
                    Range = element.Size == 0 ? Vk.WholeSize : element.Size
                };

                write.PBufferInfo = &bufferInfo;
            }
            else if (resource is VKTextureView textureView)
            {
                DescriptorImageInfo imageInfo = new()
                {
                    ImageView = textureView.ImageView,
                    ImageLayout = element.Type == ResourceType.Texture ? ImageLayout.ShaderReadOnlyOptimal : ImageLayout.General
                };

                write.PImageInfo = &imageInfo;
            }
            else if (resource is VKSampler sampler)
            {
                DescriptorImageInfo imageInfo = new()
                {
                    Sampler = sampler.Sampler
                };

                write.PImageInfo = &imageInfo;
            }
            else
            {
                throw new NotSupportedException("Resource type not supported.");
            }
        }

        Context.Vk.UpdateDescriptorSets(Context.Device, (uint)writes.Length, writes.AsPointer(), 0, null);

        DescriptorPool = descriptorPool;
        DescriptorSet = descriptorSet;
        StorageTextures = desc.Resources.Where((res, index) => layout.Desc.Elements[index].Type == ResourceType.TextureReadWrite)
                                        .Cast<VKTexture>()
                                        .ToArray();
        Textures = desc.Resources.Where((res, index) => layout.Desc.Elements[index].Type == ResourceType.Texture)
                                 .Cast<VKTexture>()
                                 .ToArray();
    }

    public new VKContext Context => (VKContext)base.Context;

    public VkDescriptorPool DescriptorPool { get; }

    public VkDescriptorSet DescriptorSet { get; }

    public VKTexture[] StorageTextures { get; }

    public VKTexture[] Textures { get; }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.DescriptorSet, DescriptorSet.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyDescriptorPool(Context.Device, DescriptorPool, null);
    }
}
