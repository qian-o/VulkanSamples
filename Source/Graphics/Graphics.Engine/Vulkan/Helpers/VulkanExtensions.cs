using Graphics.Engine.Exceptions;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan.Helpers;

internal static class VulkanExtensions
{
    #region VulkanObject
    public static VKSwapChain VK(this SwapChain swapChain)
    {
        if (swapChain is not VKSwapChain vkSwapChain)
        {
            throw new BackendException("Swap chain is not a Vulkan swap chain!");
        }

        return vkSwapChain;
    }

    public static VKShader VK(this Shader shader)
    {
        if (shader is not VKShader vkShader)
        {
            throw new BackendException("Shader is not a Vulkan shader!");
        }

        return vkShader;
    }

    public static VKBuffer VK(this Buffer buffer)
    {
        if (buffer is not VKBuffer vkBuffer)
        {
            throw new BackendException("Buffer is not a Vulkan buffer!");
        }

        return vkBuffer;
    }

    public static VKTexture VK(this Texture texture)
    {
        if (texture is not VKTexture vkTexture)
        {
            throw new BackendException("Texture is not a Vulkan texture!");
        }

        return vkTexture;
    }

    public static VKTextureView VK(this TextureView textureView)
    {
        if (textureView is not VKTextureView vkTextureView)
        {
            throw new BackendException("Texture view is not a Vulkan texture view!");
        }

        return vkTextureView;
    }

    public static VKSampler VK(this Sampler sampler)
    {
        if (sampler is not VKSampler vkSampler)
        {
            throw new BackendException("Sampler is not a Vulkan sampler!");
        }

        return vkSampler;
    }

    public static VKResourceLayout VK(this ResourceLayout resourceLayout)
    {
        if (resourceLayout is not VKResourceLayout vkResourceLayout)
        {
            throw new BackendException("Resource layout is not a Vulkan resource layout!");
        }

        return vkResourceLayout;
    }

    public static VKResourceSet VK(this ResourceSet resourceSet)
    {
        if (resourceSet is not VKResourceSet vkResourceSet)
        {
            throw new BackendException("Resource set is not a Vulkan resource set!");
        }

        return vkResourceSet;
    }

    public static VKFrameBuffer VK(this FrameBuffer frameBuffer)
    {
        if (frameBuffer is not VKFrameBuffer vkFrameBuffer)
        {
            throw new BackendException("Frame buffer is not a Vulkan frame buffer!");
        }

        return vkFrameBuffer;
    }

    public static VKGraphicsPipeline VK(this GraphicsPipeline pipeline)
    {
        if (pipeline is not VKGraphicsPipeline vkPipeline)
        {
            throw new BackendException("Graphics pipeline is not a Vulkan graphics pipeline!");
        }

        return vkPipeline;
    }
    #endregion

    public static void ThrowCode(this VkResult result, string message = "")
    {
        if (result != VkResult.Success)
        {
            throw new BackendException($"Vulkan error: {result} {message}");
        }
    }

    public static T GetExtension<T>(this Vk vk, VkInstance instance) where T : NativeExtension<Vk>
    {
        if (!vk.TryGetInstanceExtension(instance, out T ext))
        {
            throw new InvalidOperationException($"Failed to load extension {typeof(T).Name}!");
        }

        return ext;
    }

    public static T GetExtension<T>(this Vk vk, VkInstance instance, VkDevice device) where T : NativeExtension<Vk>
    {
        if (!vk.TryGetDeviceExtension(instance, device, out T ext))
        {
            throw new InvalidOperationException($"Failed to load extension {typeof(T).Name}!");
        }

        return ext;
    }
}
