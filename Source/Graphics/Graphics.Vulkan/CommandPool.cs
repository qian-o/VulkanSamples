using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class CommandPool : VulkanObject<VkCommandPool>
{
    internal CommandPool(VulkanResources vkRes, uint queueFamilyIndex) : base(vkRes, ObjectType.CommandPool)
    {
        CommandPoolCreateInfo createInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = queueFamilyIndex,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit
        };

        VkCommandPool commandPool;
        VkRes.Vk.CreateCommandPool(VkRes.VkDevice, &createInfo, null, &commandPool).ThrowCode();

        Handle = commandPool;
    }

    internal override VkCommandPool Handle { get; }

    public CommandBuffer AllocateCommandBuffer()
    {
        CommandBufferAllocateInfo allocateInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = Handle,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };

        CommandBuffer commandBuffer;
        VkRes.Vk.AllocateCommandBuffers(VkRes.VkDevice, &allocateInfo, &commandBuffer).ThrowCode();

        return commandBuffer;
    }

    public void FreeCommandBuffer(CommandBuffer commandBuffer)
    {
        VkRes.Vk.FreeCommandBuffers(VkRes.VkDevice, Handle, 1, &commandBuffer);
    }

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    internal override void DestroyObject()
    {
        VkRes.Vk.DestroyCommandPool(VkRes.VkDevice, Handle, null);
    }
}
