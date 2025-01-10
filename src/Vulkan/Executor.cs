using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Executor : VulkanObject<VkQueue>
{
    internal Executor(VulkanResources vkRes, uint queueFamilyIndex) : base(vkRes, ObjectType.Queue)
    {
        VkQueue queue;
        VkRes.Vk.GetDeviceQueue(VkRes.VkDevice, queueFamilyIndex, 0, &queue);

        Handle = queue;
        FamilyIndex = queueFamilyIndex;
    }

    internal override VkQueue Handle { get; }

    internal uint FamilyIndex { get; }

    internal override ulong[] GetHandles()
    {
        return [(ulong)Handle.Handle];
    }

    internal override void DestroyObject()
    {
    }
}
