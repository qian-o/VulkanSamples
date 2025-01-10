using Graphics.Core.Helpers;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

internal sealed unsafe class ShaderTable : VulkanObject<ulong>
{
    public ShaderTable(VulkanResources vkRes, Pipeline pipeline, ref readonly RaytracingPipelineDescription description) : base(vkRes, ObjectType.Buffer)
    {
        uint handleSize = vkRes.RayTracingPipelineProperties.ShaderGroupHandleSize;
        uint handleAlignment = vkRes.RayTracingPipelineProperties.ShaderGroupHandleAlignment;
        uint handleSizeAligned = Util.AlignedSize(handleSize, handleAlignment);

        uint missShaderCount = description.Shaders.GetMissShaderCount();
        uint hitGroupCount = description.Shaders.GetHitGroupCount();

        uint length = 1 + missShaderCount + hitGroupCount;

        uint size = handleSizeAligned * length;

        byte[] shaderHandleStorage = new byte[size];
        VkRes.KhrRayTracingPipeline.GetRayTracingShaderGroupHandles(VkRes.VkDevice, pipeline.Handle, 0, length, size, shaderHandleStorage.AsPointer());

        byte[] raygenShaderHandleStorage = shaderHandleStorage.AsSpan(0, (int)handleSizeAligned).ToArray();
        byte[] missShaderHandleStorage = shaderHandleStorage.AsSpan((int)handleSizeAligned, (int)(handleSizeAligned * missShaderCount)).ToArray();
        byte[] hitGroupHandleStorage = shaderHandleStorage.AsSpan((int)(handleSizeAligned * (1 + missShaderCount)), (int)(handleSizeAligned * hitGroupCount)).ToArray();

        DeviceBuffer raygenShaderHandleBuffer = new(VkRes,
                                                    BufferUsageFlags.ShaderBindingTableBitKhr,
                                                    (uint)raygenShaderHandleStorage.Length,
                                                    true);
        vkRes.GraphicsDevice.UpdateBuffer(raygenShaderHandleBuffer, raygenShaderHandleStorage);

        DeviceBuffer missShaderHandleBuffer = new(VkRes,
                                                  BufferUsageFlags.ShaderBindingTableBitKhr,
                                                  (uint)missShaderHandleStorage.Length,
                                                  true);
        vkRes.GraphicsDevice.UpdateBuffer(missShaderHandleBuffer, missShaderHandleStorage);

        DeviceBuffer hitGroupHandleBuffer = new(VkRes,
                                                BufferUsageFlags.ShaderBindingTableBitKhr,
                                                (uint)hitGroupHandleStorage.Length,
                                                true);
        vkRes.GraphicsDevice.UpdateBuffer(hitGroupHandleBuffer, hitGroupHandleStorage);

        Handle = handleSizeAligned;
        RaygenShaderHandleBuffer = raygenShaderHandleBuffer;
        MissShaderHandleBuffer = missShaderHandleBuffer;
        HitGroupHandleBuffer = hitGroupHandleBuffer;
    }

    internal override ulong Handle { get; }

    internal DeviceBuffer RaygenShaderHandleBuffer { get; }

    internal DeviceBuffer MissShaderHandleBuffer { get; }

    internal DeviceBuffer HitGroupHandleBuffer { get; }

    internal override ulong[] GetHandles()
    {
        return
        [
            RaygenShaderHandleBuffer.Handle.Handle,
            MissShaderHandleBuffer.Handle.Handle,
            HitGroupHandleBuffer.Handle.Handle
        ];
    }

    internal override void DestroyObject()
    {
        RaygenShaderHandleBuffer.Dispose();
        MissShaderHandleBuffer.Dispose();
        HitGroupHandleBuffer.Dispose();
    }
}
