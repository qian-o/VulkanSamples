using System.Runtime.CompilerServices;
using Graphics.Core;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphics.Vulkan;

public unsafe class GraphicsDevice : VulkanObject<VkDevice>
{
    internal const uint MinStagingBufferSize = 1024 * 4;
    internal const uint MaxStagingBufferSize = 1024 * 1024 * 4;

    private readonly object _stagingResourcesLock;
    private readonly List<StagingCommandPool> _availableStagingCommandPools;
    private readonly List<DeviceBuffer> _availableStagingBuffers;
    private readonly List<Semaphore> _availableStagingSemaphores;
    private readonly List<Fence> _availableStagingFences;

    internal GraphicsDevice(VulkanResources vkRes,
                            VkDevice device,
                            uint graphicsQueueFamilyIndex,
                            uint computeQueueFamilyIndex,
                            uint transferQueueFamilyIndex) : base(vkRes, ObjectType.Device)
    {
        _stagingResourcesLock = new object();
        _availableStagingCommandPools = [];
        _availableStagingBuffers = [];
        _availableStagingSemaphores = [];
        _availableStagingFences = [];

        Handle = device;

        VkRes.InitializeGraphicsDevice(this);

        KhrSwapchain = CreateDeviceExtension<KhrSwapchain>(device);
        ExtDescriptorBuffer = VkRes.DescriptorBufferSupported ? CreateDeviceExtension<ExtDescriptorBuffer>(device) : null;
        KhrRayTracingPipeline = VkRes.RayTracingSupported ? CreateDeviceExtension<KhrRayTracingPipeline>(device) : null;
        KhrAccelerationStructure = VkRes.RayQuerySupported || VkRes.RayTracingSupported ? CreateDeviceExtension<KhrAccelerationStructure>(device) : null;
        KhrDeferredHostOperations = VkRes.RayQuerySupported || VkRes.RayTracingSupported ? CreateDeviceExtension<KhrDeferredHostOperations>(device) : null;
        GraphicsExecutor = new Executor(VkRes, graphicsQueueFamilyIndex);
        ComputeExecutor = new Executor(VkRes, computeQueueFamilyIndex);
        TransferExecutor = new Executor(VkRes, transferQueueFamilyIndex);
        GraphicsCommandPool = new CommandPool(VkRes, graphicsQueueFamilyIndex);
        ComputeCommandPool = new CommandPool(VkRes, computeQueueFamilyIndex);
        Factory = new ResourceFactory(VkRes);
        PointSampler = Factory.CreateSampler(SamplerDescription.Point);
        LinearSampler = Factory.CreateSampler(SamplerDescription.Linear);
        Aniso4xSampler = Factory.CreateSampler(SamplerDescription.Aniso4x);
    }

    internal override VkDevice Handle { get; }

    internal KhrSwapchain KhrSwapchain { get; }

    internal ExtDescriptorBuffer? ExtDescriptorBuffer { get; }

    internal KhrRayTracingPipeline? KhrRayTracingPipeline { get; }

    internal KhrAccelerationStructure? KhrAccelerationStructure { get; }

    internal KhrDeferredHostOperations? KhrDeferredHostOperations { get; }

    internal Executor GraphicsExecutor { get; }

    internal Executor ComputeExecutor { get; }

    internal Executor TransferExecutor { get; }

    internal CommandPool GraphicsCommandPool { get; }

    internal CommandPool ComputeCommandPool { get; }

    public ResourceFactory Factory { get; }

    public Sampler PointSampler { get; }

    public Sampler LinearSampler { get; }

    public Sampler Aniso4xSampler { get; }

    public PixelFormat GetBestDepthFormat()
    {
        Format format = VkRes.PhysicalDevice.FindSupportedFormat([Format.D32SfloatS8Uint, Format.D24UnormS8Uint, Format.D32Sfloat],
                                                                 ImageTiling.Optimal,
                                                                 FormatFeatureFlags.DepthStencilAttachmentBit);

        return Formats.GetPixelFormat(format);
    }

    #region UpdateBuffer
    public void UpdateBuffer<T>(DeviceBuffer buffer, uint bufferOffsetInBytes, T* source, int length) where T : unmanaged
    {
        uint sizeInBytes = (uint)(sizeof(T) * length);

        if (bufferOffsetInBytes + sizeInBytes > buffer.SizeInBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferOffsetInBytes), "The buffer offset and size exceed the buffer size.");
        }

        if (sizeInBytes == 0)
        {
            return;
        }

        if (buffer.IsHostVisible)
        {
            void* bufferPointer = buffer.Map(sizeInBytes, bufferOffsetInBytes);

            Unsafe.CopyBlock(bufferPointer, source, sizeInBytes);

            buffer.Unmap();
        }
        else
        {
            StagingCommandPool sharedCommandPool = GetStagingCommandPool();
            DeviceBuffer stagingBuffer = GetStagingBuffer(sizeInBytes);

            void* stagingBufferPointer = stagingBuffer.Map(sizeInBytes);

            Unsafe.CopyBlock(stagingBufferPointer, source, sizeInBytes);

            stagingBuffer.Unmap();

            CommandBuffer commandBuffer = sharedCommandPool.BeginNewCommandBuffer();

            BufferCopy bufferCopy = new()
            {
                SrcOffset = 0,
                DstOffset = bufferOffsetInBytes,
                Size = sizeInBytes
            };

            VkRes.Vk.CmdCopyBuffer(commandBuffer, stagingBuffer.Handle, buffer.Handle, 1, &bufferCopy);

            sharedCommandPool.EndAndSubmitCommandBuffer(commandBuffer);

            CacheStagingBuffer(stagingBuffer);
            CacheStagingCommandPool(sharedCommandPool);
        }
    }

    public void UpdateBuffer<T>(DeviceBuffer buffer, T[] source, uint bufferOffsetInBytes = 0) where T : unmanaged
    {
        fixed (T* sourcePointer = source)
        {
            UpdateBuffer(buffer, bufferOffsetInBytes, sourcePointer, source.Length);
        }
    }

    public void UpdateBuffer<T>(DeviceBuffer buffer, ref readonly T source, uint bufferOffsetInBytes = 0) where T : unmanaged
    {
        fixed (T* sourcePointer = &source)
        {
            UpdateBuffer(buffer, bufferOffsetInBytes, sourcePointer, 1);
        }
    }
    #endregion

    #region UpdateTexture
    public void UpdateTexture<T>(Texture texture,
                                 T* source,
                                 int length,
                                 uint x,
                                 uint y,
                                 uint z,
                                 uint width,
                                 uint height,
                                 uint depth,
                                 uint mipLevel,
                                 uint arrayLayer) where T : unmanaged
    {
        uint sizeInBytes = (uint)(sizeof(T) * length);

        if (x + width > texture.Width)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "The width exceeds the texture width.");
        }

        if (y + height > texture.Height)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "The height exceeds the texture height.");
        }

        if (z + depth > texture.Depth)
        {
            throw new ArgumentOutOfRangeException(nameof(depth), "The depth exceeds the texture depth.");
        }

        if (mipLevel >= texture.MipLevels)
        {
            throw new ArgumentOutOfRangeException(nameof(mipLevel), "The mip level exceeds the texture mip levels.");
        }

        if (arrayLayer >= texture.ArrayLayers)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayLayer), "The array layer exceeds the texture array layers.");
        }

        if (sizeInBytes == 0)
        {
            return;
        }

        StagingCommandPool sharedCommandPool = GetStagingCommandPool();
        DeviceBuffer stagingBuffer = GetStagingBuffer(sizeInBytes);

        void* stagingBufferPointer = stagingBuffer.Map(sizeInBytes);

        Unsafe.CopyBlock(stagingBufferPointer, source, sizeInBytes);

        stagingBuffer.Unmap();

        CommandBuffer commandBuffer = sharedCommandPool.BeginNewCommandBuffer();

        texture.TransitionLayout(commandBuffer, ImageLayout.TransferDstOptimal);
        {
            BufferImageCopy bufferImageCopy = new()
            {
                BufferOffset = 0,
                BufferRowLength = 0,
                BufferImageHeight = 0,
                ImageSubresource = new ImageSubresourceLayers
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    MipLevel = mipLevel,
                    BaseArrayLayer = arrayLayer,
                    LayerCount = 1
                },
                ImageOffset = new Offset3D((int)x, (int)y, (int)z),
                ImageExtent = new Extent3D(width, height, depth)
            };

            VkRes.Vk.CmdCopyBufferToImage(commandBuffer, stagingBuffer.Handle, texture.Handle, ImageLayout.TransferDstOptimal, 1, &bufferImageCopy);
        }
        texture.TransitionToBestLayout(commandBuffer);

        sharedCommandPool.EndAndSubmitCommandBuffer(commandBuffer);

        CacheStagingBuffer(stagingBuffer);
        CacheStagingCommandPool(sharedCommandPool);
    }

    public void UpdateTexture<T>(Texture texture,
                                 T[] source,
                                 uint x,
                                 uint y,
                                 uint z,
                                 uint width,
                                 uint height,
                                 uint depth,
                                 uint mipLevel,
                                 uint arrayLayer) where T : unmanaged
    {
        fixed (T* sourcePointer = source)
        {
            UpdateTexture(texture, sourcePointer, source.Length, x, y, z, width, height, depth, mipLevel, arrayLayer);
        }
    }
    #endregion

    public void SubmitCommands(CommandList commandList)
    {
        Fence stagingFence = GetStagingFence();

        SubmitCommandsCore(commandList, null, stagingFence);

        stagingFence.WaitAndReset();

        commandList.Submitted();

        CacheStagingFence(stagingFence);
    }

    public void SwapBuffers(Swapchain swapchain)
    {
        SwapBuffersCore(swapchain, null, true);
    }

    public void SubmitCommandsAndSwapBuffers(CommandList commandList, Swapchain swapchain)
    {
        Semaphore stagingSemaphore = GetStagingSemaphore();
        Fence stagingFence = GetStagingFence();

        SubmitCommandsCore(commandList, stagingSemaphore, stagingFence);

        Fence[] fences = SwapBuffersCore(swapchain,
                                         stagingSemaphore,
                                         false) ? [stagingFence, swapchain.ImageAvailableFence] : [stagingFence];

        WaitAndResetFences(fences);

        commandList.Submitted();

        CacheStagingFence(stagingFence);
        CacheStagingSemaphore(stagingSemaphore);
    }

    internal override ulong[] GetHandles()
    {
        return [(ulong)Handle.Handle];
    }

    internal override void DestroyObject()
    {
        foreach (Fence fence in _availableStagingFences)
        {
            fence.Dispose();
        }

        foreach (Semaphore semaphore in _availableStagingSemaphores)
        {
            semaphore.Dispose();
        }

        foreach (DeviceBuffer deviceBuffer in _availableStagingBuffers)
        {
            deviceBuffer.Dispose();
        }

        foreach (StagingCommandPool sharedCommandPool in _availableStagingCommandPools)
        {
            sharedCommandPool.Dispose();
        }

        _availableStagingFences.Clear();
        _availableStagingSemaphores.Clear();
        _availableStagingBuffers.Clear();
        _availableStagingCommandPools.Clear();

        Aniso4xSampler.Dispose();
        LinearSampler.Dispose();
        PointSampler.Dispose();

        ComputeCommandPool.Dispose();
        GraphicsCommandPool.Dispose();

        KhrDeferredHostOperations?.Dispose();
        KhrAccelerationStructure?.Dispose();
        KhrRayTracingPipeline?.Dispose();
        ExtDescriptorBuffer?.Dispose();
        KhrSwapchain.Dispose();

        VkRes.Vk.DestroyDevice(Handle, null);
    }

    private T CreateDeviceExtension<T>(VkDevice device) where T : NativeExtension<Vk>
    {
        if (!VkRes.Vk.TryGetDeviceExtension(VkRes.Instance, device, out T ext))
        {
            throw new InvalidOperationException($"Failed to load extension {typeof(T).Name}!");
        }

        return ext;
    }

    private void SubmitCommandsCore(CommandList commandList, Semaphore? signalSemaphore, Fence fence)
    {
        CommandBuffer commandBuffer = commandList.Handle;
        Executor executor = commandList.Executor;

        SubmitInfo submitInfo = new()
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &commandBuffer
        };

        if (signalSemaphore != null)
        {
            VkSemaphore signalSemaphoreHandle = signalSemaphore.Handle;

            submitInfo.SignalSemaphoreCount = 1;
            submitInfo.PSignalSemaphores = &signalSemaphoreHandle;
        }

        VkRes.Vk.QueueSubmit(executor.Handle, 1, &submitInfo, fence.Handle);
    }

    private bool SwapBuffersCore(Swapchain swapchain, Semaphore? waitSemaphore, bool waitAcquireNextImage)
    {
        SwapchainKHR swapchainKHR = swapchain.Handle;
        uint imageIndex = swapchain.CurrentImageIndex;

        PresentInfoKHR presentInfo = new()
        {
            SType = StructureType.PresentInfoKhr,
            SwapchainCount = 1,
            PSwapchains = &swapchainKHR,
            PImageIndices = &imageIndex
        };

        if (waitSemaphore != null)
        {
            VkSemaphore waitSemaphoreHandle = waitSemaphore.Handle;

            presentInfo.WaitSemaphoreCount = 1;
            presentInfo.PWaitSemaphores = &waitSemaphoreHandle;
        }

        Result result = KhrSwapchain.QueuePresent(GraphicsExecutor.Handle, &presentInfo);

        if (result == Result.ErrorOutOfDateKhr)
        {
            return false;
        }

        if (result is not Result.Success and not Result.SuboptimalKhr)
        {
            result.ThrowCode("Failed to present the swap chain image.");
        }

        swapchain.AcquireNextImage(waitAcquireNextImage);

        return true;
    }

    private void WaitAndResetFences(Fence[] fences)
    {
        VkFence[] vkFences = fences.Select(f => f.Handle).ToArray();

        fixed (VkFence* vkFencesPointer = vkFences)
        {
            VkRes.Vk.WaitForFences(Handle, (uint)vkFences.Length, vkFencesPointer, Vk.True, ulong.MaxValue);
            VkRes.Vk.ResetFences(Handle, (uint)vkFences.Length, vkFencesPointer);
        }
    }

    private StagingCommandPool GetStagingCommandPool()
    {
        lock (_stagingResourcesLock)
        {
            foreach (StagingCommandPool stagingCommandPool in _availableStagingCommandPools)
            {
                _availableStagingCommandPools.Remove(stagingCommandPool);

                return stagingCommandPool;
            }
        }

        return new StagingCommandPool(VkRes, TransferExecutor);
    }

    private void CacheStagingCommandPool(StagingCommandPool stagingCommandPool)
    {
        lock (_stagingResourcesLock)
        {
            _availableStagingCommandPools.Add(stagingCommandPool);
        }
    }

    private DeviceBuffer GetStagingBuffer(uint sizeInBytes)
    {
        lock (_stagingResourcesLock)
        {
            foreach (DeviceBuffer deviceBuffer in _availableStagingBuffers)
            {
                if (deviceBuffer.SizeInBytes >= sizeInBytes)
                {
                    _availableStagingBuffers.Remove(deviceBuffer);

                    return deviceBuffer;
                }
            }
        }

        uint bufferSize = Math.Max(MinStagingBufferSize, sizeInBytes);

        return Factory.CreateBuffer(new BufferDescription(bufferSize, BufferUsage.Staging));
    }

    private void CacheStagingBuffer(DeviceBuffer stagingBuffer)
    {
        lock (_stagingResourcesLock)
        {
            if (stagingBuffer.SizeInBytes > MaxStagingBufferSize)
            {
                stagingBuffer.Dispose();
            }
            else
            {
                _availableStagingBuffers.Add(stagingBuffer);
            }
        }
    }

    private Semaphore GetStagingSemaphore()
    {
        lock (_stagingResourcesLock)
        {
            foreach (Semaphore stagingSemaphore in _availableStagingSemaphores)
            {
                _availableStagingSemaphores.Remove(stagingSemaphore);

                return stagingSemaphore;
            }
        }

        return new Semaphore(VkRes);
    }

    private void CacheStagingSemaphore(Semaphore stagingSemaphore)
    {
        lock (_stagingResourcesLock)
        {
            _availableStagingSemaphores.Add(stagingSemaphore);
        }
    }

    private Fence GetStagingFence()
    {
        lock (_stagingResourcesLock)
        {
            foreach (Fence stagingFence in _availableStagingFences)
            {
                _availableStagingFences.Remove(stagingFence);

                return stagingFence;
            }
        }

        return new Fence(VkRes);
    }

    private void CacheStagingFence(Fence stagingFence)
    {
        lock (_stagingResourcesLock)
        {
            _availableStagingFences.Add(stagingFence);
        }
    }
}
