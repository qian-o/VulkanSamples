using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKFence : DeviceResource
{
    public VKFence(Context context) : base(context)
    {
        FenceCreateInfo createInfo = new()
        {
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit
        };

        VkFence fence;
        Context.Vk.CreateFence(Context.Device, &createInfo, null, &fence).ThrowCode();
        Context.Vk.ResetFences(Context.Device, 1, &fence).ThrowCode();

        Fence = fence;
    }

    public new VKContext Context => (VKContext)base.Context;

    public VkFence Fence { get; }

    public void Wait()
    {
        VkFence fence = Fence;

        Context.Vk.WaitForFences(Context.Device, 1, &fence, Vk.True, ulong.MaxValue).ThrowCode();
        Context.Vk.ResetFences(Context.Device, 1, &fence).ThrowCode();
    }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.Fence, Fence.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyFence(Context.Device, Fence, null);
    }
}
