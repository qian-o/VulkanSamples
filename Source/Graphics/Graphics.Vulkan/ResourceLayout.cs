using Graphics.Core.Helpers;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class ResourceLayout : VulkanObject<VkDescriptorSetLayout>
{
    internal ResourceLayout(VulkanResources vkRes, ref readonly ResourceLayoutDescription description) : base(vkRes, ObjectType.DescriptorSetLayout)
    {
        DescriptorSetLayoutBinding[] bindings = new DescriptorSetLayoutBinding[description.Elements.Length];
        DescriptorType[] descriptorTypes = new DescriptorType[description.Elements.Length];
        uint maxDescriptorCount = 0;

        for (uint i = 0; i < description.Elements.Length; i++)
        {
            ElementDescription element = description.Elements[i];

            DescriptorSetLayoutBinding binding = new()
            {
                Binding = i,
                DescriptorType = Formats.GetDescriptorType(element.Kind, element.Options),
                DescriptorCount = 1,
                StageFlags = Formats.GetShaderStageFlags(element.Stages)
            };

            bindings[i] = binding;
            descriptorTypes[i] = binding.DescriptorType;
        }

        if (description.IsLastBindless)
        {
            DescriptorSetLayoutBinding binding = bindings[^1];

            if (binding.DescriptorType == DescriptorType.UniformBuffer)
            {
                binding.DescriptorCount = VkRes.DescriptorIndexingProperties.MaxDescriptorSetUpdateAfterBindUniformBuffers;
            }
            else if (binding.DescriptorType == DescriptorType.UniformBufferDynamic)
            {
                binding.DescriptorCount = VkRes.DescriptorIndexingProperties.MaxDescriptorSetUpdateAfterBindUniformBuffersDynamic;
            }
            else if (binding.DescriptorType == DescriptorType.StorageBuffer)
            {
                binding.DescriptorCount = VkRes.DescriptorIndexingProperties.MaxDescriptorSetUpdateAfterBindStorageBuffers;
            }
            else if (binding.DescriptorType == DescriptorType.StorageBufferDynamic)
            {
                binding.DescriptorCount = VkRes.DescriptorIndexingProperties.MaxDescriptorSetUpdateAfterBindStorageBuffersDynamic;
            }
            else if (binding.DescriptorType == DescriptorType.SampledImage)
            {
                binding.DescriptorCount = VkRes.DescriptorIndexingProperties.MaxDescriptorSetUpdateAfterBindSampledImages;
            }
            else if (binding.DescriptorType == DescriptorType.StorageImage)
            {
                binding.DescriptorCount = VkRes.DescriptorIndexingProperties.MaxDescriptorSetUpdateAfterBindStorageImages;
            }
            else if (binding.DescriptorType == DescriptorType.Sampler)
            {
                binding.DescriptorCount = VkRes.DescriptorIndexingProperties.MaxDescriptorSetUpdateAfterBindSamplers;
            }
            else
            {
                throw new NotSupportedException("The descriptor type is not supported for bindless resource layout.");
            }

            binding.DescriptorCount = maxDescriptorCount = Math.Min(binding.DescriptorCount, description.MaxDescriptorCount);

            bindings[^1] = binding;
        }

        DescriptorSetLayoutCreateInfo createInfo = new()
        {
            SType = StructureType.DescriptorSetLayoutCreateInfo,
            BindingCount = (uint)bindings.Length,
            PBindings = bindings.AsPointer()
        };

        if (VkRes.DescriptorBufferSupported)
        {
            createInfo.Flags |= DescriptorSetLayoutCreateFlags.DescriptorBufferBitExt;
        }

        if (description.IsLastBindless)
        {
            DescriptorBindingFlags[] descriptorBindings = new DescriptorBindingFlags[bindings.Length];
            Array.Fill(descriptorBindings, DescriptorBindingFlags.None);

            descriptorBindings[^1] = DescriptorBindingFlags.VariableDescriptorCountBit;

            createInfo.AddNext(out DescriptorSetLayoutBindingFlagsCreateInfo bindingFlagsCreateInfo);

            bindingFlagsCreateInfo.BindingCount = (uint)bindings.Length;
            bindingFlagsCreateInfo.PBindingFlags = descriptorBindings.AsPointer();
        }

        VkDescriptorSetLayout descriptorSetLayout;
        VkRes.Vk.CreateDescriptorSetLayout(VkRes.VkDevice, &createInfo, null, &descriptorSetLayout).ThrowCode();

        ulong sizeInBytes;
        if (VkRes.DescriptorBufferSupported)
        {
            VkRes.ExtDescriptorBuffer.GetDescriptorSetLayoutSize(VkRes.VkDevice, descriptorSetLayout, &sizeInBytes);

            sizeInBytes = Util.AlignedSize(sizeInBytes, VkRes.DescriptorBufferProperties.DescriptorBufferOffsetAlignment);
        }
        else
        {
            sizeInBytes = 0;
        }

        Handle = descriptorSetLayout;
        DescriptorTypes = descriptorTypes;
        SizeInBytes = (uint)sizeInBytes;
        IsLastBindless = description.IsLastBindless;
        MaxDescriptorCount = maxDescriptorCount;
    }

    internal override VkDescriptorSetLayout Handle { get; }

    internal DescriptorType[] DescriptorTypes { get; }

    internal uint SizeInBytes { get; }

    internal bool IsLastBindless { get; }

    internal uint MaxDescriptorCount { get; }

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    internal override void DestroyObject()
    {
        VkRes.Vk.DestroyDescriptorSetLayout(VkRes.VkDevice, Handle, null);
    }
}
