using Graphics.Core;
using Graphics.Core.Helpers;
using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;
using SkiaSharp;

namespace Graphics.Vulkan.Skia;

public static unsafe class SkiaGraphics
{
    public static GRContext CreateContext(GraphicsDevice graphicsDevice)
    {
        PhysicalDeviceProperties2 properties2 = graphicsDevice.VkRes.Properties2;

        GRVkBackendContext backendContext = new()
        {
            VkInstance = graphicsDevice.VkRes.Instance.Handle,
            VkPhysicalDevice = graphicsDevice.VkRes.VkPhysicalDevice.Handle,
            VkDevice = graphicsDevice.Handle.Handle,
            VkQueue = graphicsDevice.GraphicsExecutor.Handle.Handle,
            GraphicsQueueIndex = graphicsDevice.GraphicsExecutor.FamilyIndex,
            MaxAPIVersion = Context.ApiVersion,
            Extensions = GRVkExtensions.Create(GetProcedureAddress,
                                               graphicsDevice.VkRes.Instance.Handle,
                                               graphicsDevice.VkRes.VkPhysicalDevice.Handle,
                                               graphicsDevice.VkRes.InstanceExtensions,
                                               graphicsDevice.VkRes.DeviceExtensions),
            VkPhysicalDeviceFeatures2 = (nint)properties2.AsPointer(),
            GetProcedureAddress = GetProcedureAddress
        };

        GRContext context = GRContext.CreateVulkan(backendContext)
                            ?? throw new InvalidOperationException("Failed to create Vulkan context.");

        context.SetResourceCacheLimit(1024 * 1024 * 80);

        return context;

        nint GetProcedureAddress(string name, nint instance, nint device)
        {
            if (instance != 0)
            {
                return graphicsDevice.VkRes.Vk.GetInstanceProcAddr(graphicsDevice.VkRes.Instance, name);
            }

            if (device != 0)
            {
                return graphicsDevice.VkRes.Vk.GetDeviceProcAddr(graphicsDevice.Handle, name);
            }

            return graphicsDevice.VkRes.Vk.Context.GetProcAddress(name);
        }
    }

    public static SKSurface CreateSurface(GRContext context, Texture texture)
    {
        if (texture.Format != PixelFormat.R8G8B8A8UNorm)
        {
            throw new NotSupportedException("Only R8G8B8A8_UNorm format is supported.");
        }

        if (!texture.Usage.HasFlag(TextureUsage.RenderTarget))
        {
            throw new NotSupportedException("Only RenderTarget usage is supported.");
        }

        GRVkImageInfo imageInfo = new()
        {
            Image = texture.Handle.Handle,
            Alloc = new GRVkAlloc() { Memory = texture.DeviceMemory!.Handle.Handle, Size = texture.DeviceMemory.SizeInBytes },
            ImageTiling = (uint)ImageTiling.Optimal,
            ImageLayout = (uint)texture.ImageLayouts[0],
            Format = (uint)Formats.GetPixelFormat(texture.Format, false),
            ImageUsageFlags = (uint)Formats.GetImageUsageFlags(texture.Usage),
            SampleCount = (uint)Formats.GetSampleCount(texture.SampleCount),
            LevelCount = texture.MipLevels,
            CurrentQueueFamily = texture.VkRes.GraphicsDevice.GraphicsExecutor.FamilyIndex
        };

        GRBackendRenderTarget backendRenderTarget = new((int)texture.Width, (int)texture.Height, (int)imageInfo.SampleCount, imageInfo);

        return SKSurface.Create(context,
                                backendRenderTarget,
                                GRSurfaceOrigin.TopLeft,
                                SKColorType.Rgba8888,
                                SKColorSpace.CreateSrgbLinear());
    }
}
