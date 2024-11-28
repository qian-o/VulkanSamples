using System.Runtime.InteropServices;

namespace Tests.AndroidApp.Platforms.Android.Helpers;

internal static unsafe partial class NativeActivity
{
    private const string LibName = "android";

    [LibraryImport(LibName, EntryPoint = "ANativeWindow_fromSurface")]
    internal static partial ANativeWindow* ANativeWindowFromSurface(nint env, nint surface);

    [LibraryImport(LibName, EntryPoint = "ANativeWindow_release")]
    internal static partial void ANativeWindowRelease(ANativeWindow* window);
}
