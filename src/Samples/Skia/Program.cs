using Graphics.Core;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.ImGui;
using Graphics.Vulkan.Skia;
using Graphics.Windowing;
using Graphics.Windowing.Events;
using Hexa.NET.ImGui;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Silk.NET.Maths;
using SkiaSharp;
using Tests.Core;

namespace Tests.Skia;

internal sealed unsafe class Program
{
    private static SdlWindow mainWindow = null!;
    private static Context context = null!;
    private static GraphicsDevice device = null!;
    private static Swapchain swapchain = null!;
    private static ImGuiController imGuiController = null!;
    private static CommandList commandList = null!;
    private static GRContext grContext = null!;
    private static View[] views = null!;

    private static void Main(string[] _)
    {
        mainWindow = new()
        {
            Title = "Tests.Skia",
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
        grContext = SkiaGraphics.CreateContext(device);

        PlotModel model = new()
        {
            Title = "Simple Plot",
            Subtitle = "This is a simple plot"
        };

        Demo(model);

        views =
        [
            new AnimationView(Path.Combine("Assets", "LottieFiles", "demo1.json"), device, imGuiController, grContext),
            new AnimationView(Path.Combine("Assets", "LottieFiles", "demo2.json"), device, imGuiController, grContext),
            new PlotView("OxyPlot View", device, imGuiController, grContext){ ActualModel = model }
        ];
    }

    private static void Unloaded(object? sender, EventArgs e)
    {
        foreach (View view in views)
        {
            view.Dispose();
        }

        grContext.Dispose();
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

        ImGui.Begin("Tests.Skia");
        {
            ImGui.Text($"FPS: {1.0f / e.DeltaTime}");

            ImGui.Separator();

            ImGui.Text($"Total Time: {e.TotalTime}");

            ImGui.Separator();

            ImGui.Text($"Delta Time: {e.DeltaTime}");

            ImGui.End();
        }

        grContext.Flush(true);
        grContext.PurgeUnusedResources(2000);

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

    private static void Demo(PlotModel model)
    {
        const string xKey = "X";
        const string yKey = "Y";
        const string cKey = "Color";
        const string legendKey = "Legend";

        Random random = new(10);

        Axis x = new LinearAxis()
        {
            Key = xKey,
            Position = AxisPosition.Bottom,
            Title = "X Axis",
            Minimum = 0,
            Maximum = 100,
            AxislineStyle = LineStyle.Solid,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Dot
        };

        model.Axes.Add(x);

        Axis y = new LinearAxis()
        {
            Key = yKey,
            Position = AxisPosition.Left,
            Title = "Y Axis",
            Minimum = 0,
            Maximum = 100,
            AxislineStyle = LineStyle.Solid,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Dot
        };

        model.Axes.Add(y);

        Axis c = new LinearColorAxis()
        {
            Key = cKey,
            Position = AxisPosition.Right,
            Title = "Color Axis",
            Minimum = 0,
            Maximum = 100,
            Palette = OxyPalettes.Cool(32),
            LowColor = OxyColors.Undefined,
            HighColor = OxyColors.Undefined
        };

        model.Axes.Add(c);

        Legend legend = new()
        {
            Key = legendKey,
            LegendPosition = LegendPosition.RightTop,
            LegendOrientation = LegendOrientation.Vertical,
            LegendBorder = OxyColors.Black,
            LegendBorderThickness = 1
        };

        model.Legends.Add(legend);

        ScatterSeries scatterSeries = new()
        {
            Title = "ScatterSeries",
            XAxisKey = xKey,
            YAxisKey = yKey,
            ColorAxisKey = cKey,
            LegendKey = legendKey,
            MarkerType = MarkerType.Circle,
            MarkerSize = 4
        };

        for (int i = 0; i < 2500; i++)
        {
            ScatterPoint point = new(((random.NextDouble() * 2.2) - 1) * 200, (random.NextDouble() * 400) - 200)
            {
                Value = (random.NextDouble() * 200) - 100
            };

            scatterSeries.Points.Add(point);
        }

        model.Series.Add(scatterSeries);

        LineSeries lineSeries = new()
        {
            Title = "LineSeries",
            XAxisKey = xKey,
            YAxisKey = yKey,
            LegendKey = legendKey,
            Color = OxyColors.SkyBlue,
            StrokeThickness = 3,
            LineStyle = LineStyle.Dash,
            MarkerType = MarkerType.Circle,
            MarkerSize = 5,
            MarkerStroke = OxyColors.White,
            MarkerFill = OxyColors.SkyBlue,
            MarkerStrokeThickness = 1.5
        };

        int pointY = random.Next(10, 90);

        for (int pointX = 0; pointX < 100; pointX += 10)
        {
            lineSeries.Points.Add(new DataPoint(pointX, pointY));

            pointY += random.Next(-20, 20);
        }

        model.Series.Add(lineSeries);

        model.InvalidatePlot(true);
    }
}
