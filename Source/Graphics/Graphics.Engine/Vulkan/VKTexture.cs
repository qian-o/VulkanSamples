using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;
using Graphics.Engine.Helpers;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;
using System.Runtime.CompilerServices;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKTexture : Texture
{
    public VKTexture(Context context,
                     ref readonly TextureDesc desc) : base(context, in desc)
    {
        ImageCreateInfo createInfo = new()
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = Formats.GetImageType(desc.Type),
            Format = Formats.GetPixelFormat(desc.Format),
            Extent = new Extent3D
            {
                Width = desc.Width,
                Height = desc.Height,
                Depth = desc.Depth
            },
            MipLevels = desc.MipLevels,
            ArrayLayers = VKHelpers.GetArrayLayers(desc),
            Samples = Formats.GetSampleCountFlags(desc.SampleCount),
            Tiling = ImageTiling.Optimal,
            Usage = Formats.GetImageUsageFlags(desc.Usage),
            SharingMode = SharingMode.Exclusive,
            InitialLayout = ImageLayout.Preinitialized,
            Flags = desc.Type == TextureType.TextureCube ? ImageCreateFlags.CreateCubeCompatibleBit : ImageCreateFlags.None
        };

        VkImage image;
        Context.Vk.CreateImage(Context.Device, &createInfo, null, &image).ThrowCode();

        MemoryRequirements memoryRequirements;
        Context.Vk.GetImageMemoryRequirements(Context.Device, image, &memoryRequirements);

        DeviceMemory = new(Context, false, memoryRequirements);

        Context.Vk.BindImageMemory(Context.Device, image, DeviceMemory.DeviceMemory, 0).ThrowCode();

        Layouts = new ImageLayout[createInfo.ArrayLayers * desc.MipLevels];
        Array.Fill(Layouts, ImageLayout.Preinitialized);

        Image = image;
    }

    public VKTexture(Context context,
                     VkImage image,
                     ref readonly TextureDesc desc) : base(context, in desc)
    {
        Image = image;
        Layouts = new ImageLayout[VKHelpers.GetArrayLayers(desc) * desc.MipLevels];
        Array.Fill(Layouts, ImageLayout.Undefined);
    }

    public new VKContext Context => (VKContext)base.Context;

    public VkImage Image { get; }

    public VKDeviceMemory? DeviceMemory { get; }

    public ImageLayout[] Layouts { get; }

    public ImageLayout this[uint baseMipLevel, CubeMapFace baseFace]
    {
        get
        {
            return Layouts[((uint)baseFace * Desc.MipLevels) + baseMipLevel];
        }
        set
        {
            Layouts[((uint)baseFace * Desc.MipLevels) + baseMipLevel] = value;
        }
    }

    public void TransitionImageLayout(VkCommandBuffer commandBuffer,
                                      ImageLayout newLayout,
                                      uint baseMipLevel,
                                      uint levelCount,
                                      CubeMapFace baseFace,
                                      uint faceCount)
    {
        if (newLayout == this[baseMipLevel, baseFace]
            || newLayout == ImageLayout.Undefined
            || newLayout == ImageLayout.Preinitialized)
        {
            return;
        }

        ImageMemoryBarrier barrier = new()
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = this[baseMipLevel, baseFace],
            NewLayout = newLayout,
            Image = Image,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = Formats.GetImageAspectFlags(newLayout),
                BaseMipLevel = baseMipLevel,
                LevelCount = levelCount,
                BaseArrayLayer = (uint)baseFace,
                LayerCount = faceCount
            }
        };

        VKHelpers.GetPipelineStageFlagsForImageLayout(ref barrier,
                                                      out PipelineStageFlags src,
                                                      out PipelineStageFlags dst);

        Context.Vk.CmdPipelineBarrier(commandBuffer,
                                      src,
                                      dst,
                                      DependencyFlags.None,
                                      0,
                                      null,
                                      0,
                                      null,
                                      1,
                                      &barrier);

        this[baseMipLevel, baseFace] = newLayout;
    }

    public void TransitionImageLayout(VkCommandBuffer commandBuffer,
                                      ImageLayout newLayout)
    {
        TransitionImageLayout(commandBuffer,
                              newLayout,
                              0,
                              Desc.MipLevels,
                              CubeMapFace.PositiveX,
                              VKHelpers.GetArrayLayers(Desc));
    }

    public void SetData(VkCommandBuffer commandBuffer,
                        nint source,
                        uint sourceSizeInBytes,
                        TextureRegion region)
    {
        Buffer buffer = Context.BufferPool.Buffer(sourceSizeInBytes);

        MappedResource mappedResource = Context.MapMemory(buffer, MapMode.Write);

        Unsafe.CopyBlock((void*)mappedResource.Data,
                         (void*)source,
                         sourceSizeInBytes);

        Context.UnmapMemory(buffer);

        BufferImageCopy bufferImageCopy = new()
        {
            BufferOffset = 0,
            BufferRowLength = 0,
            BufferImageHeight = 0,
            ImageSubresource = new ImageSubresourceLayers
            {
                AspectMask = Formats.GetImageAspectFlags(Desc.Usage),
                MipLevel = region.MipLevel,
                BaseArrayLayer = (uint)region.Face,
                LayerCount = 1
            },
            ImageOffset = new Offset3D
            {
                X = (int)region.X,
                Y = (int)region.Y,
                Z = (int)region.Z
            },
            ImageExtent = new Extent3D
            {
                Width = region.Width,
                Height = region.Height,
                Depth = region.Depth
            }
        };

        ImageLayout oldLayout = this[region.MipLevel, region.Face];

        TransitionImageLayout(commandBuffer,
                              ImageLayout.TransferDstOptimal,
                              region.MipLevel,
                              1,
                              region.Face,
                              1);

        Context.Vk.CmdCopyBufferToImage(commandBuffer,
                                        buffer.VK().Buffer,
                                        Image,
                                        ImageLayout.TransferDstOptimal,
                                        1,
                                        &bufferImageCopy);

        TransitionImageLayout(commandBuffer,
                              oldLayout,
                              region.MipLevel,
                              1,
                              region.Face,
                              1);
    }

    public void CopyTo(VkCommandBuffer commandBuffer,
                       TextureRegion sourceRegion,
                       VKTexture vkDestination,
                       TextureRegion destinationRegion)
    {
        ImageLayout sourceOldLayout = this[sourceRegion.MipLevel, sourceRegion.Face];
        ImageLayout destinationOldLayout = vkDestination[destinationRegion.MipLevel, destinationRegion.Face];

        TransitionImageLayout(commandBuffer,
                              ImageLayout.TransferSrcOptimal,
                              sourceRegion.MipLevel,
                              1,
                              sourceRegion.Face,
                              1);

        vkDestination.TransitionImageLayout(commandBuffer,
                                            ImageLayout.TransferDstOptimal,
                                            destinationRegion.MipLevel,
                                            1,
                                            destinationRegion.Face,
                                            1);

        if (sourceRegion.SizeEquals(destinationRegion))
        {
            ImageCopy imageCopy = new()
            {
                SrcSubresource = new ImageSubresourceLayers
                {
                    AspectMask = Formats.GetImageAspectFlags(Desc.Usage),
                    MipLevel = sourceRegion.MipLevel,
                    BaseArrayLayer = (uint)sourceRegion.Face,
                    LayerCount = 1
                },
                SrcOffset = new Offset3D
                {
                    X = (int)sourceRegion.X,
                    Y = (int)sourceRegion.Y,
                    Z = (int)sourceRegion.Z
                },
                DstSubresource = new ImageSubresourceLayers
                {
                    AspectMask = Formats.GetImageAspectFlags(vkDestination.Desc.Usage),
                    MipLevel = destinationRegion.MipLevel,
                    BaseArrayLayer = (uint)destinationRegion.Face,
                    LayerCount = 1
                },
                DstOffset = new Offset3D
                {
                    X = (int)destinationRegion.X,
                    Y = (int)destinationRegion.Y,
                    Z = (int)destinationRegion.Z
                },
                Extent = new Extent3D
                {
                    Width = sourceRegion.Width,
                    Height = sourceRegion.Height,
                    Depth = sourceRegion.Depth
                }
            };

            Context.Vk.CmdCopyImage(commandBuffer,
                                    Image,
                                    ImageLayout.TransferSrcOptimal,
                                    vkDestination.Image,
                                    ImageLayout.TransferDstOptimal,
                                    1,
                                    &imageCopy);
        }
        else
        {
            ImageBlit imageBlit = new()
            {
                SrcSubresource = new ImageSubresourceLayers
                {
                    AspectMask = Formats.GetImageAspectFlags(Desc.Usage),
                    MipLevel = sourceRegion.MipLevel,
                    BaseArrayLayer = (uint)sourceRegion.Face,
                    LayerCount = 1
                },
                SrcOffsets = new ImageBlit.SrcOffsetsBuffer
                {
                    Element0 = new Offset3D
                    {
                        X = (int)sourceRegion.X,
                        Y = (int)sourceRegion.Y,
                        Z = (int)sourceRegion.Z
                    },
                    Element1 = new Offset3D
                    {
                        X = (int)sourceRegion.Width,
                        Y = (int)sourceRegion.Height,
                        Z = (int)sourceRegion.Depth
                    }
                },
                DstSubresource = new ImageSubresourceLayers
                {
                    AspectMask = Formats.GetImageAspectFlags(vkDestination.Desc.Usage),
                    MipLevel = destinationRegion.MipLevel,
                    BaseArrayLayer = (uint)destinationRegion.Face,
                    LayerCount = 1
                },
                DstOffsets = new ImageBlit.DstOffsetsBuffer
                {
                    Element0 = new Offset3D
                    {
                        X = (int)destinationRegion.X,
                        Y = (int)destinationRegion.Y,
                        Z = (int)destinationRegion.Z
                    },
                    Element1 = new Offset3D
                    {
                        X = (int)destinationRegion.Width,
                        Y = (int)destinationRegion.Height,
                        Z = (int)destinationRegion.Depth
                    }
                }
            };

            Context.Vk.CmdBlitImage(commandBuffer,
                                   Image,
                                   ImageLayout.TransferSrcOptimal,
                                   vkDestination.Image,
                                   ImageLayout.TransferDstOptimal,
                                   1,
                                   &imageBlit,
                                   Filter.Linear);
        }

        TransitionImageLayout(commandBuffer,
                              sourceOldLayout,
                              sourceRegion.MipLevel,
                              1,
                              sourceRegion.Face,
                              1);

        vkDestination.TransitionImageLayout(commandBuffer,
                                            destinationOldLayout,
                                            destinationRegion.MipLevel,
                                            1,
                                            destinationRegion.Face,
                                            1);
    }

    public void ResolveTo(VkCommandBuffer commandBuffer,
                          TexturePosition sourcePosition,
                          VKTexture vkDestination,
                          TexturePosition destinationPosition)
    {
        Utils.GetMipDimensions(Desc.Width,
                               Desc.Height,
                               Desc.Depth,
                               sourcePosition.MipLevel,
                               out uint width,
                               out uint height,
                               out uint depth);

        ImageLayout sourceOldLayout = this[sourcePosition.MipLevel, sourcePosition.Face];
        ImageLayout destinationOldLayout = vkDestination[destinationPosition.MipLevel, destinationPosition.Face];

        TransitionImageLayout(commandBuffer,
                              ImageLayout.TransferSrcOptimal,
                              sourcePosition.MipLevel,
                              1,
                              sourcePosition.Face,
                              1);

        vkDestination.TransitionImageLayout(commandBuffer,
                                            ImageLayout.TransferDstOptimal,
                                            destinationPosition.MipLevel,
                                            1,
                                            destinationPosition.Face,
                                            1);

        ImageResolve imageResolve = new()
        {
            SrcSubresource = new ImageSubresourceLayers
            {
                AspectMask = Formats.GetImageAspectFlags(Desc.Usage),
                MipLevel = sourcePosition.MipLevel,
                BaseArrayLayer = (uint)sourcePosition.Face,
                LayerCount = 1
            },
            SrcOffset = new Offset3D
            {
                X = (int)sourcePosition.X,
                Y = (int)sourcePosition.Y,
                Z = (int)sourcePosition.Z
            },
            DstSubresource = new ImageSubresourceLayers
            {
                AspectMask = Formats.GetImageAspectFlags(vkDestination.Desc.Usage),
                MipLevel = destinationPosition.MipLevel,
                BaseArrayLayer = (uint)destinationPosition.Face,
                LayerCount = 1
            },
            DstOffset = new Offset3D
            {
                X = (int)destinationPosition.X,
                Y = (int)destinationPosition.Y,
                Z = (int)destinationPosition.Z
            },
            Extent = new Extent3D
            {
                Width = width,
                Height = height,
                Depth = depth
            }
        };

        Context.Vk.CmdResolveImage(commandBuffer,
                                  Image,
                                  ImageLayout.TransferSrcOptimal,
                                  vkDestination.Image,
                                  ImageLayout.TransferDstOptimal,
                                  1,
                                  &imageResolve);

        TransitionImageLayout(commandBuffer,
                              sourceOldLayout,
                              sourcePosition.MipLevel,
                              1,
                              sourcePosition.Face,
                              1);

        vkDestination.TransitionImageLayout(commandBuffer,
                                            destinationOldLayout,
                                            destinationPosition.MipLevel,
                                            1,
                                            destinationPosition.Face,
                                            1);
    }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.Image, Image.Handle, name);

        if (DeviceMemory != null)
        {
            DeviceMemory.Name = name;
        }
    }

    protected override void Destroy()
    {
        DeviceMemory?.Dispose();

        Context.Vk.DestroyImage(Context.Device, Image, null);
    }
}
