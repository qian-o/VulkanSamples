namespace Graphics.Windowing.Enums;

public enum WindowBorder
{
    /// <summary>
    /// The window can be resized by clicking and dragging its border.
    /// </summary>
    Resizable,

    /// <summary>
    /// The window border is visible, but cannot be resized. All window-resizings must happen solely in the code.
    /// </summary>
    Fixed,

    /// <summary>
    /// The window border is hidden.
    /// </summary>
    Hidden
}
