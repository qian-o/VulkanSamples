using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan.Helpers;

internal static class VkEx
{
    public static void ThrowCode(this Result result, string message = "")
    {
        if (result != Result.Success)
        {
            throw new GraphicsException($"Vulkan error: {result} {message}");
        }
    }
}
