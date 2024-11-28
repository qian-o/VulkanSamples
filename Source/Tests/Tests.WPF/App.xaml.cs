using System.Windows;
using Graphics.Vulkan;

namespace Tests.WPF;

public partial class App : Application
{
    static App()
    {
        Context = new Context();
        Device = Context.CreateGraphicsDevice(Context.GetBestPhysicalDevice());
    }

    public static Context Context { get; }

    public static GraphicsDevice Device { get; }

    public static ResourceFactory Factory => Device.Factory;
}

