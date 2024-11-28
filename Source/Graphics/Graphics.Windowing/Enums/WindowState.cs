namespace Graphics.Windowing.Enums;

public enum WindowState
{
    /// <summary>
    /// The window is in its regular configuration.
    /// </summary>
    Normal,

    /// <summary>
    /// The window has been minimized to the task bar.
    /// </summary>
    Minimized,

    /// <summary>
    /// The window has been maximized, covering the entire desktop, but not the taskbar.
    /// </summary>
    Maximized,

    /// <summary>
    /// The window has been fullscreened, covering the entire surface of the monitor.
    /// </summary>
    Fullscreen
}
