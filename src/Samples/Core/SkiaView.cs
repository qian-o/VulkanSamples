using System.Numerics;
using Graphics.Core;
using Graphics.Vulkan;
using Graphics.Vulkan.ImGui;
using Graphics.Vulkan.Skia;
using Graphics.Windowing.Events;
using Hexa.NET.ImGui;
using Silk.NET.Maths;
using SkiaSharp;

namespace Tests.Core;

public abstract class SkiaView(string title,
                               GraphicsDevice device,
                               ImGuiController imGuiController,
                               GRContext grContext) : View(title)
{
    private readonly CommandList _commandList = device.Factory.CreateGraphicsCommandList();

    private FramebufferObject? _framebufferObject;
    private SKSurface? _surface;

    protected override void OnRender(TimeEventArgs e)
    {
        if (_surface != null)
        {
            SKCanvas canvas = _surface.Canvas;

            canvas.Clear(SKColors.White);

            if (UseDpiScale)
            {
                canvas.SetMatrix(SKMatrix.CreateScale(DpiScale, DpiScale));
            }

            OnRenderSurface(canvas, e);

            canvas.ResetMatrix();
        }

        if (_framebufferObject != null)
        {
            _commandList.Begin();

            _framebufferObject.Present(_commandList);

            _commandList.End();

            device.SubmitCommands(_commandList);

            ImGui.Image(imGuiController.GetBinding(device.Factory, _framebufferObject.PresentTexture), new Vector2(_framebufferObject.Width, _framebufferObject.Height));
        }
    }

    protected override void OnResize(ValueEventArgs<Vector2D<int>> e)
    {
        _surface?.Dispose();

        if (_framebufferObject != null)
        {
            imGuiController.RemoveBinding(imGuiController.GetBinding(device.Factory, _framebufferObject.PresentTexture));

            _framebufferObject.Dispose();
        }

        _framebufferObject = new FramebufferObject(device, e.Value.X, e.Value.Y, TextureSampleCount.Count1);

        _surface = SkiaGraphics.CreateSurface(grContext, _framebufferObject.ColorTexture);
    }

    protected abstract void OnRenderSurface(SKCanvas canvas, TimeEventArgs e);

    protected override void Destroy()
    {
        _surface?.Dispose();
        _framebufferObject?.Dispose();

        _commandList.Dispose();
    }
}
