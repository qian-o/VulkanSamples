using Graphics.Core;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.ImGui;
using Graphics.Windowing;
using Graphics.Windowing.Events;
using Hexa.NET.ImGui;
using Silk.NET.Maths;
using Tests.Core;

namespace Tests.RayTracing;

internal sealed unsafe class Program
{
    private static SdlWindow mainWindow = null!;
    private static Context context = null!;
    private static GraphicsDevice device = null!;
    private static Swapchain swapchain = null!;
    private static ImGuiController imGuiController = null!;
    private static CommandList commandList = null!;
    private static View[] views = null!;

    private static void Main(string[] _)
    {
        mainWindow = new()
        {
            Title = "Tests.RayTracing",
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

        views = [new MainView(device, imGuiController)];
    }

    private static void Unloaded(object? sender, EventArgs e)
    {
        foreach (View view in views)
        {
            view.Dispose();
        }

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

        foreach (View view in views)
        {
            view.Update(e);
        }
    }

    private static void Render(object? sender, TimeEventArgs e)
    {
        foreach (View view in views)
        {
            view.Render(e);
        }

        ImGui.Begin("Tests.RayTracing");
        {
            ImGui.Text($"FPS: {1.0f / e.DeltaTime}");

            ImGui.Separator();

            ImGui.Text($"Total Time: {e.TotalTime}");

            ImGui.Separator();

            ImGui.Text($"Delta Time: {e.DeltaTime}");

            ImGui.End();
        }

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
