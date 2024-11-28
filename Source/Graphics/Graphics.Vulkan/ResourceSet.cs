using Graphics.Core.Helpers;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class ResourceSet : VulkanObject<ulong>
{
    private readonly DeviceBuffer? descriptorBuffer;
    private readonly VkDescriptorPool descriptorPool;
    private readonly VkDescriptorSet descriptorSet;

    private readonly IBindableResource[]? useResources;

    private readonly List<Texture> sampledTextures = [];
    private readonly List<Texture> storageTextures = [];

    private IBindableResource[]? bindlessResources;

    internal ResourceSet(VulkanResources vkRes, ref readonly ResourceSetDescription description) : base(vkRes)
    {
        Layout = description.Layout;

        if (VkRes.DescriptorBufferSupported)
        {
            const BufferUsageFlags bufferUsageFlags = BufferUsageFlags.TransferDstBit
                                                      | BufferUsageFlags.ResourceDescriptorBufferBitExt
                                                      | BufferUsageFlags.SamplerDescriptorBufferBitExt;

            descriptorBuffer = new(VkRes, bufferUsageFlags, Layout.SizeInBytes, true);

            Handle = descriptorBuffer.Address;
        }
        else
        {
            DescriptorPoolSize[] poolSizes = new DescriptorPoolSize[Layout.DescriptorTypes.Length];

            for (uint i = 0; i < Layout.DescriptorTypes.Length; i++)
            {
                DescriptorType type = Layout.DescriptorTypes[i];

                DescriptorPoolSize poolSize = new()
                {
                    Type = type,
                    DescriptorCount = 1
                };

                poolSizes[i] = poolSize;
            }

            if (Layout.IsLastBindless)
            {
                poolSizes[^1].DescriptorCount = Layout.MaxDescriptorCount;
            }

            DescriptorPoolCreateInfo poolCreateInfo = new()
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                MaxSets = 1,
                PoolSizeCount = (uint)poolSizes.Length,
                PPoolSizes = poolSizes.AsPointer(),
                Flags = DescriptorPoolCreateFlags.FreeDescriptorSetBit
            };

            VkRes.Vk.CreateDescriptorPool(VkRes.VkDevice, &poolCreateInfo, null, out descriptorPool).ThrowCode();

            VkDescriptorSetLayout descriptorSetLayout = Layout.Handle;

            DescriptorSetAllocateInfo allocateInfo = new()
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = descriptorPool,
                DescriptorSetCount = 1,
                PSetLayouts = &descriptorSetLayout,
            };

            if (Layout.IsLastBindless)
            {
                allocateInfo.AddNext(out DescriptorSetVariableDescriptorCountAllocateInfo variableDescriptorCountAllocateInfo);

                variableDescriptorCountAllocateInfo.DescriptorSetCount = 1;
                variableDescriptorCountAllocateInfo.PDescriptorCounts = Alloter.Alloc(Layout.MaxDescriptorCount);
            }

            VkRes.Vk.AllocateDescriptorSets(VkRes.VkDevice, &allocateInfo, out descriptorSet).ThrowCode();

            Handle = descriptorSet.Handle;
        }

        int resourceCount = Layout.IsLastBindless ? Layout.DescriptorTypes.Length - 1 : Layout.DescriptorTypes.Length;

        useResources = new IBindableResource[resourceCount];

        for (uint i = 0; i < description.BoundResources.Length; i++)
        {
            IBindableResource? bindableResource = description.BoundResources[i];

            if (bindableResource is not null)
            {
                useResources[i] = bindableResource;
            }
        }

        Refresh();
    }

    internal override ulong Handle { get; }

    internal ResourceLayout Layout { get; }

    internal IReadOnlyList<Texture> SampledTextures => sampledTextures;

    internal IReadOnlyList<Texture> StorageTextures => storageTextures;

    public void UpdateSet(IBindableResource bindableResource, uint index)
    {
        if (index >= Layout.DescriptorTypes.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (Layout.IsLastBindless && index >= Layout.DescriptorTypes.Length - 1)
        {
            throw new InvalidOperationException("Resource layout is bindless.");
        }

        useResources![index] = bindableResource;

        Refresh();
    }

    public void UpdateBindless(params IBindableResource[] bindableResources)
    {
        if (!Layout.IsLastBindless)
        {
            throw new InvalidOperationException("Resource layout is not bindless.");
        }

        if (bindableResources.Length > Layout.MaxDescriptorCount)
        {
            throw new InvalidDataException("Resource count exceeds the maximum descriptor count.");
        }

        bindlessResources = bindableResources;

        Refresh();
    }

    internal override ulong[] GetHandles()
    {
        return [];
    }

    internal override void DestroyObject()
    {
        if (VkRes.DescriptorBufferSupported)
        {
            descriptorBuffer?.Dispose();
        }
        else
        {
            VkRes.Vk.FreeDescriptorSets(VkRes.VkDevice, descriptorPool, 1, in descriptorSet);

            VkRes.Vk.DestroyDescriptorPool(VkRes.VkDevice, descriptorPool, null);
        }
    }

    private void Refresh()
    {
        if (useResources!.Any(item => item is null) || (Layout.IsLastBindless && bindlessResources is null))
        {
            return;
        }

        sampledTextures.Clear();
        storageTextures.Clear();
        Alloter.Clear();

        if (VkRes.DescriptorBufferSupported)
        {
            byte* descriptor = (byte*)descriptorBuffer!.Map(Layout.SizeInBytes);

            for (int i = 0; i < useResources!.Length; i++)
            {
                ulong offset = VkRes.ExtDescriptorBuffer.GetDescriptorSetLayoutBindingOffset(VkRes.VkDevice, Layout.Handle, (uint)i);

                WriteDescriptorBuffer(descriptor + offset, Layout.DescriptorTypes[i], useResources[i]);
            }

            if (bindlessResources != null)
            {
                ulong offset = VkRes.ExtDescriptorBuffer.GetDescriptorSetLayoutBindingOffset(VkRes.VkDevice, Layout.Handle, (uint)Layout.DescriptorTypes.Length - 1);

                WriteDescriptorBuffer(descriptor + offset, Layout.DescriptorTypes[^1], bindlessResources);
            }

            descriptorBuffer.Unmap();
        }
        else
        {
            WriteDescriptorSet[] writeDescriptorSets = new WriteDescriptorSet[Layout.DescriptorTypes.Length];

            for (int i = 0; i < useResources!.Length; i++)
            {
                writeDescriptorSets[i] = GetWriteDescriptorSet(i, Layout.DescriptorTypes[i], useResources[i]);
            }

            if (bindlessResources != null)
            {
                writeDescriptorSets[^1] = GetWriteDescriptorSet(Layout.DescriptorTypes.Length - 1, Layout.DescriptorTypes[^1], bindlessResources);
            }

            VkRes.Vk.UpdateDescriptorSets(VkRes.VkDevice,
                                          (uint)writeDescriptorSets.Length,
                                          writeDescriptorSets.AsPointer(),
                                          0,
                                          null);
        }
    }

    private void WriteDescriptorBuffer(byte* buffer, DescriptorType type, params IBindableResource[] bindableResources)
    {
        if (IsDescriptorBuffer(type))
        {
            bool isUniform = type is DescriptorType.UniformBuffer or DescriptorType.UniformBufferDynamic;

            nuint descriptorSize = isUniform ? VkRes.DescriptorBufferProperties.UniformBufferDescriptorSize : VkRes.DescriptorBufferProperties.StorageBufferDescriptorSize;

            for (int i = 0; i < bindableResources.Length; i++)
            {
                DeviceBufferRange range = Util.GetBufferRange(bindableResources[i], 0);

                DescriptorAddressInfoEXT addressInfo = new()
                {
                    SType = StructureType.DescriptorAddressInfoExt,
                    Address = range.Buffer.Address + range.Offset,
                    Range = range.SizeInBytes,
                    Format = Format.Undefined
                };

                if (isUniform)
                {
                    GetDescriptor(descriptorSize, new DescriptorDataEXT() { PUniformBuffer = &addressInfo });
                }
                else
                {
                    GetDescriptor(descriptorSize, new DescriptorDataEXT() { PStorageBuffer = &addressInfo });
                }

                buffer += descriptorSize;
            }
        }
        else if (IsDescriptorImage(type))
        {
            bool isSampled = type == DescriptorType.SampledImage;

            nuint descriptorSize = isSampled ? VkRes.DescriptorBufferProperties.SampledImageDescriptorSize : VkRes.DescriptorBufferProperties.StorageImageDescriptorSize;

            for (int i = 0; i < bindableResources.Length; i++)
            {
                TextureView textureView = (TextureView)bindableResources[i];

                DescriptorImageInfo imageInfo = new()
                {
                    ImageView = textureView.Handle,
                    ImageLayout = isSampled ? ImageLayout.ShaderReadOnlyOptimal : ImageLayout.General
                };

                if (isSampled)
                {
                    GetDescriptor(descriptorSize, new DescriptorDataEXT() { PSampledImage = &imageInfo });
                }
                else
                {
                    GetDescriptor(descriptorSize, new DescriptorDataEXT() { PStorageImage = &imageInfo });
                }

                buffer += descriptorSize;

                if (isSampled)
                {
                    sampledTextures.Add(textureView.Target);
                }
                else
                {
                    storageTextures.Add(textureView.Target);
                }
            }
        }
        else if (IsDescriptorSampler(type))
        {
            nuint descriptorSize = VkRes.DescriptorBufferProperties.SamplerDescriptorSize;

            for (int i = 0; i < bindableResources.Length; i++)
            {
                Sampler sampler = (Sampler)bindableResources[i];

                VkSampler vkSampler = sampler.Handle;

                GetDescriptor(descriptorSize, new DescriptorDataEXT() { PSampler = &vkSampler });

                buffer += descriptorSize;
            }
        }
        else if (IsDescriptorAccelerationStructure(type))
        {
            nuint descriptorSize = VkRes.DescriptorBufferProperties.AccelerationStructureDescriptorSize;

            for (int i = 0; i < bindableResources.Length; i++)
            {
                TopLevelAS topLevelAS = (TopLevelAS)bindableResources[i];

                GetDescriptor(descriptorSize, new DescriptorDataEXT() { AccelerationStructure = topLevelAS.Address });

                buffer += descriptorSize;
            }
        }
        else
        {
            throw new NotSupportedException();
        }

        void GetDescriptor(nuint descriptorSize, DescriptorDataEXT descriptorData)
        {
            DescriptorGetInfoEXT getInfo = new()
            {
                SType = StructureType.DescriptorGetInfoExt,
                Type = type,
                Data = descriptorData
            };

            VkRes.ExtDescriptorBuffer.GetDescriptor(VkRes.VkDevice,
                                                    &getInfo,
                                                    descriptorSize,
                                                    buffer);
        }
    }

    private WriteDescriptorSet GetWriteDescriptorSet(int binding, DescriptorType type, params IBindableResource[] bindableResources)
    {
        WriteDescriptorSet writeDescriptorSet = new()
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = descriptorSet,
            DstBinding = (uint)binding,
            DescriptorCount = (uint)bindableResources.Length,
            DescriptorType = type
        };

        if (IsDescriptorBuffer(type))
        {
            DescriptorBufferInfo* bufferInfos = Alloter.Alloc<DescriptorBufferInfo>(bindableResources.Length);

            for (int i = 0; i < bindableResources.Length; i++)
            {
                DeviceBufferRange range = Util.GetBufferRange(bindableResources[i], 0);

                bufferInfos[i] = new()
                {
                    Buffer = range.Buffer.Handle,
                    Offset = range.Offset,
                    Range = range.SizeInBytes
                };
            }

            writeDescriptorSet.PBufferInfo = bufferInfos;
        }
        else if (IsDescriptorImage(type))
        {
            bool isSampled = type == DescriptorType.SampledImage;

            DescriptorImageInfo* imageInfos = Alloter.Alloc<DescriptorImageInfo>(bindableResources.Length);

            for (int i = 0; i < bindableResources.Length; i++)
            {
                TextureView textureView = (TextureView)bindableResources[i];

                imageInfos[i] = new()
                {
                    ImageView = textureView.Handle,
                    ImageLayout = isSampled ? ImageLayout.ShaderReadOnlyOptimal : ImageLayout.General
                };

                if (isSampled)
                {
                    sampledTextures.Add(textureView.Target);
                }
                else
                {
                    storageTextures.Add(textureView.Target);
                }
            }

            writeDescriptorSet.PImageInfo = imageInfos;
        }
        else if (IsDescriptorSampler(type))
        {
            DescriptorImageInfo* imageInfos = Alloter.Alloc<DescriptorImageInfo>(bindableResources.Length);

            for (int i = 0; i < bindableResources.Length; i++)
            {
                Sampler sampler = (Sampler)bindableResources[i];

                imageInfos[i] = new()
                {
                    Sampler = sampler.Handle
                };
            }

            writeDescriptorSet.PImageInfo = imageInfos;
        }
        else if (IsDescriptorAccelerationStructure(type))
        {
            WriteDescriptorSetAccelerationStructureKHR writeDescriptorSetAS = new()
            {
                SType = StructureType.WriteDescriptorSetAccelerationStructureKhr,
                AccelerationStructureCount = (uint)bindableResources.Length
            };

            AccelerationStructureKHR* accelerationStructures = Alloter.Alloc<AccelerationStructureKHR>(bindableResources.Length);

            for (int i = 0; i < bindableResources.Length; i++)
            {
                TopLevelAS topLevelAS = (TopLevelAS)bindableResources[i];

                accelerationStructures[i] = topLevelAS.Handle;
            }

            writeDescriptorSetAS.PAccelerationStructures = accelerationStructures;

            writeDescriptorSet.PNext = Alloter.Alloc(writeDescriptorSetAS);
        }
        else
        {
            throw new NotSupportedException();
        }

        return writeDescriptorSet;
    }

    private static bool IsDescriptorBuffer(DescriptorType type)
    {
        return type is DescriptorType.UniformBuffer
               or DescriptorType.UniformBufferDynamic
               or DescriptorType.StorageBuffer
               or DescriptorType.StorageBufferDynamic;
    }

    private static bool IsDescriptorImage(DescriptorType type)
    {
        return type is DescriptorType.SampledImage
               or DescriptorType.StorageImage;
    }

    private static bool IsDescriptorSampler(DescriptorType type)
    {
        return type == DescriptorType.Sampler;
    }

    private static bool IsDescriptorAccelerationStructure(DescriptorType type)
    {
        return type == DescriptorType.AccelerationStructureKhr;
    }
}
