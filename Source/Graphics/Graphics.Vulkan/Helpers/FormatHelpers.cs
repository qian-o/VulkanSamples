using Graphics.Core;

namespace Graphics.Vulkan.Helpers;

internal static class FormatHelpers
{
    public static bool IsStencilFormat(PixelFormat format)
    {
        return format switch
        {
            PixelFormat.D24UNormS8UInt or
            PixelFormat.D32FloatS8UInt => true,

            _ => false
        };
    }
}
