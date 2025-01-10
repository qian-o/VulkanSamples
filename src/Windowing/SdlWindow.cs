using System.Text;
using Graphics.Core.Helpers;
using Graphics.Windowing.Enums;
using Graphics.Windowing.Events;
using Graphics.Windowing.Structs;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;
using Silk.NET.SDL;

namespace Graphics.Windowing;

public unsafe class SdlWindow : WindowImplementationBase
{
    private Window* window;
    private SdlVkSurface? surface;

    private string title = "SdlWindow";
    private WindowState state = WindowState.Normal;
    private WindowBorder border = WindowBorder.Resizable;
    private Vector2D<int> minimumSize;
    private Vector2D<int> maximumSize;
    private Vector2D<int> position = new(50, 50);
    private Vector2D<int> size = new(800, 600);
    private bool isVisible = true;
    private bool topMost;
    private bool showInTaskbar = true;
    private float opacity = 1.0f;

    public override string Title
    {
        get
        {
            return title;
        }
        set
        {
            title = value;

            if (IsCreated)
            {
                SdlManager.Sdl.SetWindowTitle(window, Encoding.UTF8.GetBytes(value).AsPointer());
            }
        }
    }

    public override WindowState State
    {
        get
        {
            if (IsCreated)
            {
                WindowFlags flags = (WindowFlags)SdlManager.Sdl.GetWindowFlags(window);

                if (flags.HasFlag(WindowFlags.Minimized))
                {
                    return state = WindowState.Minimized;
                }
                else if (flags.HasFlag(WindowFlags.Maximized))
                {
                    return state = WindowState.Maximized;
                }
                else if (flags.HasFlag(WindowFlags.Fullscreen))
                {
                    return state = WindowState.Fullscreen;
                }
                else
                {
                    return state = WindowState.Normal;
                }
            }

            return state;
        }
        set
        {
            state = value;

            if (IsCreated)
            {
                switch (value)
                {
                    case WindowState.Normal:
                        SdlManager.Sdl.RestoreWindow(window);
                        break;
                    case WindowState.Minimized:
                        SdlManager.Sdl.MinimizeWindow(window);
                        break;
                    case WindowState.Maximized:
                        SdlManager.Sdl.MaximizeWindow(window);
                        break;
                    case WindowState.Fullscreen:
                        SdlManager.Sdl.SetWindowFullscreen(window, (uint)WindowFlags.Fullscreen);
                        break;
                }
            }
        }
    }

    public override WindowBorder Border
    {
        get
        {
            return border;
        }
        set
        {
            border = value;

            if (IsCreated)
            {
                switch (value)
                {
                    case WindowBorder.Resizable:
                        SdlManager.Sdl.SetWindowBordered(window, SdlBool.True);
                        SdlManager.Sdl.SetWindowResizable(window, SdlBool.True);
                        break;
                    case WindowBorder.Fixed:
                        SdlManager.Sdl.SetWindowBordered(window, SdlBool.True);
                        SdlManager.Sdl.SetWindowResizable(window, SdlBool.False);
                        break;
                    case WindowBorder.Hidden:
                        SdlManager.Sdl.SetWindowBordered(window, SdlBool.False);
                        SdlManager.Sdl.SetWindowBordered(window, SdlBool.False);
                        break;
                }
            }
        }
    }

    public override Vector2D<int> MinimumSize
    {
        get
        {
            return minimumSize;
        }
        set
        {
            minimumSize = value;

            if (IsCreated)
            {
                SdlManager.Sdl.SetWindowMinimumSize(window, value.X, value.Y);
            }
        }
    }

    public override Vector2D<int> MaximumSize
    {
        get
        {
            return maximumSize;
        }
        set
        {
            maximumSize = value;

            if (IsCreated)
            {
                SdlManager.Sdl.SetWindowMaximumSize(window, value.X, value.Y);
            }
        }
    }

    public override Vector2D<int> Position
    {
        get
        {
            if (IsCreated)
            {
                int x, y;
                SdlManager.Sdl.GetWindowPosition(window, &x, &y);

                return position = new Vector2D<int>(x, y);
            }

            return position;
        }
        set
        {
            position = value;

            if (IsCreated)
            {
                SdlManager.Sdl.SetWindowPosition(window, value.X, value.Y);
            }
        }
    }

    public override Vector2D<int> Size
    {
        get
        {
            if (IsCreated)
            {
                int width, height;
                SdlManager.Sdl.GetWindowSize(window, &width, &height);

                return size = new Vector2D<int>(width, height);
            }

            return size;
        }
        set
        {
            size = value;

            if (IsCreated)
            {
                SdlManager.Sdl.SetWindowSize(window, value.X, value.Y);
            }
        }
    }

    public override bool IsVisible
    {
        get
        {
            return isVisible;
        }
        set
        {
            isVisible = value;

            if (IsCreated)
            {
                if (value)
                {
                    SdlManager.Sdl.ShowWindow(window);
                }
                else
                {
                    SdlManager.Sdl.HideWindow(window);
                }
            }
        }
    }

    public override bool TopMost
    {
        get
        {
            return topMost;
        }
        set
        {
            topMost = value;

            if (IsCreated)
            {
                if (value)
                {
                    SdlManager.Sdl.SetWindowAlwaysOnTop(window, SdlBool.True);
                }
                else
                {
                    SdlManager.Sdl.SetWindowAlwaysOnTop(window, SdlBool.False);
                }
            }
        }
    }

    public override bool ShowInTaskbar
    {
        get
        {
            return showInTaskbar;
        }
        set
        {
            showInTaskbar = value;

            if (IsCreated)
            {
                // SDL does not support modifying this property after the window is created.
            }
        }
    }

    public override float Opacity
    {
        get
        {
            return opacity;
        }
        set
        {
            opacity = value;

            if (IsCreated)
            {
                SdlManager.Sdl.SetWindowOpacity(window, value);
            }
        }
    }

    public override bool IsCreated
    {
        get
        {
            return window != null;
        }
    }

    public override nint Handle
    {
        get
        {
            return (nint)window;
        }
    }

    public override float DpiScale
    {
        get
        {
            int displayIndex = SdlManager.Sdl.GetWindowDisplayIndex(window);

            float ddpi;
            SdlManager.Sdl.GetDisplayDPI(displayIndex, &ddpi, null, null);

            return ddpi == 0 ? 1.0f : ddpi / 96.0f;
        }
    }

    public override bool IsFocused
    {
        get
        {
            return ((WindowFlags)SdlManager.Sdl.GetWindowFlags(window)).HasFlag(WindowFlags.InputFocus);
        }
    }

    public override IVkSurface Surface
    {
        get
        {
            return surface ??= new SdlVkSurface(window);
        }
    }

    public override event EventHandler<ValueEventArgs<WindowState>>? StateChanged;

    public override event EventHandler<ValueEventArgs<Vector2D<int>>>? PositionChanged;

    public override event EventHandler<ValueEventArgs<Vector2D<int>>>? SizeChanged;

    public override event EventHandler<KeyEventArgs>? KeyDown;

    public override event EventHandler<KeyEventArgs>? KeyUp;

    public override event EventHandler<ValueEventArgs<char>>? KeyChar;

    public override event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseMove;

    public override event EventHandler<MouseButtonEventArgs>? MouseDown;

    public override event EventHandler<MouseButtonEventArgs>? MouseUp;

    public override event EventHandler<ValueEventArgs<Vector2D<int>>>? MouseWheel;

    public override event EventHandler<MouseButtonEventArgs>? Click;

    public override event EventHandler<MouseButtonEventArgs>? DoubleClick;

    public override void Show()
    {
        if (Initialize())
        {
            IsVisible = true;

            base.Show();
        }
    }

    public override void Close()
    {
        if (Uninitialize())
        {
            surface?.Dispose();

            base.Close();
        }
    }

    public override void Focus()
    {
        SdlManager.Sdl.RaiseWindow(window);
    }

    public override void DoEvents()
    {
        uint id = SdlManager.Sdl.GetWindowID(window);

        foreach (Event @event in SdlManager.Events)
        {
            if (@event.Window.WindowID != id)
            {
                continue;
            }

            ProcessEvent(@event);
        }
    }

    private bool Initialize()
    {
        if (IsCreated)
        {
            return false;
        }

        WindowFlags flags = IsVisible ? WindowFlags.Shown : WindowFlags.Hidden;

        switch (State)
        {
            case WindowState.Normal:
                flags |= WindowFlags.Resizable;
                break;
            case WindowState.Minimized:
                flags |= WindowFlags.Minimized;
                break;
            case WindowState.Maximized:
                flags |= WindowFlags.Maximized;
                break;
            case WindowState.Fullscreen:
                flags |= WindowFlags.Fullscreen;
                break;
        }

        switch (Border)
        {
            case WindowBorder.Resizable:
                flags |= WindowFlags.Resizable;
                break;
            case WindowBorder.Fixed:
                flags |= WindowFlags.Borderless;
                break;
            case WindowBorder.Hidden:
                flags |= WindowFlags.Borderless;
                break;
        }

        if (TopMost)
        {
            flags |= WindowFlags.AlwaysOnTop;
        }

        if (!ShowInTaskbar)
        {
            flags |= WindowFlags.SkipTaskbar;
        }

        flags |= WindowFlags.Vulkan;

        window = SdlManager.Sdl.CreateWindow(Encoding.UTF8.GetBytes(Title).AsPointer(),
                                             Position.X,
                                             Position.Y,
                                             Size.X,
                                             Size.Y,
                                             (uint)flags);

        WindowManager.AddWindow(this);

        return true;
    }

    private bool Uninitialize()
    {
        if (!IsCreated)
        {
            return false;
        }

        SdlManager.Sdl.DestroyWindow(window);

        window = null;

        WindowManager.RemoveWindow(this);

        return true;
    }

    private void ProcessEvent(Event @event)
    {
        EventType type = (EventType)@event.Type;

        switch (type)
        {
            case EventType.Windowevent:
                ProcessWindowEvent(@event.Window);
                break;
            case EventType.Keydown:
                ProcessKeyboardEvent(@event.Key, true);
                break;
            case EventType.Keyup:
                ProcessKeyboardEvent(@event.Key, false);
                break;
            case EventType.Textinput:
                ProcessTextInputEvent(@event.Text);
                break;
            case EventType.Mousemotion:
                MouseMove?.Invoke(this, new ValueEventArgs<Vector2D<int>>(new Vector2D<int>(@event.Motion.X, @event.Motion.Y)));
                break;
            case EventType.Mousebuttondown:
                ProcessMouseButtonEvent(@event.Button, true);
                break;
            case EventType.Mousebuttonup:
                ProcessMouseButtonEvent(@event.Button, false);
                break;
            case EventType.Mousewheel:
                MouseWheel?.Invoke(this, new ValueEventArgs<Vector2D<int>>(new Vector2D<int>(@event.Wheel.X, @event.Wheel.Y)));
                break;
        }
    }

    private void ProcessWindowEvent(WindowEvent windowEvent)
    {
        WindowEventID windowEventID = (WindowEventID)windowEvent.Event;

        switch (windowEventID)
        {
            case WindowEventID.Moved:
                PositionChanged?.Invoke(this, new ValueEventArgs<Vector2D<int>>(Position));
                break;
            case WindowEventID.Resized:
                SizeChanged?.Invoke(this, new ValueEventArgs<Vector2D<int>>(Size));
                break;
            case WindowEventID.Minimized:
            case WindowEventID.Maximized:
                StateChanged?.Invoke(this, new ValueEventArgs<WindowState>(State));
                break;
            case WindowEventID.Restored:
                SizeChanged?.Invoke(this, new ValueEventArgs<Vector2D<int>>(Size));
                StateChanged?.Invoke(this, new ValueEventArgs<WindowState>(State));
                break;
            case WindowEventID.Close:
                Close();
                break;
        }
    }

    private void ProcessKeyboardEvent(KeyboardEvent keyboardEvent, bool isKeyDown)
    {
        Key key = SdlManager.GetKey(keyboardEvent.Keysym.Scancode);
        KeyModifiers modifiers = SdlManager.GetKeyModifiers((Keymod)keyboardEvent.Keysym.Mod);

        if (isKeyDown)
        {
            KeyDown?.Invoke(this, new KeyEventArgs(key, modifiers));
        }
        else
        {
            KeyUp?.Invoke(this, new KeyEventArgs(key, modifiers));
        }
    }

    private void ProcessTextInputEvent(TextInputEvent textInputEvent)
    {
        const int charSize = 32;

        char* chars = stackalloc char[charSize];
        Encoding.UTF8.GetChars(&textInputEvent.Text[0], charSize, chars, charSize);

        for (int i = 0; i < charSize; i++)
        {
            if (chars[i] == '\0')
            {
                break;
            }

            KeyChar?.Invoke(this, new ValueEventArgs<char>(chars[i]));
        }
    }

    private void ProcessMouseButtonEvent(MouseButtonEvent mouseButtonEvent, bool isMouseDown)
    {
        MouseButton button = SdlManager.GetMouseButton(mouseButtonEvent.Button);

        if (isMouseDown)
        {
            MouseDown?.Invoke(this, new MouseButtonEventArgs(button, new Vector2D<int>(mouseButtonEvent.X, mouseButtonEvent.Y)));
        }
        else
        {
            MouseUp?.Invoke(this, new MouseButtonEventArgs(button, new Vector2D<int>(mouseButtonEvent.X, mouseButtonEvent.Y)));

            if (mouseButtonEvent.Clicks == 1)
            {
                Click?.Invoke(this, new MouseButtonEventArgs(button, new Vector2D<int>(mouseButtonEvent.X, mouseButtonEvent.Y)));
            }
            else if (mouseButtonEvent.Clicks == 2)
            {
                DoubleClick?.Invoke(this, new MouseButtonEventArgs(button, new Vector2D<int>(mouseButtonEvent.X, mouseButtonEvent.Y)));
            }
        }
    }
}
