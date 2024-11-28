using Graphics.Core.Helpers;
using Graphics.Engine.Enums;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKCommandProcessor : CommandProcessor
{
    private readonly Queue<VKCommandBuffer> availableBuffers = [];

    private VkQueue queue;

    private int waitSubmitBufferCount;
    private VKCommandBuffer[] waitSubmitBuffers = [];

    public VKCommandProcessor(Context context,
                              CommandProcessorType type) : base(context)
    {
        FamilyIndex = type switch
        {
            CommandProcessorType.Graphics => Context.GraphicsFamilyIndex,
            CommandProcessorType.Compute => Context.ComputeFamilyIndex,
            CommandProcessorType.Transfer => Context.TransferFamilyIndex,
            _ => throw new NotSupportedException()
        };

        Context.Vk.GetDeviceQueue(Context.Device, FamilyIndex, 0, queue.AsPointer());
    }

    public new VKContext Context => (VKContext)base.Context;

    public uint FamilyIndex { get; }

    public override CommandBuffer CommandBuffer()
    {
        lock (this)
        {
            VKCommandBuffer commandBuffer;

            if (availableBuffers.Count == 0)
            {
                commandBuffer = new VKCommandBuffer(Context, this);
            }
            else
            {
                commandBuffer = availableBuffers.Dequeue();
                commandBuffer.Reset();
            }

            return commandBuffer;
        }
    }

    public override void Submit(bool isSyncUpToGpu = true)
    {
        lock (this)
        {
            if (isSyncUpToGpu)
            {
                Context.SyncUpToGpu();
            }

            for (int i = 0; i < waitSubmitBufferCount; i++)
            {
                VKCommandBuffer vKCommandBuffer = waitSubmitBuffers[i];

                VkCommandBuffer commandBuffer = vKCommandBuffer.CommandBuffer;
                PipelineStageFlags pipelineStageFlags = PipelineStageFlags.ColorAttachmentOutputBit;

                SubmitInfo submitInfo = new()
                {
                    SType = StructureType.SubmitInfo,
                    CommandBufferCount = 1,
                    PCommandBuffers = &commandBuffer,
                    PWaitDstStageMask = &pipelineStageFlags
                };

                Context.Vk.QueueSubmit(queue, 1, &submitInfo, default);

                availableBuffers.Enqueue(vKCommandBuffer);
            }

            waitSubmitBufferCount = 0;
            Array.Clear(waitSubmitBuffers, 0, waitSubmitBuffers.Length);
        }
    }

    public override void WaitIdle()
    {
        Context.Vk.QueueWaitIdle(queue);
    }

    public void CommitCommandBuffer(VKCommandBuffer commandBuffer)
    {
        lock (this)
        {
            if (waitSubmitBuffers.Length == waitSubmitBufferCount)
            {
                Array.Resize(ref waitSubmitBuffers, waitSubmitBuffers.Length + 60);
            }

            waitSubmitBuffers[waitSubmitBufferCount++] = commandBuffer;
        }
    }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.Queue, (ulong)queue.Handle, name);
    }

    protected override void Destroy()
    {
        foreach (VKCommandBuffer commandBuffer in availableBuffers)
        {
            commandBuffer.Dispose();
        }
    }
}
