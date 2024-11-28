using Graphics.Core;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.ImGui;
using Graphics.Windowing;
using Graphics.Windowing.Events;
using Hexa.NET.ImGui;
using Silk.NET.Maths;

internal sealed unsafe class Program
{
    private static SdlWindow mainWindow = null!;
    private static Context context = null!;
    private static GraphicsDevice device = null!;
    private static Swapchain swapchain = null!;
    private static ImGuiController imGuiController = null!;
    private static CommandList commandList = null!;

    private static void Main(string[] _)
    {
        mainWindow = new()
        {
            Title = "Tests.MultiViewports",
            MinimumSize = new(100, 100)
        };

        mainWindow.Loaded += Loaded;
        mainWindow.Unloaded += Unloaded;
        mainWindow.SizeChanged += SizeChanged;
        mainWindow.Update += Update;
        mainWindow.Render += Render;

        mainWindow.Show();

        WindowManager.Loop();
    }

    private static void Loaded(object? sender, EventArgs e)
    {
        context = new();
        device = context.CreateGraphicsDevice(context.GetBestPhysicalDevice());
        swapchain = device.Factory.CreateSwapchain(new SwapchainDescription(mainWindow.Surface!, device.GetBestDepthFormat()));
        imGuiController = new(mainWindow,
                              () => new SdlWindow(),
                              device,
                              swapchain.OutputDescription,
                              new ImGuiFontConfig("Assets/Fonts/msyh.ttf", 16, (a) => (nint)a.Fonts.GetGlyphRangesChineseFull()),
                              ImGuiSizeConfig.Default);
        commandList = device.Factory.CreateGraphicsCommandList();
    }

    private static void Unloaded(object? sender, EventArgs e)
    {
        commandList.Dispose();
        imGuiController.Dispose();
        swapchain.Dispose();
        device.Dispose();
        context.Dispose();

        WindowManager.Stop();
    }

    private static void SizeChanged(object? sender, ValueEventArgs<Vector2D<int>> e)
    {
        swapchain.Resize();
    }

    private static void Update(object? sender, TimeEventArgs e)
    {
        imGuiController.Update((float)e.DeltaTime);
    }

    private static void Render(object? sender, TimeEventArgs e)
    {
        ImGui.ShowDemoWindow();

        commandList.Begin();
        {
            commandList.SetFramebuffer(swapchain.Framebuffer);
            commandList.ClearColorTarget(0, RgbaFloat.Black);
            commandList.ClearDepthStencil(1.0f);

            imGuiController.Render(commandList);
        }
        commandList.End();

        device.SubmitCommandsAndSwapBuffers(commandList, swapchain);

        imGuiController.PlatformSwapBuffers();
    }
}