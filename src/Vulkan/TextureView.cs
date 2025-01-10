using Graphics.Core;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class TextureView : VulkanObject<ImageView>, IBindableResource
{
    internal TextureView(VulkanResources vkRes, ref readonly TextureViewDescription description) : base(vkRes, ObjectType.ImageView)
    {
        ImageViewCreateInfo createInfo = new()
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = description.Target.Handle,
            Format = Formats.GetPixelFormat(description.Target.Format, description.Target.Usage.HasFlag(TextureUsage.DepthStencil))
        };

        ImageAspectFlags aspectFlags = description.Target.Usage.HasFlag(TextureUsage.DepthStencil) ? ImageAspectFlags.DepthBit : ImageAspectFlags.ColorBit;

        createInfo.SubresourceRange = new ImageSubresourceRange
        {
            AspectMask = aspectFlags,
            BaseMipLevel = description.BaseMipLevel,
            LevelCount = description.MipLevels,
            BaseArrayLayer = description.BaseArrayLayer,
            LayerCount = description.ArrayLayers
        };

        if (description.Target.Usage.HasFlag(TextureUsage.Cubemap))
        {
            createInfo.ViewType = ImageViewType.TypeCube;
        }
        else
        {
            switch (description.Target.Type)
            {
                case TextureType.Texture1D:
                    createInfo.ViewType = ImageViewType.Type1D;
                    break;
                case TextureType.Texture2D:
                    createInfo.ViewType = ImageViewType.Type2D;
                    break;
                case TextureType.Texture3D:
                    createInfo.ViewType = ImageViewType.Type3D;
                    break;
            }
        }

        VkImageView imageView;
        VkRes.Vk.CreateImageView(VkRes.VkDevice, &createInfo, null, &imageView).ThrowCode();

        Handle = imageView;
        Target = description.Target;
    }

    internal override VkImageView Handle { get; }

    internal Texture Target { get; }

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    internal override void DestroyObject()
    {
        VkRes.Vk.DestroyImageView(VkRes.VkDevice, Handle, null);
    }
}
