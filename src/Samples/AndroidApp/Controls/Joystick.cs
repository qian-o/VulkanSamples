using System.Numerics;
using CommunityToolkit.Maui.Behaviors;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Tests.AndroidApp.Controls;

public class Joystick : SKCanvasView
{
    public Joystick()
    {
        IgnorePixelScaling = true;
        EnableTouchEvents = true;

        PaintSurface += Joystick_PaintSurface;

        Touch += (a, b) =>
        {
            float width = (float)Width;
            float height = (float)Height;

            IsMoving = b.ActionType is SKTouchAction.Pressed or SKTouchAction.Moved;

            if (IsMoving)
            {
                Vector2 center = new(width / 2, height / 2);
                Vector2 up = Vector2.Normalize(new Vector2(0, -center.Y));
                Vector2 handle = Vector2.Normalize(new Vector2(b.Location.X + center.X - width, b.Location.Y + center.Y - height));

                float dot = Vector2.Dot(up, handle);

                Radians = MathF.Acos(dot);

                if (handle.X < 0)
                {
                    Radians = (MathF.PI * 2) - Radians;
                }
            }

            InvalidateSurface();
        };

        Behaviors.Add(new TouchBehavior());
    }

    public bool IsMoving { get; private set; }

    public float Radians { get; private set; }

    private void Joystick_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        SKImageInfo info = e.Info;
        SKCanvas canvas = e.Surface.Canvas;

        float width = info.Width;
        float height = info.Height;

        canvas.Clear();

        if (IsMoving)
        {
            SKMatrix matrix = SKMatrix.Concat(canvas.TotalMatrix, SKMatrix.CreateRotation(Radians, width / 2, height / 2));

            canvas.SetMatrix(matrix);
        }

        DrawOutline(canvas, width, height, 2, SKColors.White);

        DrawInnerCircle(canvas, width, height, 12, new SKColor(102, 204, 255), SKColors.White);

        DrawHandle(canvas, width, height, 24, SKColors.White, IsMoving);
    }

    private static void DrawOutline(SKCanvas canvas,
                                    float width,
                                    float height,
                                    float strokeWidth,
                                    SKColor color,
                                    float alpha = 0.5f,
                                    float interval = 4)
    {
        float centerX = width / 2;
        float centerY = height / 2;

        canvas.DrawArc(new SKRect(0, 0, width, height), 0, 360, false, new SKPaint
        {
            IsAntialias = true,
            IsDither = true,
            Color = ColorWithAlpha(color, alpha),
            Style = SKPaintStyle.Fill
        });

        canvas.DrawArc(new SKRect(strokeWidth, strokeWidth, width - strokeWidth, height - strokeWidth), 0, 360, false, new SKPaint
        {
            IsAntialias = true,
            IsDither = true,
            Color = SKColors.White,
            Style = SKPaintStyle.Fill,
            BlendMode = SKBlendMode.DstOut
        });

        canvas.DrawRect(new SKRect(centerX - interval, 0, centerX + interval, height), new SKPaint
        {
            IsAntialias = true,
            IsDither = true,
            Color = SKColors.Red,
            Style = SKPaintStyle.Fill,
            BlendMode = SKBlendMode.DstOut
        });

        canvas.DrawRect(new SKRect(0, centerY - interval, width, centerY + interval), new SKPaint
        {
            IsAntialias = true,
            IsDither = true,
            Color = SKColors.Red,
            Style = SKPaintStyle.Fill,
            BlendMode = SKBlendMode.DstOut
        });
    }

    private static void DrawInnerCircle(SKCanvas canvas,
                                        float width,
                                        float height,
                                        float margin,
                                        SKColor inColor,
                                        SKColor outColor,
                                        float alpha = 0.5f)
    {
        float centerX = width / 2;
        float centerY = height / 2;

        canvas.DrawArc(new SKRect(margin, margin, width - margin, height - margin), 0, 360, false, new SKPaint
        {
            IsAntialias = true,
            IsDither = true,
            Style = SKPaintStyle.Fill,
            Shader = SKShader.CreateRadialGradient(new SKPoint(centerX, centerY),
                                                   centerX,
                                                   [ColorWithAlpha(inColor, alpha), ColorWithAlpha(outColor, 0)],
                                                   [0.5f, 1],
                                                   SKShaderTileMode.Clamp)
        });
    }

    private static void DrawHandle(SKCanvas canvas,
                                   float width,
                                   float height,
                                   float radius,
                                   SKColor color,
                                   bool isMoving,
                                   float alpha = 0.8f)
    {
        float centerX = width / 2;
        float centerY = height / 2;

        if (isMoving)
        {
            canvas.DrawCircle(centerX, radius + 6, radius, new SKPaint
            {
                IsAntialias = true,
                IsDither = true,
                Color = ColorWithAlpha(color, alpha),
                Style = SKPaintStyle.Fill
            });
        }
        else
        {
            canvas.DrawCircle(centerX, centerY, radius, new SKPaint
            {
                IsAntialias = true,
                IsDither = true,
                Color = ColorWithAlpha(color, alpha),
                Style = SKPaintStyle.Fill
            });
        }
    }

    private static SKColor ColorWithAlpha(SKColor color, float alpha)
    {
        return new SKColor(color.Red, color.Green, color.Blue, (byte)(color.Alpha * alpha));
    }
}
