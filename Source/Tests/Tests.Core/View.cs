using System.Numerics;
using Graphics.Core;
using Graphics.Windowing.Events;
using Hexa.NET.ImGui;
using Silk.NET.Maths;

namespace Tests.Core;

public abstract class View(string title) : DisposableObject
{
    public Vector2 Position { get; private set; }

    public float DpiScale { get; private set; } = 1.0f;

    public bool UseDpiScale { get; set; } = true;

    public uint Width { get; private set; }

    public uint Height { get; private set; }

    public float ActualWidth => UseDpiScale ? Width * (1 / DpiScale) : Width;

    public float ActualHeight => UseDpiScale ? Height * (1 / DpiScale) : Height;

    public void Update(TimeEventArgs e)
    {
        OnUpdate(e);
    }

    public void Render(TimeEventArgs e)
    {
        ImGui.Begin(title);
        {
            Position = ImGui.GetCursorScreenPos();
            DpiScale = ImGui.GetWindowDpiScale();

            Vector2 size = ImGui.GetContentRegionAvail();

            uint width = Convert.ToUInt32(Math.Max(1, size.X));
            uint height = Convert.ToUInt32(Math.Max(1, size.Y));

            if (Width != width || Height != height)
            {
                Width = width;
                Height = height;

                OnResize(new ValueEventArgs<Vector2D<int>>(new Vector2D<int>((int)Width, (int)Height)));
            }

            OnRender(e);

            ImGui.End();
        }
    }

    protected abstract void OnUpdate(TimeEventArgs e);

    protected abstract void OnRender(TimeEventArgs e);

    protected abstract void OnResize(ValueEventArgs<Vector2D<int>> e);
}
