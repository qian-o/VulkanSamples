using Silk.NET.Maths;
using Windowing.Enums;

namespace Windowing.Events;

public class MouseButtonEventArgs(MouseButton button, Vector2D<int> position) : EventArgs
{
    public MouseButton Button { get; } = button;

    public Vector2D<int> Position { get; } = position;
}
