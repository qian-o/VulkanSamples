using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Graphics.Vulkan;
using Graphics.Windowing.Events;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Direct3D9;
using Silk.NET.DXGI;
using Format = Silk.NET.Direct3D9.Format;
using PixelFormat = Graphics.Core.PixelFormat;
using PresentParameters = Silk.NET.Direct3D9.PresentParameters;
using Swapeffect = Silk.NET.Direct3D9.Swapeffect;
using WRect = System.Windows.Rect;

namespace Tests.WPF;

public unsafe class Surface : Control
{
    private readonly Stopwatch stopwatch = Stopwatch.StartNew();

    private TimeSpan lastRenderTime = TimeSpan.FromSeconds(-1);

    private readonly D3DImage image = new();

    private readonly CommandList commandList = App.Factory.CreateGraphicsCommandList();

    private readonly D3D11 d3d11 = D3D11.GetApi(null);
    private readonly D3D9 d3d9 = D3D9.GetApi(null);

    private ComPtr<ID3D11Device> d3d11Device;
    private ComPtr<ID3D11DeviceContext> d3d11DeviceContext;
    private ComPtr<ID3D11Texture2D> d3d11Texture;

    private ComPtr<IDirect3D9Ex> d3d9Ex;
    private ComPtr<IDirect3DDevice9Ex> d3d9DeviceEx;
    private ComPtr<IDirect3DSurface9> d3d9Surface;
    private ComPtr<IDirect3DTexture9> d3d9Texture;

    private nint sharedHandle;

    private Texture? texture;
    private TextureView? textureView;

    public event EventHandler? Resized;
    public event Action<CommandList, TimeEventArgs>? Rendering;

    public Surface()
    {
        Focusable = true;
        Effect = new GammaCorrectionEffect();

        d3d11.CreateDevice(default(ComPtr<IDXGIAdapter>),
                           D3DDriverType.Hardware,
                           0,
                           (uint)CreateDeviceFlag.BgraSupport,
                           null,
                           0,
                           D3D11.SdkVersion,
                           ref d3d11Device,
                           null,
                           ref d3d11DeviceContext);

        d3d9.Direct3DCreate9Ex(D3D9.SdkVersion, ref d3d9Ex);

        PresentParameters parameters = new()
        {
            Windowed = true,
            SwapEffect = Swapeffect.Discard,
            PresentationInterval = D3D9.PresentIntervalImmediate
        };

        d3d9Ex.CreateDeviceEx(0,
                              Devtype.Hal,
                              new WindowInteropHelper(Application.Current.MainWindow).Handle,
                              D3D9.CreateHardwareVertexprocessing,
                              ref parameters,
                              null,
                              ref d3d9DeviceEx);

        CompositionTarget.Rendering += CompositionTarget_Rendering;
    }

    public Texture Texture => texture!;

    public TextureView TextureView => textureView!;

    private void CompositionTarget_Rendering(object? sender, EventArgs e)
    {
        RenderingEventArgs args = (RenderingEventArgs)e;

        if (lastRenderTime != args.RenderingTime)
        {
            if (texture != null)
            {
                commandList.Begin();

                Rendering?.Invoke(commandList, new TimeEventArgs(lastRenderTime.TotalSeconds - stopwatch.Elapsed.TotalSeconds, stopwatch.Elapsed.TotalSeconds));

                commandList.End();

                App.Device.SubmitCommands(commandList);
            }

            lastRenderTime = args.RenderingTime;

            InvalidateVisual();
        }
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        CreateResources();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (d3d9Surface.Handle == null)
        {
            return;
        }

        if (!image.IsFrontBufferAvailable)
        {
            return;
        }

        image.Lock();
        image.AddDirtyRect(new Int32Rect(0, 0, image.PixelWidth, image.PixelHeight));
        image.Unlock();

        drawingContext.DrawImage(image, new WRect(0, 0, ActualWidth, ActualHeight));
    }

    protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        Focus();
    }

    private void DestroyResources()
    {
        textureView?.Dispose();
        texture?.Dispose();

        d3d11Texture.Dispose();

        d3d9Surface.Dispose();
        d3d9Texture.Dispose();

        d3d11Texture = default;
        d3d9Surface = default;
        d3d9Texture = default;
    }

    private void CreateResources()
    {
        DestroyResources();

        if (ActualWidth <= 0 || ActualHeight <= 0)
        {
            return;
        }

        uint width = (uint)ActualWidth;
        uint height = (uint)ActualHeight;

        void* d3d9shared = null;
        d3d9DeviceEx.CreateTexture(width,
                                   height,
                                   1,
                                   D3D9.UsageRendertarget,
                                   Format.X8R8G8B8,
                                   Pool.Default,
                                   ref d3d9Texture,
                                   ref d3d9shared);

        d3d9Texture.GetSurfaceLevel(0, ref d3d9Surface);

        void* d3d11shared = null;
        d3d11Texture = d3d11Device.OpenSharedResource<ID3D11Texture2D>(d3d9shared);

        using ComPtr<IDXGIResource> resource = d3d11Texture.QueryInterface<IDXGIResource>();
        resource.GetSharedHandle(ref d3d11shared);

        sharedHandle = (nint)d3d11shared;

        texture = App.Factory.CreateTexture(sharedHandle,
                                            width,
                                            height,
                                            PixelFormat.B8G8R8A8UNorm);

        textureView = App.Factory.CreateTextureView(texture);

        image.Lock();
        image.SetBackBuffer(D3DResourceType.IDirect3DSurface9, (nint)d3d9Surface.Handle);
        image.Unlock();

        Resized?.Invoke(this, EventArgs.Empty);
    }
}
