using Graphics.Windowing.Enums;
using Graphics.Windowing.Interfaces;
using Graphics.Windowing.Structs;
using Silk.NET.Maths;

namespace Graphics.Windowing;

public static class WindowManager
{
    private static readonly List<IWindow> windows = [];
    private static readonly List<IWindow> windowsToAdd = [];
    private static readonly List<IWindow> windowsToRemove = [];

    private static bool isRunning;

    public static void Loop()
    {
        isRunning = true;

        while (isRunning)
        {
            SdlManager.PollEvents();

            foreach (IWindow window in windows)
            {
                window.DoEvents();
            }

            foreach (IWindow window in windowsToAdd)
            {
                windows.Add(window);
            }

            foreach (IWindow window in windowsToRemove)
            {
                windows.Remove(window);
            }

            windowsToAdd.Clear();
            windowsToRemove.Clear();

            if (windows.Count == 0)
            {
                Stop();
            }

            foreach (IWindow window in windows)
            {
                window.DoUpdate();
                window.DoRender();
            }
        }
    }

    public static void Stop()
    {
        isRunning = false;
    }

    public static bool WindowFocused()
    {
        foreach (IWindow window in windows)
        {
            if (window.IsFocused)
            {
                return true;
            }
        }

        return false;
    }

    public static Vector2D<int> GetMousePosition()
    {
        return SdlManager.GetMousePosition();
    }

    public static bool IsMouseButtonDown(MouseButton button)
    {
        return SdlManager.IsMouseButtonDown(button);
    }

    public static void SetCursor(Cursor cursor)
    {
        SdlManager.SetCursor(cursor);
    }

    public static void SetTextInputRect(int x, int y, int w, int h)
    {
        SdlManager.SetTextInputRect(x, y, w, h);
    }

    public static int GetDisplayCount()
    {
        return SdlManager.GetDisplayCount();
    }

    public static Display GetDisplay(int index)
    {
        return SdlManager.GetDisplay(index);
    }

    public static Display[] GetDisplays()
    {
        int displayCount = GetDisplayCount();

        Display[] displays = new Display[displayCount];

        for (int i = 0; i < displayCount; i++)
        {
            displays[i] = GetDisplay(i);
        }

        return displays;
    }

    internal static void AddWindow(IWindow window)
    {
        windowsToAdd.Add(window);
    }

    internal static void RemoveWindow(IWindow window)
    {
        windowsToRemove.Add(window);
    }
}
