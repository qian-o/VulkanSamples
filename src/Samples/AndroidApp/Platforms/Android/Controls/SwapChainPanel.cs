using System.Diagnostics;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Graphics.Core.Helpers;
using Java.Interop;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Tests.AndroidApp.Controls;
using Tests.AndroidApp.Platforms.Android.Helpers;
using Format = Android.Graphics.Format;

namespace Tests.AndroidApp.Platforms.Android.Controls;

internal sealed unsafe class VkSurface(ANativeWindow* window) : IVkSurface, IDisposable
{
    private readonly Vk _vk = Vk.GetApi();
    private readonly Allocator _alloter = new();

    ~VkSurface()
    {
        Dispose();
    }

    public VkNonDispatchableHandle Create<T>(VkHandle instance, T* allocator) where T : unmanaged
    {
        if (!_vk.TryGetInstanceExtension(new Instance(instance.Handle), out KhrAndroidSurface androidSurface))
        {
            throw new InvalidOperationException("KHR Android Surface extension is not supported.");
        }

        AndroidSurfaceCreateInfoKHR createInfo = new()
        {
            SType = StructureType.AndroidSurfaceCreateInfoKhr,
            Window = (nint*)window
        };

        SurfaceKHR surface;
        if (androidSurface.CreateAndroidSurface(new Instance(instance.Handle), &createInfo, (AllocationCallbacks*)allocator, &surface) != Result.Success)
        {
            throw new InvalidOperationException("Failed to create Android surface.");
        }

        return new VkNonDispatchableHandle(surface.Handle);
    }

    public byte** GetRequiredExtensions(out uint count)
    {
        count = 2;

        return _alloter.Alloc([KhrSurface.ExtensionName, KhrAndroidSurface.ExtensionName]);
    }

    public void Dispose()
    {
        _alloter.Dispose();
        _vk.Dispose();

        GC.SuppressFinalize(this);
    }
}

internal sealed class Timer : IDisposable
{
    private readonly Stopwatch _stopwatch = new();
    private readonly float _frequency = 1.0f / Stopwatch.Frequency;

    public float DeltaTime { get; private set; }

    public float TotalTime { get; private set; }

    public void Start()
    {
        _stopwatch.Start();
    }

    public void Stop()
    {
        _stopwatch.Stop();
    }

    public void Update()
    {
        long elapsedTicks = _stopwatch.ElapsedTicks;

        DeltaTime = elapsedTicks * _frequency;
        TotalTime += DeltaTime;

        _stopwatch.Restart();
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        _stopwatch.Reset();
    }
}

internal sealed class FrameCallback(Choreographer choreographer, ISwapChainPanel swapChainPanel) : Java.Lang.Object, Choreographer.IFrameCallback
{
    private readonly Timer timer = new();

    public void DoFrame(long frameTimeNanos)
    {
        timer.Update();

        swapChainPanel.Update(timer.DeltaTime, timer.TotalTime);

        timer.Update();

        swapChainPanel.Render(timer.DeltaTime, timer.TotalTime);

        choreographer.PostFrameCallback(this);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        timer.Dispose();
    }
}

internal sealed unsafe class SwapChainPanel : SurfaceView, ISurfaceHolderCallback
{
    private readonly ISwapChainPanel _swapChainPanel;

    private ANativeWindow* _window;
    private FrameCallback? _frameCallback;

    public SwapChainPanel(Context context, ISwapChainPanel swapChainPanel) : base(context)
    {
        _swapChainPanel = swapChainPanel;

        SetWillNotDraw(false);

        Holder?.AddCallback(this);
    }

    public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
    {
        if (_window == null)
        {
            CreateSurface();
        }
        else
        {
            _swapChainPanel.Resize((uint)width, (uint)height);
        }
    }

    public void SurfaceCreated(ISurfaceHolder holder)
    {
        CreateSurface();
    }

    public void SurfaceDestroyed(ISurfaceHolder holder)
    {
        DestroySurface();
    }

    private void CreateSurface()
    {
        DestroySurface();

        _window = NativeActivity.ANativeWindowFromSurface(JniEnvironment.EnvironmentPointer, Holder!.Surface!.Handle);

        _swapChainPanel.CreateSwapChainPanel(new VkSurface(_window));

        Choreographer.Instance?.PostFrameCallback(_frameCallback = new FrameCallback(Choreographer.Instance, _swapChainPanel));
    }

    private void DestroySurface()
    {
        if (_window != null)
        {
            _swapChainPanel.DestroySwapChainPanel();

            NativeActivity.ANativeWindowRelease(_window);
            _window = null;
        }

        if (_frameCallback != null)
        {
            Choreographer.Instance?.RemoveFrameCallback(_frameCallback);

            _frameCallback?.Dispose();
            _frameCallback = null;
        }
    }
}
