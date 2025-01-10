using Graphics.Windowing.Enums;
using Graphics.Windowing.Structs;
using Silk.NET.Maths;
using Silk.NET.SDL;
using Cursor = Graphics.Windowing.Enums.Cursor;
using SdlCursor = Silk.NET.SDL.Cursor;

namespace Graphics.Windowing;

internal static unsafe class SdlManager
{
    private static readonly Dictionary<Scancode, Key> keyMap;
    private static readonly Dictionary<byte, MouseButton> mouseButtonMap;
    private static readonly Dictionary<Cursor, nint> cursorMap;

    static SdlManager()
    {
        keyMap = new()
        {
            { Scancode.ScancodeUnknown, Key.Unknown },
            { Scancode.ScancodeSpace, Key.Space },
            { Scancode.ScancodeApostrophe, Key.Apostrophe },
            { Scancode.ScancodeComma, Key.Comma },
            { Scancode.ScancodeMinus, Key.Minus },
            { Scancode.ScancodePeriod, Key.Period },
            { Scancode.ScancodeSlash, Key.Slash },
            { Scancode.Scancode0, Key.Number0 },
            { Scancode.Scancode1, Key.Number1 },
            { Scancode.Scancode2, Key.Number2 },
            { Scancode.Scancode3, Key.Number3 },
            { Scancode.Scancode4, Key.Number4 },
            { Scancode.Scancode5, Key.Number5 },
            { Scancode.Scancode6, Key.Number6 },
            { Scancode.Scancode7, Key.Number7 },
            { Scancode.Scancode8, Key.Number8 },
            { Scancode.Scancode9, Key.Number9 },
            { Scancode.ScancodeSemicolon, Key.Semicolon },
            { Scancode.ScancodeEquals, Key.Equal },
            { Scancode.ScancodeA, Key.A },
            { Scancode.ScancodeB, Key.B },
            { Scancode.ScancodeC, Key.C },
            { Scancode.ScancodeD, Key.D },
            { Scancode.ScancodeE, Key.E },
            { Scancode.ScancodeF, Key.F },
            { Scancode.ScancodeG, Key.G },
            { Scancode.ScancodeH, Key.H },
            { Scancode.ScancodeI, Key.I },
            { Scancode.ScancodeJ, Key.J },
            { Scancode.ScancodeK, Key.K },
            { Scancode.ScancodeL, Key.L },
            { Scancode.ScancodeM, Key.M },
            { Scancode.ScancodeN, Key.N },
            { Scancode.ScancodeO, Key.O },
            { Scancode.ScancodeP, Key.P },
            { Scancode.ScancodeQ, Key.Q },
            { Scancode.ScancodeR, Key.R },
            { Scancode.ScancodeS, Key.S },
            { Scancode.ScancodeT, Key.T },
            { Scancode.ScancodeU, Key.U },
            { Scancode.ScancodeV, Key.V },
            { Scancode.ScancodeW, Key.W },
            { Scancode.ScancodeX, Key.X },
            { Scancode.ScancodeY, Key.Y },
            { Scancode.ScancodeZ, Key.Z },
            { Scancode.ScancodeLeftbracket, Key.LeftBracket },
            { Scancode.ScancodeBackslash, Key.BackSlash },
            { Scancode.ScancodeRightbracket, Key.RightBracket },
            { Scancode.ScancodeGrave, Key.GraveAccent },
            { Scancode.ScancodeEscape, Key.Escape },
            { Scancode.ScancodeReturn, Key.Enter },
            { Scancode.ScancodeTab, Key.Tab },
            { Scancode.ScancodeBackspace, Key.Backspace },
            { Scancode.ScancodeInsert, Key.Insert },
            { Scancode.ScancodeDelete, Key.Delete },
            { Scancode.ScancodeRight, Key.Right },
            { Scancode.ScancodeLeft, Key.Left },
            { Scancode.ScancodeDown, Key.Down },
            { Scancode.ScancodeUp, Key.Up },
            { Scancode.ScancodePageup, Key.PageUp },
            { Scancode.ScancodePagedown, Key.PageDown },
            { Scancode.ScancodeHome, Key.Home },
            { Scancode.ScancodeEnd, Key.End },
            { Scancode.ScancodeCapslock, Key.CapsLock },
            { Scancode.ScancodeScrolllock, Key.ScrollLock },
            { Scancode.ScancodeNumlockclear, Key.NumLock },
            { Scancode.ScancodePrintscreen, Key.PrintScreen },
            { Scancode.ScancodePause, Key.Pause },
            { Scancode.ScancodeF1, Key.F1 },
            { Scancode.ScancodeF2, Key.F2 },
            { Scancode.ScancodeF3, Key.F3 },
            { Scancode.ScancodeF4, Key.F4 },
            { Scancode.ScancodeF5, Key.F5 },
            { Scancode.ScancodeF6, Key.F6 },
            { Scancode.ScancodeF7, Key.F7 },
            { Scancode.ScancodeF8, Key.F8 },
            { Scancode.ScancodeF9, Key.F9 },
            { Scancode.ScancodeF10, Key.F10 },
            { Scancode.ScancodeF11, Key.F11 },
            { Scancode.ScancodeF12, Key.F12 },
            { Scancode.ScancodeF13, Key.F13 },
            { Scancode.ScancodeF14, Key.F14 },
            { Scancode.ScancodeF15, Key.F15 },
            { Scancode.ScancodeF16, Key.F16 },
            { Scancode.ScancodeF17, Key.F17 },
            { Scancode.ScancodeF18, Key.F18 },
            { Scancode.ScancodeF19, Key.F19 },
            { Scancode.ScancodeF20, Key.F20 },
            { Scancode.ScancodeF21, Key.F21 },
            { Scancode.ScancodeF22, Key.F22 },
            { Scancode.ScancodeF23, Key.F23 },
            { Scancode.ScancodeF24, Key.F24 },
            { Scancode.ScancodeKP0, Key.Keypad0 },
            { Scancode.ScancodeKP1, Key.Keypad1 },
            { Scancode.ScancodeKP2, Key.Keypad2 },
            { Scancode.ScancodeKP3, Key.Keypad3 },
            { Scancode.ScancodeKP4, Key.Keypad4 },
            { Scancode.ScancodeKP5, Key.Keypad5 },
            { Scancode.ScancodeKP6, Key.Keypad6 },
            { Scancode.ScancodeKP7, Key.Keypad7 },
            { Scancode.ScancodeKP8, Key.Keypad8 },
            { Scancode.ScancodeKP9, Key.Keypad9 },
            { Scancode.ScancodeKPDecimal, Key.KeypadDecimal },
            { Scancode.ScancodeKPDivide, Key.KeypadDivide },
            { Scancode.ScancodeKPMultiply, Key.KeypadMultiply },
            { Scancode.ScancodeKPMinus, Key.KeypadSubtract },
            { Scancode.ScancodeKPPlus, Key.KeypadAdd },
            { Scancode.ScancodeKPEnter, Key.KeypadEnter },
            { Scancode.ScancodeKPEquals, Key.KeypadEqual },
            { Scancode.ScancodeLshift, Key.ShiftLeft },
            { Scancode.ScancodeLctrl, Key.ControlLeft },
            { Scancode.ScancodeLalt, Key.AltLeft },
            { Scancode.ScancodeLgui, Key.SuperLeft },
            { Scancode.ScancodeRshift, Key.ShiftRight },
            { Scancode.ScancodeRctrl, Key.ControlRight },
            { Scancode.ScancodeRalt, Key.AltRight },
            { Scancode.ScancodeRgui, Key.SuperRight },
            { Scancode.ScancodeMenu, Key.Menu }
        };
        mouseButtonMap = new()
        {
            { 1, MouseButton.Left },
            { 2, MouseButton.Middle },
            { 3, MouseButton.Right }
        };
        cursorMap = new()
        {
            { Cursor.Arrow, (nint)Sdl.CreateSystemCursor(SystemCursor.SystemCursorArrow) },
            { Cursor.TextInput, (nint)Sdl.CreateSystemCursor(SystemCursor.SystemCursorIbeam) },
            { Cursor.ResizeAll, (nint)Sdl.CreateSystemCursor(SystemCursor.SystemCursorSizeall) },
            { Cursor.ResizeNS, (nint)Sdl.CreateSystemCursor(SystemCursor.SystemCursorSizens) },
            { Cursor.ResizeWE, (nint)Sdl.CreateSystemCursor(SystemCursor.SystemCursorSizewe) },
            { Cursor.ResizeNESW, (nint)Sdl.CreateSystemCursor(SystemCursor.SystemCursorSizenesw) },
            { Cursor.ResizeNWSE, (nint)Sdl.CreateSystemCursor(SystemCursor.SystemCursorSizenwse) },
            { Cursor.Hand, (nint)Sdl.CreateSystemCursor(SystemCursor.SystemCursorHand) },
            { Cursor.NotAllowed, (nint)Sdl.CreateSystemCursor(SystemCursor.SystemCursorNo) }
        };
    }

    public static Sdl Sdl { get; } = Sdl.GetApi();

    public static List<Event> Events { get; } = [];

    public static void PollEvents()
    {
        Events.Clear();

        Event ev;
        while (Sdl.PollEvent(&ev) == (int)SdlBool.True)
        {
            Events.Add(ev);
        }
    }

    public static Key GetKey(Scancode scancode)
    {
        return keyMap.TryGetValue(scancode, out Key key) ? key : Key.Unknown;
    }

    public static KeyModifiers GetKeyModifiers(Keymod keymod)
    {
        KeyModifiers keyModifiers = KeyModifiers.None;

        if (keymod.HasFlag(Keymod.Lshift) || keymod.HasFlag(Keymod.Rshift) || keymod.HasFlag(Keymod.Shift))
        {
            keyModifiers |= KeyModifiers.Shift;
        }

        if (keymod.HasFlag(Keymod.Lctrl) || keymod.HasFlag(Keymod.Rctrl) || keymod.HasFlag(Keymod.Ctrl))
        {
            keyModifiers |= KeyModifiers.Control;
        }

        if (keymod.HasFlag(Keymod.Lalt) || keymod.HasFlag(Keymod.Ralt) || keymod.HasFlag(Keymod.Alt))
        {
            keyModifiers |= KeyModifiers.Alt;
        }

        if (keymod.HasFlag(Keymod.Lgui) || keymod.HasFlag(Keymod.Rgui) || keymod.HasFlag(Keymod.Gui))
        {
            keyModifiers |= KeyModifiers.Super;
        }

        return keyModifiers;
    }

    public static MouseButton GetMouseButton(byte button)
    {
        return mouseButtonMap.TryGetValue(button, out MouseButton mouseButton) ? mouseButton : MouseButton.Unknown;
    }

    public static Vector2D<int> GetMousePosition()
    {
        int x, y;
        Sdl.GetGlobalMouseState(&x, &y);

        return new Vector2D<int>(x, y);
    }

    public static bool IsMouseButtonDown(MouseButton button)
    {
        int mask = button switch
        {
            MouseButton.Left => 0,
            MouseButton.Middle => 1,
            MouseButton.Right => 2,
            _ => (int)button - 1,
        };

        return (Sdl.GetGlobalMouseState(null, null) & (1 << mask)) != 0;
    }

    public static void SetCursor(Cursor cursor)
    {
        Sdl.SetCursor((SdlCursor*)cursorMap[cursor]);
    }

    public static void SetTextInputRect(int x, int y, int w, int h)
    {
        Rectangle<int> rect = new(x, y, w, h);

        Sdl.SetTextInputRect(&rect);
    }

    public static int GetDisplayCount()
    {
        return Sdl.GetNumVideoDisplays();
    }

    public static Display GetDisplay(int index)
    {
        string name = Sdl.GetDisplayNameS(index);

        Rectangle<int> main;
        Sdl.GetDisplayBounds(index, &main);

        Rectangle<int> work;
        Sdl.GetDisplayUsableBounds(index, &work);

        float ddpi;
        Sdl.GetDisplayDPI(index, &ddpi, null, null);

        return new Display(index,
                           name,
                           main.Origin,
                           main.Size,
                           work.Origin,
                           work.Size,
                           ddpi == 0 ? 1.0f : ddpi / 96.0f);
    }
}
