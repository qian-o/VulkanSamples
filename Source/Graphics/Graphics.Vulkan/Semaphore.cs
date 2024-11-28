using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Semaphore : VulkanObject<VkSemaphore>
{
    internal Semaphore(VulkanResources vkRes) : base(vkRes, ObjectType.Semaphore)
    {
        SemaphoreCreateInfo createInfo = new()
        {
            SType = StructureType.SemaphoreCreateInfo
        };

        VkSemaphore semaphore;
        VkRes.Vk.CreateSemaphore(VkRes.VkDevice, &createInfo, null, &semaphore).ThrowCode();

        Handle = semaphore;
    }

    internal override VkSemaphore Handle { get; }

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    internal override void DestroyObject()
    {
        VkRes.Vk.DestroySemaphore(VkRes.VkDevice, Handle, null);
    }
}
