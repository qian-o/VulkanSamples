using System.Runtime.InteropServices;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.SDL;

namespace Graphics.Windowing.Structs;

public unsafe struct SdlVkSurface(Window* window) : IVkSurface, IDisposable
{
    private byte** requiredExtensions;

    public readonly VkNonDispatchableHandle Create<T>(VkHandle instance, T* allocator) where T : unmanaged
    {
        VkNonDispatchableHandle surface;

        SdlManager.Sdl.VulkanCreateSurface(window, instance, &surface);

        return surface;
    }

    public byte** GetRequiredExtensions(out uint count)
    {
        fixed (uint* countPtr = &count)
        {
            SdlManager.Sdl.VulkanGetInstanceExtensions(window, countPtr, (byte**)0);

            if (requiredExtensions == null)
            {
                requiredExtensions = (byte**)Marshal.AllocHGlobal((int)count * sizeof(byte*));
            }
            else
            {
                requiredExtensions = (byte**)Marshal.ReAllocHGlobal((nint)requiredExtensions, (nint)count * sizeof(byte*));
            }

            SdlManager.Sdl.VulkanGetInstanceExtensions(window, countPtr, requiredExtensions);

            return requiredExtensions;
        }
    }

    public readonly void Dispose()
    {
        if (requiredExtensions != null)
        {
            Marshal.FreeHGlobal((nint)requiredExtensions);
        }
    }
}
