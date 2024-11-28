using Graphics.Core;
using Graphics.Core.Helpers;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Framebuffer : VulkanObject<VkFramebuffer>
{
    internal Framebuffer(VulkanResources vkRes,
                         ref readonly FramebufferDescription description,
                         bool isPresented) : base(vkRes, ObjectType.Framebuffer)
    {
        bool hasDepth = description.DepthTarget.HasValue;

        uint colorAttachmentCount = (uint)description.ColorTargets.Length;
        uint depthAttachmentCount = hasDepth ? 1u : 0u;
        uint attachmentCount = colorAttachmentCount + depthAttachmentCount;

        AttachmentDescription[] attachments = new AttachmentDescription[attachmentCount];
        AttachmentReference[] references = new AttachmentReference[attachmentCount];

        for (uint i = 0; i < colorAttachmentCount; i++)
        {
            Texture colorTarget = description.ColorTargets[i].Target;

            attachments[i] = new AttachmentDescription
            {
                Format = colorTarget.VkFormat,
                Samples = colorTarget.VkSampleCount,
                LoadOp = AttachmentLoadOp.Load,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.ColorAttachmentOptimal,
                FinalLayout = ImageLayout.ColorAttachmentOptimal
            };

            references[i] = new AttachmentReference
            {
                Attachment = i,
                Layout = ImageLayout.ColorAttachmentOptimal
            };
        }

        if (hasDepth)
        {
            Texture depthTarget = description.DepthTarget!.Value.Target;

            bool hasStencil = FormatHelpers.IsStencilFormat(depthTarget.Format);

            attachments[^1] = new AttachmentDescription
            {
                Format = depthTarget.VkFormat,
                Samples = depthTarget.VkSampleCount,
                LoadOp = AttachmentLoadOp.Load,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = hasStencil ? AttachmentLoadOp.Load : AttachmentLoadOp.DontCare,
                StencilStoreOp = hasStencil ? AttachmentStoreOp.Store : AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.DepthStencilAttachmentOptimal,
                FinalLayout = ImageLayout.DepthStencilAttachmentOptimal
            };

            references[^1] = new AttachmentReference
            {
                Attachment = attachmentCount - 1,
                Layout = ImageLayout.DepthStencilAttachmentOptimal
            };
        }

        SubpassDescription subpass = new()
        {
            PipelineBindPoint = PipelineBindPoint.Graphics
        };

        if (colorAttachmentCount > 0)
        {
            subpass.ColorAttachmentCount = colorAttachmentCount;
            subpass.PColorAttachments = references.AsPointer();
        }

        if (hasDepth)
        {
            subpass.PDepthStencilAttachment = references[^1].AsPointer();
        }

        SubpassDependency subpassDependency = new()
        {
            SrcSubpass = Vk.SubpassExternal,
            DstSubpass = 0,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            SrcAccessMask = AccessFlags.None,
            DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            DstAccessMask = AccessFlags.ColorAttachmentReadBit | AccessFlags.ColorAttachmentWriteBit
        };

        RenderPassCreateInfo createInfo = new()
        {
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = attachmentCount,
            PAttachments = attachments.AsPointer(),
            SubpassCount = 1,
            PSubpasses = &subpass,
            DependencyCount = 1,
            PDependencies = &subpassDependency
        };

        VkRenderPass renderPassClear;
        VkRes.Vk.CreateRenderPass(VkRes.VkDevice, &createInfo, null, &renderPassClear).ThrowCode();

        Texture[] colors = new Texture[colorAttachmentCount];
        TextureView[] colorViews = new TextureView[colorAttachmentCount];
        for (int i = 0; i < description.ColorTargets.Length; i++)
        {
            FramebufferAttachmentDescription attachmentDescription = description.ColorTargets[i];

            TextureViewDescription colorDescription = new(attachmentDescription.Target,
                                                          attachmentDescription.MipLevel,
                                                          attachmentDescription.ArrayLayer);

            colors[i] = attachmentDescription.Target;
            colorViews[i] = new TextureView(VkRes, in colorDescription);
        }

        Texture? depth = null;
        TextureView? depthView = null;
        if (hasDepth)
        {
            FramebufferAttachmentDescription attachmentDescription = description.DepthTarget!.Value;

            TextureViewDescription depthDescription = new(attachmentDescription.Target,
                                                          attachmentDescription.MipLevel,
                                                          attachmentDescription.ArrayLayer);

            depth = attachmentDescription.Target;
            depthView = new TextureView(VkRes, in depthDescription);
        }

        VkImageView[] imageViews = new VkImageView[attachmentCount];
        for (int i = 0; i < colorViews.Length; i++)
        {
            imageViews[i] = colorViews[i].Handle;
        }

        if (hasDepth)
        {
            imageViews[^1] = depthView!.Handle;
        }

        FramebufferAttachmentDescription framebufferAttachment;
        if (description.ColorTargets.Length > 0)
        {
            framebufferAttachment = description.ColorTargets[0];
        }
        else
        {
            framebufferAttachment = description.DepthTarget!.Value;
        }

        Util.GetMipDimensions(framebufferAttachment.Target,
                              framebufferAttachment.MipLevel,
                              out uint width,
                              out uint height,
                              out uint _);

        FramebufferCreateInfo framebufferCreateInfo = new()
        {
            SType = StructureType.FramebufferCreateInfo,
            RenderPass = renderPassClear,
            AttachmentCount = attachmentCount,
            PAttachments = imageViews.AsPointer(),
            Width = width,
            Height = height,
            Layers = 1
        };

        VkFramebuffer framebuffer;
        VkRes.Vk.CreateFramebuffer(VkRes.VkDevice, &framebufferCreateInfo, null, &framebuffer).ThrowCode();

        Handle = framebuffer;
        RenderPass = renderPassClear;
        ColorAttachmentCount = colorAttachmentCount;
        DepthAttachmentCount = depthAttachmentCount;
        AttachmentCount = attachmentCount;
        Colors = colors;
        Depth = depth;
        ColorViews = colorViews;
        DepthView = depthView;
        IsPresented = isPresented;
        Width = width;
        Height = height;
        OutputDescription = OutputDescription.CreateFromFramebufferDescription(in description);
    }

    internal override VkFramebuffer Handle { get; }

    internal VkRenderPass RenderPass { get; }

    internal uint ColorAttachmentCount { get; }

    internal uint DepthAttachmentCount { get; }

    internal uint AttachmentCount { get; }

    internal Texture[] Colors { get; }

    internal Texture? Depth { get; }

    internal TextureView[] ColorViews { get; }

    internal TextureView? DepthView { get; }

    internal bool IsPresented { get; }

    public uint Width { get; }

    public uint Height { get; }

    public OutputDescription OutputDescription { get; }

    internal void TransitionToInitialLayout(CommandBuffer commandBuffer)
    {
        for (int i = 0; i < ColorViews.Length; i++)
        {
            Texture color = Colors[i];

            color.TransitionLayout(commandBuffer, ImageLayout.ColorAttachmentOptimal);
        }

        Depth?.TransitionLayout(commandBuffer, ImageLayout.DepthStencilAttachmentOptimal);
    }

    internal void TransitionToFinalLayout(CommandBuffer commandBuffer)
    {
        for (int i = 0; i < ColorViews.Length; i++)
        {
            Texture color = Colors[i];

            ImageLayout finalLayout = IsPresented
                ? ImageLayout.PresentSrcKhr
                : color.Usage.HasFlag(TextureUsage.Sampled) ? ImageLayout.ShaderReadOnlyOptimal : ImageLayout.ColorAttachmentOptimal;

            color.TransitionLayout(commandBuffer, finalLayout);
        }

        if (Depth != null)
        {
            ImageLayout finalLayout = Depth.Usage.HasFlag(TextureUsage.Sampled)
                ? ImageLayout.ShaderReadOnlyOptimal
                : ImageLayout.DepthStencilAttachmentOptimal;

            Depth.TransitionLayout(commandBuffer, finalLayout);
        }
    }

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    internal override void DestroyObject()
    {
        VkRes.Vk.DestroyFramebuffer(VkRes.VkDevice, Handle, null);

        DepthView?.Dispose();

        foreach (TextureView colorView in ColorViews)
        {
            colorView.Dispose();
        }

        VkRes.Vk.DestroyRenderPass(VkRes.VkDevice, RenderPass, null);
    }
}
