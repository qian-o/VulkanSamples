using Graphics.Core.Helpers;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class TopLevelAS : VulkanObject<AccelerationStructureKHR>, IBindableResource
{
    internal TopLevelAS(VulkanResources vkRes, ref readonly TopLevelASDescription description) : base(vkRes, ObjectType.AccelerationStructureKhr)
    {
        AccelerationStructureInstanceKHR[] instances = new AccelerationStructureInstanceKHR[description.Instances.Length];

        for (int i = 0; i < description.Instances.Length; i++)
        {
            AccelStructInstance instance = description.Instances[i];

            instances[i] = new()
            {
                Transform = Util.GetTransformMatrix(instance.Transform4x4),
                InstanceCustomIndex = instance.InstanceID,
                Mask = instance.InstanceMask,
                InstanceShaderBindingTableRecordOffset = instance.InstanceContributionToHitGroupIndex,
                AccelerationStructureReference = instance.BottomLevel!.Address,
                Flags = Formats.GetGeometryInstanceFlags(instance.Options)
            };
        }

        using DeviceBuffer instanceBuffer = new(vkRes,
                                                BufferUsageFlags.AccelerationStructureBuildInputReadOnlyBitKhr,
                                                (uint)(sizeof(AccelerationStructureInstanceKHR) * description.Instances.Length),
                                                true);

        vkRes.GraphicsDevice.UpdateBuffer(instanceBuffer, instances);

        AccelerationStructureGeometryKHR geometry = new()
        {
            SType = StructureType.AccelerationStructureGeometryKhr,
            Flags = GeometryFlagsKHR.OpaqueBitKhr,
            GeometryType = GeometryTypeKHR.InstancesKhr,
            Geometry = new AccelerationStructureGeometryDataKHR
            {
                Instances = new AccelerationStructureGeometryInstancesDataKHR
                {
                    SType = StructureType.AccelerationStructureGeometryInstancesDataKhr,
                    ArrayOfPointers = false,
                    Data = new DeviceOrHostAddressConstKHR
                    {
                        DeviceAddress = instanceBuffer.Address
                    }
                }
            }
        };

        AccelerationStructureBuildGeometryInfoKHR buildGeometryInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildGeometryInfoKhr,
            Type = AccelerationStructureTypeKHR.TopLevelKhr,
            GeometryCount = 1,
            PGeometries = &geometry,
            Flags = Formats.GetBuildAccelerationStructureFlags(description.Options)
        };

        AccelerationStructureBuildRangeInfoKHR buildRangeInfo = new()
        {
            PrimitiveCount = (uint)description.Instances.Length,
            PrimitiveOffset = description.Offset,
            FirstVertex = 0,
            TransformOffset = 0
        };

        AccelerationStructureBuildSizesInfoKHR buildSizesInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildSizesInfoKhr
        };

        VkRes.KhrAccelerationStructure.GetAccelerationStructureBuildSizes(VkRes.VkDevice,
                                                                          AccelerationStructureBuildTypeKHR.DeviceKhr,
                                                                          &buildGeometryInfo,
                                                                          &buildRangeInfo.PrimitiveCount,
                                                                          &buildSizesInfo);

        DeviceBuffer asBuffer = new(VkRes,
                                    BufferUsageFlags.AccelerationStructureStorageBitKhr,
                                    (uint)buildSizesInfo.AccelerationStructureSize,
                                    false);

        AccelerationStructureCreateInfoKHR createInfo = new()
        {
            SType = StructureType.AccelerationStructureCreateInfoKhr,
            Buffer = asBuffer.Handle,
            Size = buildSizesInfo.AccelerationStructureSize,
            Type = AccelerationStructureTypeKHR.TopLevelKhr
        };

        AccelerationStructureKHR tlas;
        VkRes.KhrAccelerationStructure.CreateAccelerationStructure(VkRes.VkDevice, &createInfo, null, &tlas).ThrowCode();

        AccelerationStructureDeviceAddressInfoKHR asDeviceAddressInfo = new()
        {
            SType = StructureType.AccelerationStructureDeviceAddressInfoKhr,
            AccelerationStructure = tlas
        };

        ulong topLevelASAddress = VkRes.KhrAccelerationStructure.GetAccelerationStructureDeviceAddress(VkRes.VkDevice, &asDeviceAddressInfo);

        using DeviceBuffer scratchBuffer = new(VkRes,
                                               BufferUsageFlags.StorageBufferBit,
                                               (uint)buildSizesInfo.BuildScratchSize,
                                               false);

        buildGeometryInfo.Mode = BuildAccelerationStructureModeKHR.BuildKhr;
        buildGeometryInfo.DstAccelerationStructure = tlas;
        buildGeometryInfo.ScratchData = new DeviceOrHostAddressKHR
        {
            DeviceAddress = scratchBuffer.Address
        };

        using StagingCommandPool commandPool = new(VkRes, VkRes.GraphicsDevice.GraphicsExecutor);

        CommandBuffer commandBuffer = commandPool.BeginNewCommandBuffer();

        AccelerationStructureBuildRangeInfoKHR* pBuildRangeInfos = buildRangeInfo.AsPointer();

        VkRes.KhrAccelerationStructure.CmdBuildAccelerationStructures(commandBuffer,
                                                                      1,
                                                                      &buildGeometryInfo,
                                                                      &pBuildRangeInfos);

        commandPool.EndAndSubmitCommandBuffer(commandBuffer);

        Handle = tlas;
        Address = topLevelASAddress;
        DeviceBuffer = asBuffer;
    }

    internal override AccelerationStructureKHR Handle { get; }

    internal ulong Address { get; }

    internal DeviceBuffer DeviceBuffer { get; }

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    internal override void DestroyObject()
    {
        VkRes.KhrAccelerationStructure.DestroyAccelerationStructure(VkRes.VkDevice, Handle, null);

        DeviceBuffer.Dispose();
    }
}
