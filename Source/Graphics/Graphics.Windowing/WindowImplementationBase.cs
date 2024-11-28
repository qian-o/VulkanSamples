using System.Diagnostics;
using Graphics.Windowing.Enums;
using Graphics.Windowing.Events;
using Graphics.Windowing.Interfaces;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;

namespace Graphics.Windowing;

public abstract class WindowImplementationBase : IWindow
{
    #region Abstract Properties
    public abstract string Title { get; set; }

    public abstract WindowState State { get; set; }

    public abstract WindowBorder Border { get; set; }

    public abstract Vector2D<int> MinimumSize { get; set; }

    public abstract Vector2D<int> MaximumSize { get; set; }

    public abstract Vector2D<int> Position { get; set; }

    public abstract Vector2D<int> Size { get; set; }

    public abstract bool IsVisible { get; set; }

    public abstract bool TopMost { get; set; }

    public abstract bool ShowInTaskbar { get; set; }

    public abstract float Opacity { get; set; }

    public abstract bool IsCreated { get; }

    public abstract nint Handle { get; }

    public abstract float DpiScale { get; }

    public abstract bool IsFocused { get; }

    public abstract IVkSurface Surface { get; }
    #endregion

    #region Abstract Events
    public abstract event EventHandler<ValueEventArgs<WindowState>>? StateChanged;

    public abstract event EventHandler<ValueEventArgs<Vector2D<int>>>? PositionChanged;

    public abstract event EventHandler<ValueEventArgs<Vector2D<int>>>? SizeChanged;

    public abstract event EventHandler<KeyEventArgs>? KeyDown;

    public abstract event EventHandler<KeyEventArgs>? KeyUp;

    public abstract event EventHandler<ValueEventArgs<char>>? KeyChar;

    public abstract event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseMove;

    public abstract event EventHandler<MouseButtonEventArgs>? MouseDown;

    public abstract event EventHandler<MouseButtonEventArgs>? MouseUp;

    public abstract event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseWheel;

    public abstract event EventHandler<MouseButtonEventArgs>? Click;

    public abstract event EventHandler<MouseButtonEventArgs>? DoubleClick;
    #endregion

    #region Abstract Methods
    public abstract void Focus();

    public abstract void DoEvents();
    #endregion

    private readonly Stopwatch updateStopwatch = new();
    private readonly Stopwatch renderStopwatch = new();
    private readonly Stopwatch lifetimeStopwatch = new();

    private double updatePeriod;
    private double renderPeriod;

    public double UpdatePerSecond
    {
        get
        {
            return updatePeriod <= double.Epsilon ? 0.0 : 1.0 / updatePeriod;
        }
        set
        {
            updatePeriod = value <= double.Epsilon ? 0.0 : 1.0 / value;
        }
    }

    public double RenderPerSecond
    {
        get
        {
            return renderPeriod <= double.Epsilon ? 0.0 : 1.0 / renderPeriod;
        }
        set
        {
            renderPeriod = value <= double.Epsilon ? 0.0 : 1.0 / value;
        }
    }

    public double Time
    {
        get
        {
            return lifetimeStopwatch.Elapsed.TotalSeconds;
        }
    }

    public event EventHandler<EventArgs>? Loaded;

    public event EventHandler<EventArgs>? Unloaded;

    public event EventHandler<TimeEventArgs>? Update;

    public event EventHandler<TimeEventArgs>? Render;

    public virtual void Show()
    {
        updateStopwatch.Start();
        renderStopwatch.Start();
        lifetimeStopwatch.Start();

        Loaded?.Invoke(this, EventArgs.Empty);
    }

    public virtual void Close()
    {
        updateStopwatch.Stop();
        renderStopwatch.Stop();
        lifetimeStopwatch.Stop();

        Unloaded?.Invoke(this, EventArgs.Empty);

        updateStopwatch.Reset();
        renderStopwatch.Reset();
        lifetimeStopwatch.Reset();
    }

    public void DoUpdate()
    {
        double delta = updateStopwatch.Elapsed.TotalSeconds;

        if (delta >= updatePeriod)
        {
            updateStopwatch.Restart();

            Update?.Invoke(this, new TimeEventArgs(delta, lifetimeStopwatch.Elapsed.TotalSeconds));
        }
    }

    public void DoRender()
    {
        double delta = renderStopwatch.Elapsed.TotalSeconds;

        if (delta >= renderPeriod)
        {
            renderStopwatch.Restart();

            Render?.Invoke(this, new TimeEventArgs(delta, lifetimeStopwatch.Elapsed.TotalSeconds));
        }
    }
}
