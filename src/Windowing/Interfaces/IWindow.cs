using Graphics.Windowing.Enums;
using Graphics.Windowing.Events;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;

namespace Graphics.Windowing.Interfaces;

public interface IWindow
{
    string Title { get; set; }

    WindowState State { get; set; }

    WindowBorder Border { get; set; }

    Vector2D<int> MinimumSize { get; set; }

    Vector2D<int> MaximumSize { get; set; }

    Vector2D<int> Position { get; set; }

    Vector2D<int> Size { get; set; }

    bool IsVisible { get; set; }

    bool TopMost { get; set; }

    bool ShowInTaskbar { get; set; }

    float Opacity { get; set; }

    double UpdatePerSecond { get; set; }

    double RenderPerSecond { get; set; }

    bool IsCreated { get; }

    nint Handle { get; }

    float DpiScale { get; }

    bool IsFocused { get; }

    IVkSurface Surface { get; }

    double Time { get; }

    event EventHandler<EventArgs>? Loaded;

    event EventHandler<EventArgs>? Unloaded;

    event EventHandler<ValueEventArgs<WindowState>>? StateChanged;

    event EventHandler<ValueEventArgs<Vector2D<int>>>? PositionChanged;

    event EventHandler<ValueEventArgs<Vector2D<int>>>? SizeChanged;

    event EventHandler<KeyEventArgs>? KeyDown;

    event EventHandler<KeyEventArgs>? KeyUp;

    event EventHandler<ValueEventArgs<char>>? KeyChar;

    event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseMove;

    event EventHandler<MouseButtonEventArgs>? MouseDown;

    event EventHandler<MouseButtonEventArgs>? MouseUp;

    event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseWheel;

    event EventHandler<MouseButtonEventArgs>? Click;

    event EventHandler<MouseButtonEventArgs>? DoubleClick;

    event EventHandler<TimeEventArgs>? Update;

    event EventHandler<TimeEventArgs>? Render;

    void Show();

    void Close();

    void Focus();

    void DoEvents();

    void DoUpdate();

    void DoRender();
}
