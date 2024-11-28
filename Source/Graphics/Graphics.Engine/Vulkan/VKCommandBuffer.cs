using System.Numerics;
using Graphics.Core.Helpers;
using Graphics.Engine.Enums;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using VkViewport = Silk.NET.Vulkan.Viewport;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKCommandBuffer : CommandBuffer
{
    private VKFrameBuffer? activeFrameBuffer;
    private Pipeline? activePipeline;

    public VKCommandBuffer(Context context, VKCommandProcessor processor) : base(context)
    {
        Processor = processor;

        CommandPoolCreateInfo createInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = Processor.FamilyIndex,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit
        };

        VkCommandPool commandPool;
        Context.Vk.CreateCommandPool(Context.Device, &createInfo, null, &commandPool).ThrowCode();

        CommandBufferAllocateInfo allocateInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = commandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };

        VkCommandBuffer commandBuffer;
        Context.Vk.AllocateCommandBuffers(Context.Device, &allocateInfo, &commandBuffer).ThrowCode();

        CommandPool = commandPool;
        CommandBuffer = commandBuffer;
    }

    public new VKContext Context => (VKContext)base.Context;

    public VKCommandProcessor Processor { get; }

    public VkCommandPool CommandPool { get; }

    public VkCommandBuffer CommandBuffer { get; }

    public override void Begin()
    {
        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };

        Context.Vk.BeginCommandBuffer(CommandBuffer, &beginInfo).ThrowCode();

        activeFrameBuffer = null;
    }

    public override void Reset()
    {
        Context.Vk.ResetCommandBuffer(CommandBuffer, CommandBufferResetFlags.None).ThrowCode();
    }

    public override void Commit()
    {
        Processor.CommitCommandBuffer(this);
    }

    public override void UpdateBufferData(Buffer buffer,
                                          nint source,
                                          uint sourceSizeInBytes,
                                          uint destinationOffsetInBytes = 0)
    {
        VKBuffer vkBuffer = buffer.VK();

        vkBuffer.SetData(CommandBuffer, source, sourceSizeInBytes, destinationOffsetInBytes);
    }

    public override void CopyBuffer(Buffer source,
                                    Buffer destination,
                                    uint sourceSizeInBytes,
                                    uint destinationOffsetInBytes = 0)
    {
        VKBuffer vkSource = source.VK();
        VKBuffer vkDestination = destination.VK();

        vkSource.CopyTo(CommandBuffer, vkDestination, sourceSizeInBytes, destinationOffsetInBytes);
    }

    public override void UpdateTextureData(Texture texture,
                                           nint source,
                                           uint sourceSizeInBytes,
                                           TextureRegion region)
    {
        VKTexture vkTexture = texture.VK();

        vkTexture.SetData(CommandBuffer, source, sourceSizeInBytes, region);
    }

    public override void CopyTexture(Texture source,
                                     TextureRegion sourceRegion,
                                     Texture destination,
                                     TextureRegion destinationRegion)
    {
        VKTexture vkSource = source.VK();
        VKTexture vkDestination = destination.VK();

        vkSource.CopyTo(CommandBuffer, sourceRegion, vkDestination, destinationRegion);
    }

    public override void ResolveTexture(Texture source,
                                        TexturePosition sourcePosition,
                                        Texture destination,
                                        TexturePosition destinationPosition)
    {
        VKTexture vkSource = source.VK();
        VKTexture vkDestination = destination.VK();

        vkSource.ResolveTo(CommandBuffer, sourcePosition, vkDestination, destinationPosition);
    }

    public override void BeginRendering(FrameBuffer frameBuffer, ClearValue clearValue)
    {
        activeFrameBuffer?.TransitionToFinalLayout(CommandBuffer);

        activeFrameBuffer = frameBuffer.VK();
        activeFrameBuffer.TransitionToIntermedialLayout(CommandBuffer);

        Context.Vk.CmdBeginRendering(CommandBuffer, in activeFrameBuffer.RenderingInfo);

        bool clearColor = clearValue.Options.HasFlag(ClearOptions.Color);
        bool clearDepth = clearValue.Options.HasFlag(ClearOptions.Depth);
        bool clearStencil = clearValue.Options.HasFlag(ClearOptions.Stencil);

        if (clearColor)
        {
            for (int i = 0; i < activeFrameBuffer.ColorTargets.Length; i++)
            {
                Vector4 color = clearValue.ColorValues[i];

                ClearAttachment clearAttachment = new()
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    ColorAttachment = (uint)i,
                    ClearValue = new()
                    {
                        Color = new()
                        {
                            Float32_0 = color.X,
                            Float32_1 = color.Y,
                            Float32_2 = color.Z,
                            Float32_3 = color.W
                        }
                    }
                };

                Texture colorTarget = activeFrameBuffer.ColorTargets[i].Desc.Target;

                ClearRect clearRect = new()
                {
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                    Rect = new()
                    {
                        Offset = new()
                        {
                            X = 0,
                            Y = 0
                        },
                        Extent = new()
                        {
                            Width = colorTarget.Desc.Width,
                            Height = colorTarget.Desc.Height
                        }
                    }
                };

                Context.Vk.CmdClearAttachments(CommandBuffer, 1, &clearAttachment, 1, &clearRect);
            }
        }

        if (activeFrameBuffer.DepthStencilTarget != null && (clearDepth || clearStencil))
        {
            ImageAspectFlags aspectMask = 0;

            if (clearDepth)
            {
                aspectMask |= ImageAspectFlags.DepthBit;
            }

            if (clearStencil)
            {
                aspectMask |= ImageAspectFlags.StencilBit;
            }

            ClearAttachment clearAttachment = new()
            {
                AspectMask = aspectMask,
                ClearValue = new()
                {
                    DepthStencil = new()
                    {
                        Depth = clearValue.Depth,
                        Stencil = clearValue.Stencil
                    }
                }
            };

            Texture depthStencilTarget = activeFrameBuffer.DepthStencilTarget.Desc.Target;

            ClearRect clearRect = new()
            {
                BaseArrayLayer = 0,
                LayerCount = 1,
                Rect = new()
                {
                    Offset = new()
                    {
                        X = 0,
                        Y = 0
                    },
                    Extent = new()
                    {
                        Width = depthStencilTarget.Desc.Width,
                        Height = depthStencilTarget.Desc.Height
                    }
                }
            };

            Context.Vk.CmdClearAttachments(CommandBuffer, 1, &clearAttachment, 1, &clearRect);
        }

        SetFullViewports();
        SetFullScissorRectangles();
    }

    public override void EndRendering()
    {
        Context.Vk.CmdEndRendering(CommandBuffer);

        activeFrameBuffer?.TransitionToFinalLayout(CommandBuffer);
    }

    public override void SetViewports(Viewport[] viewports)
    {
        VkViewport[] vkViewports = viewports.Select(viewport => new VkViewport
        {
            X = viewport.X,
            Y = viewport.Y + viewport.Height,
            Width = viewport.Width,
            Height = -viewport.Height,
            MinDepth = viewport.MinDepth,
            MaxDepth = viewport.MaxDepth
        }).ToArray();

        Context.Vk.CmdSetViewport(CommandBuffer, 0, (uint)vkViewports.Length, vkViewports.AsPointer());
    }

    public override void SetScissorRectangles(Rectangle<int>[] scissors)
    {
        Rect2D[] vkScissors = scissors.Select(scissor => new Rect2D
        {
            Offset = new Offset2D
            {
                X = scissor.Origin.X,
                Y = scissor.Origin.Y
            },
            Extent = new Extent2D
            {
                Width = (uint)scissor.Size.X,
                Height = (uint)scissor.Size.Y
            }
        }).ToArray();

        Context.Vk.CmdSetScissor(CommandBuffer, 0, (uint)vkScissors.Length, vkScissors.AsPointer());
    }

    public override void SetFullViewports(float minDepth = 0, float maxDepth = 1)
    {
        Viewport[] viewports = new Viewport[activeFrameBuffer!.ColorTargets.Length];
        Array.Fill(viewports, new Viewport(0, 0, activeFrameBuffer.Width, activeFrameBuffer.Height, minDepth, maxDepth));

        SetViewports(viewports);
    }

    public override void SetFullScissorRectangles()
    {
        Rectangle<int>[] scissors = new Rectangle<int>[activeFrameBuffer!.ColorTargets.Length];
        Array.Fill(scissors, new Rectangle<int>(0, 0, (int)activeFrameBuffer.Width, (int)activeFrameBuffer.Height));

        SetScissorRectangles(scissors);
    }

    public override void SetGraphicsPipeline(GraphicsPipeline pipeline)
    {
        Context.Vk.CmdBindPipeline(CommandBuffer, PipelineBindPoint.Graphics, pipeline.VK().Pipeline);

        activePipeline = pipeline;
    }

    public override void SetVertexBuffer(uint slot, Buffer buffer, uint offset = 0)
    {
        VkBuffer vkBuffer = buffer.VK().Buffer;

        ulong vkOffset = offset;

        Context.Vk.CmdBindVertexBuffers(CommandBuffer, slot, 1, &vkBuffer, &vkOffset);
    }

    public override void SetIndexBuffer(Buffer buffer,
                                        IndexFormat format = IndexFormat.U16Bit,
                                        uint offset = 0)
    {
        VkBuffer vkBuffer = buffer.VK().Buffer;

        IndexType vkFormat = format switch
        {
            IndexFormat.U16Bit => IndexType.Uint16,
            IndexFormat.U32Bit => IndexType.Uint32,
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

        Context.Vk.CmdBindIndexBuffer(CommandBuffer, vkBuffer, offset, vkFormat);
    }

    public override void SetResourceSet(ResourceSet resourceSet,
                                        uint index = 0,
                                        uint[]? constantBufferOffsets = null)
    {
        VKResourceSet vKResourceSet = resourceSet.VK();

        foreach (VKTexture texture in vKResourceSet.StorageTextures)
        {
            texture.TransitionImageLayout(CommandBuffer, ImageLayout.General);
        }

        foreach (VKTexture texture in vKResourceSet.Textures)
        {
            texture.TransitionImageLayout(CommandBuffer, ImageLayout.ShaderReadOnlyOptimal);
        }

        VkDescriptorSet descriptorSet = vKResourceSet.DescriptorSet;
        uint[] offsets = constantBufferOffsets ?? [];

        if (activePipeline is VKGraphicsPipeline graphicsPipeline)
        {
            Context.Vk.CmdBindDescriptorSets(CommandBuffer,
                                             PipelineBindPoint.Graphics,
                                             graphicsPipeline.PipelineLayout,
                                             index,
                                             1,
                                             &descriptorSet,
                                             (uint)offsets.Length,
                                             offsets);
        }
        else
        {
            throw new InvalidOperationException("The active pipeline type is not supported.");
        }
    }

    public override void DrawInstanced(uint vertexCountPerInstance,
                                       uint instanceCount,
                                       uint startVertexLocation = 0,
                                       uint startInstanceLocation = 0)
    {
        Context.Vk.CmdDraw(CommandBuffer, vertexCountPerInstance, instanceCount, startVertexLocation, startInstanceLocation);
    }

    public override void DrawInstancedIndirect(Buffer argBuffer,
                                               uint offset,
                                               uint drawCount,
                                               uint stride)
    {
        VkBuffer vkBuffer = argBuffer.VK().Buffer;

        Context.Vk.CmdDrawIndirect(CommandBuffer, vkBuffer, offset, drawCount, stride);
    }

    public override void DrawIndexed(uint indexCount,
                                     uint startIndexLocation = 0,
                                     uint baseVertexLocation = 0)
    {
        Context.Vk.CmdDrawIndexed(CommandBuffer, indexCount, 1, startIndexLocation, (int)baseVertexLocation, 0);
    }

    public override void DrawIndexedInstanced(uint indexCountPerInstance,
                                              uint instanceCount,
                                              uint startIndexLocation = 0,
                                              uint baseVertexLocation = 0,
                                              uint startInstanceLocation = 0)
    {
        Context.Vk.CmdDrawIndexed(CommandBuffer, indexCountPerInstance, instanceCount, startIndexLocation, (int)baseVertexLocation, startInstanceLocation);
    }

    public override void DrawIndexedInstancedIndirect(Buffer argBuffer,
                                                      uint offset,
                                                      uint drawCount,
                                                      uint stride)
    {
        VkBuffer vkBuffer = argBuffer.VK().Buffer;

        Context.Vk.CmdDrawIndexedIndirect(CommandBuffer, vkBuffer, offset, drawCount, stride);
    }

    public override void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
    {
        Context.Vk.CmdDispatch(CommandBuffer, groupCountX, groupCountY, groupCountZ);
    }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.CommandPool, CommandPool.Handle, name);
        Context.SetDebugName(ObjectType.CommandBuffer, (ulong)CommandBuffer.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyCommandPool(Context.Device, CommandPool, null);
    }

    protected override void ClearCache()
    {
    }

    protected override void EndInternal()
    {
        Context.Vk.EndCommandBuffer(CommandBuffer).ThrowCode();
    }
}
