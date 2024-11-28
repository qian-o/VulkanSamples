using Graphics.Core.Helpers;
using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;
using Graphics.Engine.Helpers;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKSwapChain : SwapChain
{
    private VkQueue queue;

    private SwapchainKHR swapchain;

    private Texture? depthStencilTarget;
    private FrameBuffer[] frameBuffers = [];

    private uint currentImageIndex;

    public VKSwapChain(Context context,
                       ref readonly SwapChainDesc desc) : base(context, in desc)
    {
        Fence = new VKFence(Context);
        Surface = desc.Target.Create<AllocationCallbacks>(Context.Instance.ToHandle(), null).ToSurface();

        InitSwapChain();
        AcquireNextImage();
    }

    public new VKContext Context => (VKContext)base.Context;

    public VKFence Fence { get; }

    public SurfaceKHR Surface { get; }

    public override FrameBuffer FrameBuffer => frameBuffers[currentImageIndex];

    public override void Present()
    {
        SwapchainKHR swapchainKHR = swapchain;
        uint imageIndex = currentImageIndex;

        PresentInfoKHR presentInfo = new()
        {
            SType = StructureType.PresentInfoKhr,
            SwapchainCount = 1,
            PSwapchains = &swapchainKHR,
            PImageIndices = &imageIndex
        };

        Result result = Context.KhrSwapchain.QueuePresent(queue, &presentInfo);

        if (result == Result.ErrorOutOfDateKhr)
        {
            return;
        }
        else
        {
            result.ThrowCode();
        }

        AcquireNextImage();
    }

    public override void Resize()
    {
        InitSwapChain();
        AcquireNextImage();
    }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.SwapchainKhr, swapchain.Handle, name);
    }

    protected override void Destroy()
    {
        DestroySwapChain();

        Fence.Dispose();

        Context.KhrSurface.DestroySurface(Context.Instance, Surface, null);
    }

    private void InitSwapChain()
    {
        if (queue.Handle == default)
        {
            Context.Vk.GetDeviceQueue(Context.Device, Context.GraphicsFamilyIndex, 0, queue.AsPointer());
        }

        DestroySwapChain();

        SurfaceCapabilitiesKHR surfaceCapabilities;
        Context.KhrSurface.GetPhysicalDeviceSurfaceCapabilities(Context.PhysicalDevice,
                                                                Surface,
                                                                &surfaceCapabilities);

        uint surfaceFormatCount;
        Context.KhrSurface.GetPhysicalDeviceSurfaceFormats(Context.PhysicalDevice,
                                                           Surface,
                                                           &surfaceFormatCount,
                                                           null);

        SurfaceFormatKHR[] surfaceFormats = new SurfaceFormatKHR[surfaceFormatCount];
        Context.KhrSurface.GetPhysicalDeviceSurfaceFormats(Context.PhysicalDevice,
                                                           Surface,
                                                           &surfaceFormatCount,
                                                           surfaceFormats.AsPointer());

        uint presentModeCount;
        Context.KhrSurface.GetPhysicalDeviceSurfacePresentModes(Context.PhysicalDevice,
                                                                Surface,
                                                                &presentModeCount,
                                                                null);

        PresentModeKHR[] presentModes = new PresentModeKHR[presentModeCount];
        Context.KhrSurface.GetPhysicalDeviceSurfacePresentModes(Context.PhysicalDevice,
                                                                Surface,
                                                                &presentModeCount,
                                                                presentModes.AsPointer());

        uint desiredNumberOfSwapchainImages = surfaceCapabilities.MinImageCount + 1;
        if (surfaceCapabilities.MaxImageCount > 0 && desiredNumberOfSwapchainImages > surfaceCapabilities.MaxImageCount)
        {
            desiredNumberOfSwapchainImages = surfaceCapabilities.MaxImageCount;
        }

        SwapchainCreateInfoKHR createInfo = new()
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = Surface,
            MinImageCount = desiredNumberOfSwapchainImages,
            ImageFormat = ChooseSwapSurfaceFormat(surfaceFormats).Format,
            ImageColorSpace = ChooseSwapSurfaceFormat(surfaceFormats).ColorSpace,
            ImageExtent = ChooseSwapExtent(surfaceCapabilities),
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            PreTransform = SurfaceTransformFlagsKHR.IdentityBitKhr,
            CompositeAlpha = surfaceCapabilities.SupportedCompositeAlpha,
            PresentMode = ChooseSwapPresentMode(presentModes, Desc.VSync),
            ImageSharingMode = SharingMode.Exclusive,
            Clipped = true
        };

        Context.KhrSwapchain.CreateSwapchain(Context.Device, &createInfo, null, swapchain.AsPointer()).ThrowCode();

        uint imageCount;
        Context.KhrSwapchain.GetSwapchainImages(Context.Device, swapchain, &imageCount, null);

        VkImage[] images = new VkImage[imageCount];
        Context.KhrSwapchain.GetSwapchainImages(Context.Device, swapchain, &imageCount, images.AsPointer());

        if (Desc.DepthStencilTargetFormat != null)
        {
            TextureDesc depthStencilDesc = TextureDesc.Default2D(createInfo.ImageExtent.Width,
                                                                 createInfo.ImageExtent.Height,
                                                                 1,
                                                                 Desc.DepthStencilTargetFormat.Value);
            depthStencilDesc.Usage = TextureUsage.DepthStencil;

            depthStencilTarget = Context.Factory.CreateTexture(ref depthStencilDesc);
        }

        frameBuffers = new FrameBuffer[imageCount];
        for (int i = 0; i < imageCount; i++)
        {
            TextureDesc colorDesc = TextureDesc.Default2D(createInfo.ImageExtent.Width,
                                                          createInfo.ImageExtent.Height,
                                                          1,
                                                          Formats.GetPixelFormat(createInfo.ImageFormat));
            colorDesc.Usage = TextureUsage.RenderTarget;

            VKTexture colorTarget = new(Context, images[i], ref colorDesc);

            FrameBufferDesc frameBufferDesc = FrameBufferDesc.Default(depthStencilTarget, colorTarget);

            frameBuffers[i] = Context.Factory.CreateFrameBuffer(ref frameBufferDesc);
        }
    }

    private void DestroySwapChain()
    {
        if (swapchain.Handle == default)
        {
            return;
        }

        foreach (FrameBuffer frameBuffer in frameBuffers)
        {
            frameBuffer.Dispose();
        }

        depthStencilTarget?.Dispose();

        Context.KhrSwapchain.DestroySwapchain(Context.Device, swapchain, null);

        swapchain = default;
        depthStencilTarget = null;
        frameBuffers = [];
    }

    private void AcquireNextImage()
    {
        uint imageIndex;
        Context.KhrSwapchain.AcquireNextImage(Context.Device,
                                              swapchain,
                                              ulong.MaxValue,
                                              default,
                                              Fence.Fence,
                                              &imageIndex);

        currentImageIndex = imageIndex;

        Fence.Wait();
    }

    private static SurfaceFormatKHR ChooseSwapSurfaceFormat(SurfaceFormatKHR[] surfaceFormats)
    {
        foreach (SurfaceFormatKHR availableFormat in surfaceFormats)
        {
            if (availableFormat.Format == Format.B8G8R8A8Srgb && availableFormat.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
            {
                return availableFormat;
            }
        }

        return surfaceFormats[0];
    }

    private static PresentModeKHR ChooseSwapPresentMode(PresentModeKHR[] presentModes, bool vsync)
    {
        PresentModeKHR presentMode = PresentModeKHR.FifoKhr;

        if (vsync && presentModes.Contains(PresentModeKHR.FifoRelaxedKhr))
        {
            presentMode = PresentModeKHR.FifoRelaxedKhr;
        }
        else if (presentModes.Contains(PresentModeKHR.MailboxKhr))
        {
            presentMode = PresentModeKHR.MailboxKhr;
        }
        else if (presentModes.Contains(PresentModeKHR.ImmediateKhr))
        {
            presentMode = PresentModeKHR.ImmediateKhr;
        }

        return presentMode;
    }

    private static Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities)
    {
        if (capabilities.CurrentExtent.Width != uint.MaxValue)
        {
            return capabilities.CurrentExtent;
        }

        return new Extent2D(Utils.Lerp(capabilities.MinImageExtent.Width,
                                       capabilities.MaxImageExtent.Width,
                                       0.5f),
                            Utils.Lerp(capabilities.MinImageExtent.Height,
                                       capabilities.MaxImageExtent.Height,
                                       0.5f));
    }
}
