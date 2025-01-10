using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Fence : VulkanObject<VkFence>
{
    internal Fence(VulkanResources vkRes) : base(vkRes, ObjectType.Fence)
    {
        FenceCreateInfo createInfo = new()
        {
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit
        };

        VkFence fence;
        VkRes.Vk.CreateFence(VkRes.VkDevice, &createInfo, null, &fence).ThrowCode();
        VkRes.Vk.ResetFences(VkRes.VkDevice, 1, &fence).ThrowCode();

        Handle = fence;
    }

    internal override VkFence Handle { get; }

    public void WaitAndReset()
    {
        VkFence fence = Handle;

        VkRes.Vk.WaitForFences(VkRes.VkDevice, 1, &fence, Vk.True, ulong.MaxValue).ThrowCode();
        VkRes.Vk.ResetFences(VkRes.VkDevice, 1, &fence).ThrowCode();
    }

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    internal override void DestroyObject()
    {
        VkRes.Vk.DestroyFence(VkRes.VkDevice, Handle, null);
    }
}
