using System.Numerics;
using Hexa.NET.ImGui;

namespace Tests.Core;

public class ImGuiMouseButtonEventArgs(ImGuiMouseButton button, Vector2 position) : EventArgs
{
    public ImGuiMouseButton Button { get; } = button;

    public Vector2 Position { get; } = position;
}

public class ImGuiMouseMoveEventArgs(Vector2 position) : EventArgs
{
    public Vector2 Position { get; } = position;
}

public class ImGuiMouseWheelEventArgs(Vector2 position, float wheel) : EventArgs
{
    public Vector2 Position { get; } = position;

    public float Wheel { get; } = wheel;
}

public class ViewController(View view)
{
    public event EventHandler<ImGuiMouseButtonEventArgs>? MouseDown;
    public event EventHandler<ImGuiMouseButtonEventArgs>? MouseUp;
    public event EventHandler<ImGuiMouseMoveEventArgs>? MouseMove;
    public event EventHandler<ImGuiMouseWheelEventArgs>? MouseWheel;

    private bool isLeftMouseDown;
    private bool isRightMouseDown;
    private bool isMiddleMouseDown;
    private Vector2 lastMousePosition;

    public bool UseDpiScale { get; set; } = true;

    public void Update()
    {
        Vector2 mousePosition = ImGui.GetMousePos() - view.Position;
        mousePosition /= view.DpiScale;

        if (ImGuiP.IsMouseDown(ImGuiMouseButton.Left) && !isLeftMouseDown)
        {
            isLeftMouseDown = true;

            MouseDown?.Invoke(view, new ImGuiMouseButtonEventArgs(ImGuiMouseButton.Left, mousePosition));
        }
        else if (!ImGuiP.IsMouseDown(ImGuiMouseButton.Left) && isLeftMouseDown)
        {
            isLeftMouseDown = false;

            MouseUp?.Invoke(view, new ImGuiMouseButtonEventArgs(ImGuiMouseButton.Left, mousePosition));
        }

        if (ImGuiP.IsMouseDown(ImGuiMouseButton.Right) && !isRightMouseDown)
        {
            isRightMouseDown = true;

            MouseDown?.Invoke(view, new ImGuiMouseButtonEventArgs(ImGuiMouseButton.Right, mousePosition));
        }
        else if (!ImGuiP.IsMouseDown(ImGuiMouseButton.Right) && isRightMouseDown)
        {
            isRightMouseDown = false;

            MouseUp?.Invoke(view, new ImGuiMouseButtonEventArgs(ImGuiMouseButton.Right, mousePosition));
        }

        if (ImGuiP.IsMouseDown(ImGuiMouseButton.Middle) && !isMiddleMouseDown)
        {
            isMiddleMouseDown = true;

            MouseDown?.Invoke(view, new ImGuiMouseButtonEventArgs(ImGuiMouseButton.Middle, mousePosition));
        }
        else if (!ImGuiP.IsMouseDown(ImGuiMouseButton.Middle) && isMiddleMouseDown)
        {
            isMiddleMouseDown = false;

            MouseUp?.Invoke(view, new ImGuiMouseButtonEventArgs(ImGuiMouseButton.Middle, mousePosition));
        }

        if (lastMousePosition != mousePosition)
        {
            MouseMove?.Invoke(view, new ImGuiMouseMoveEventArgs(mousePosition));

            lastMousePosition = mousePosition;
        }

        float wheel = ImGui.GetIO().MouseWheel;
        if (wheel != 0)
        {
            MouseWheel?.Invoke(view, new ImGuiMouseWheelEventArgs(mousePosition, wheel));
        }
    }
}
