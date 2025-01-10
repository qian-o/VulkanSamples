using Graphics.Vulkan;
using Tests.AndroidApp.Views;

namespace Tests.AndroidApp;

public partial class App : Application
{
    private static Context? _context;
    private static PhysicalDevice? _physicalDevice;
    private static GraphicsDevice? _device;

    public static Context Context => _context ??= new();

    public static PhysicalDevice PhysicalDevice => _physicalDevice ??= Context.GetBestPhysicalDevice();

    public static GraphicsDevice Device => _device ??= Context.CreateGraphicsDevice(PhysicalDevice);

    public App()
    {
        InitializeComponent();

        Windows[0].Page = new AppShell();
    }
}
