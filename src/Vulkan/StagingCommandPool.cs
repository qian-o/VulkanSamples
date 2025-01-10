using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class StagingCommandPool : VulkanObject<VkCommandPool>
{
    internal StagingCommandPool(VulkanResources vkRes, Executor executor) : base(vkRes, ObjectType.CommandPool)
    {
        CommandPoolCreateInfo createInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = executor.FamilyIndex,
            Flags = CommandPoolCreateFlags.TransientBit
        };

        VkCommandPool commandPool;
        VkRes.Vk.CreateCommandPool(VkRes.VkDevice, &createInfo, null, &commandPool).ThrowCode();

        Handle = commandPool;
        TaskExecutor = executor;
        Fence = new Fence(VkRes);
    }

    internal override VkCommandPool Handle { get; }

    internal Executor TaskExecutor { get; }

    internal Fence Fence { get; }

    public CommandBuffer BeginNewCommandBuffer()
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

        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };

        VkRes.Vk.BeginCommandBuffer(commandBuffer, &beginInfo).ThrowCode();

        return commandBuffer;
    }

    public void EndAndSubmitCommandBuffer(CommandBuffer commandBuffer)
    {
        VkRes.Vk.EndCommandBuffer(commandBuffer).ThrowCode();

        SubmitInfo submitInfo = new()
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &commandBuffer
        };

        VkRes.Vk.QueueSubmit(TaskExecutor.Handle, 1, &submitInfo, Fence.Handle).ThrowCode();

        Fence.WaitAndReset();

        VkRes.Vk.FreeCommandBuffers(VkRes.VkDevice, Handle, 1, &commandBuffer);
    }

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    internal override void DestroyObject()
    {
        Fence.Dispose();

        VkRes.Vk.DestroyCommandPool(VkRes.VkDevice, Handle, null);
    }
}
