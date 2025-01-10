using Graphics.Core.Helpers;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class BottomLevelAS : VulkanObject<AccelerationStructureKHR>, IBindableResource
{
    internal BottomLevelAS(VulkanResources vkRes, ref readonly BottomLevelASDescription description) : base(vkRes, ObjectType.AccelerationStructureKhr)
    {
        using DeviceBuffer transformBuffer = new(VkRes,
                                                 BufferUsageFlags.AccelerationStructureBuildInputReadOnlyBitKhr,
                                                 (uint)(description.Geometries.Length * sizeof(TransformMatrixKHR)),
                                                 true);

        for (int i = 0; i < description.Geometries.Length; i++)
        {
            AccelStructGeometry geometry = description.Geometries[i];

            if (geometry is AccelStructTriangles triangles)
            {
                TransformMatrixKHR transform = Util.GetTransformMatrix(triangles.Transform);

                VkRes.GraphicsDevice.UpdateBuffer(transformBuffer, in transform, (uint)(i * sizeof(TransformMatrixKHR)));
            }
        }

        AccelerationStructureGeometryKHR[] geometries = new AccelerationStructureGeometryKHR[description.Geometries.Length];
        AccelerationStructureBuildRangeInfoKHR[] buildRangeInfos = new AccelerationStructureBuildRangeInfoKHR[description.Geometries.Length];
        uint[] maxPrimitiveCounts = new uint[description.Geometries.Length];

        for (int i = 0; i < description.Geometries.Length; i++)
        {
            AccelStructGeometry geometry = description.Geometries[i];

            AccelerationStructureGeometryKHR geometryKhr;
            AccelerationStructureBuildRangeInfoKHR buildRangeInfoKhr;

            if (geometry is AccelStructTriangles triangles)
            {
                ulong vertexAddress = triangles.VertexBuffer!.Address + triangles.VertexOffsetInBytes;
                ulong indexAddress = triangles.IndexBuffer != null ? triangles.IndexBuffer.Address + triangles.IndexOffsetInBytes : 0;
                ulong transformAddress = transformBuffer.Address + (uint)(i * sizeof(TransformMatrixKHR));

                geometryKhr = new AccelerationStructureGeometryKHR
                {
                    SType = StructureType.AccelerationStructureGeometryKhr,
                    Flags = Formats.GetGeometryFlags(triangles.Options),
                    GeometryType = GeometryTypeKHR.TrianglesKhr,
                    Geometry = new AccelerationStructureGeometryDataKHR
                    {
                        Triangles = new AccelerationStructureGeometryTrianglesDataKHR
                        {
                            SType = StructureType.AccelerationStructureGeometryTrianglesDataKhr,
                            VertexData = new DeviceOrHostAddressConstKHR
                            {
                                DeviceAddress = vertexAddress
                            },
                            VertexFormat = Formats.GetPixelFormat(triangles.VertexFormat, false),
                            VertexStride = triangles.VertexStrideInBytes,
                            MaxVertex = triangles.VertexCount,
                            IndexType = triangles.IndexBuffer != null ? Formats.GetIndexType(triangles.IndexFormat) : IndexType.NoneKhr,
                            IndexData = new DeviceOrHostAddressConstKHR
                            {
                                DeviceAddress = indexAddress
                            },
                            TransformData = new DeviceOrHostAddressConstKHR
                            {
                                DeviceAddress = transformAddress
                            }
                        }
                    }
                };

                buildRangeInfoKhr = new AccelerationStructureBuildRangeInfoKHR
                {
                    PrimitiveCount = triangles.IndexBuffer != null ? triangles.IndexCount / 3 : triangles.VertexCount / 3,
                    PrimitiveOffset = 0,
                    FirstVertex = 0,
                    TransformOffset = 0
                };
            }
            else if (geometry is AccelStructAABBs aabbs)
            {
                geometryKhr = new AccelerationStructureGeometryKHR
                {
                    SType = StructureType.AccelerationStructureGeometryKhr,
                    Flags = Formats.GetGeometryFlags(aabbs.Options),
                    GeometryType = GeometryTypeKHR.AabbsKhr,
                    Geometry = new AccelerationStructureGeometryDataKHR
                    {
                        Aabbs = new AccelerationStructureGeometryAabbsDataKHR
                        {
                            SType = StructureType.AccelerationStructureGeometryAabbsDataKhr,
                            Data = new DeviceOrHostAddressConstKHR
                            {
                                DeviceAddress = aabbs.AABBs!.Address
                            },
                            Stride = aabbs.Stride
                        }
                    }
                };

                buildRangeInfoKhr = new AccelerationStructureBuildRangeInfoKHR
                {
                    PrimitiveCount = (uint)aabbs.Count,
                    PrimitiveOffset = aabbs.Offset,
                    FirstVertex = 0,
                    TransformOffset = 0
                };
            }
            else
            {
                throw new NotSupportedException("Geometry type not supported.");
            }

            geometries[i] = geometryKhr;
            buildRangeInfos[i] = buildRangeInfoKhr;
            maxPrimitiveCounts[i] = buildRangeInfoKhr.PrimitiveCount;
        }

        AccelerationStructureBuildGeometryInfoKHR buildGeometryInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildGeometryInfoKhr,
            Type = AccelerationStructureTypeKHR.BottomLevelKhr,
            GeometryCount = (uint)geometries.Length,
            PGeometries = geometries.AsPointer(),
            Flags = BuildAccelerationStructureFlagsKHR.PreferFastTraceBitKhr
        };

        AccelerationStructureBuildSizesInfoKHR buildSizesInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildSizesInfoKhr
        };

        VkRes.KhrAccelerationStructure.GetAccelerationStructureBuildSizes(VkRes.VkDevice,
                                                                          AccelerationStructureBuildTypeKHR.DeviceKhr,
                                                                          &buildGeometryInfo,
                                                                          maxPrimitiveCounts.AsPointer(),
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
            Type = AccelerationStructureTypeKHR.BottomLevelKhr
        };

        AccelerationStructureKHR blas;
        VkRes.KhrAccelerationStructure.CreateAccelerationStructure(VkRes.VkDevice, &createInfo, null, &blas).ThrowCode();

        AccelerationStructureDeviceAddressInfoKHR deviceAddressInfo = new()
        {
            SType = StructureType.AccelerationStructureDeviceAddressInfoKhr,
            AccelerationStructure = blas
        };

        ulong blasAddress = VkRes.KhrAccelerationStructure.GetAccelerationStructureDeviceAddress(VkRes.VkDevice, &deviceAddressInfo);

        using DeviceBuffer scratchBuffer = new(VkRes,
                                               BufferUsageFlags.StorageBufferBit,
                                               (uint)buildSizesInfo.BuildScratchSize,
                                               false);

        buildGeometryInfo.Mode = BuildAccelerationStructureModeKHR.BuildKhr;
        buildGeometryInfo.DstAccelerationStructure = blas;
        buildGeometryInfo.ScratchData = new DeviceOrHostAddressKHR
        {
            DeviceAddress = scratchBuffer.Address
        };

        using StagingCommandPool commandPool = new(VkRes, VkRes.GraphicsDevice.GraphicsExecutor);

        CommandBuffer commandBuffer = commandPool.BeginNewCommandBuffer();

        AccelerationStructureBuildRangeInfoKHR* pBuildRangeInfos = buildRangeInfos.AsPointer();

        VkRes.KhrAccelerationStructure.CmdBuildAccelerationStructures(commandBuffer,
                                                                      1,
                                                                      &buildGeometryInfo,
                                                                      &pBuildRangeInfos);

        commandPool.EndAndSubmitCommandBuffer(commandBuffer);

        Handle = blas;
        Address = blasAddress;
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
