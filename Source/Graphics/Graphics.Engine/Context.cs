using Graphics.Core;
using Graphics.Engine.Enums;
using Graphics.Engine.Vulkan;

namespace Graphics.Engine;

public abstract class Context : DisposableObject
{
    public abstract Backend Backend { get; }

    public abstract DeviceCapabilities Capabilities { get; }

    public abstract ResourceFactory Factory { get; }

    public abstract void CreateDevice(bool useValidationLayers = false);

    public abstract void UpdateBufferData(Buffer buffer,
                                          nint source,
                                          uint sourceSizeInBytes,
                                          uint destinationOffsetInBytes = 0);

    public abstract void UpdateTextureData(Texture texture,
                                           nint source,
                                           uint sourceSizeInBytes,
                                           TextureRegion region);

    public abstract MappedResource MapMemory(Buffer buffer, MapMode mode);

    public abstract void UnmapMemory(Buffer buffer);

    public abstract void SyncUpToGpu();

    public static Context Create(Backend backend)
    {
        return backend switch
        {
            Backend.Vulkan => new VKContext(),
            _ => throw new NotSupportedException()
        };
    }
}
