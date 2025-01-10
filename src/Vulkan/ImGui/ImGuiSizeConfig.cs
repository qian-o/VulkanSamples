using System.Numerics;
using Hexa.NET.ImGui;

namespace Graphics.Vulkan.ImGui;

public record struct ImGuiSizeConfig
{
    public static ImGuiSizeConfig Default => new()
    {
        WindowPadding = new Vector2(8, 8),
        WindowRounding = 0.0f,
        WindowMinSize = new Vector2(32, 32),
        ChildRounding = 0.0f,
        PopupRounding = 0.0f,
        FramePadding = new Vector2(4, 3),
        FrameRounding = 0.0f,
        ItemSpacing = new Vector2(8, 4),
        ItemInnerSpacing = new Vector2(4, 4),
        CellPadding = new Vector2(4, 2),
        TouchExtraPadding = new Vector2(0, 0),
        IndentSpacing = 21.0f,
        ColumnsMinSpacing = 6.0f,
        ScrollbarSize = 14.0f,
        ScrollbarRounding = 9.0f,
        GrabMinSize = 12.0f,
        GrabRounding = 0.0f,
        LogSliderDeadzone = 4.0f,
        TabRounding = 4.0f,
        TabMinWidthForCloseButton = 0.0f,
        TabBarOverlineSize = 2.0f,
        SeparatorTextPadding = new Vector2(20, 3),
        DockingSeparatorSize = 2.0f,
        DisplayWindowPadding = new Vector2(19, 19),
        DisplaySafeAreaPadding = new Vector2(3, 3),
        MouseCursorScale = 1.0f
    };

    public Vector2 WindowPadding { get; set; }

    public float WindowRounding { get; set; }

    public Vector2 WindowMinSize { get; set; }

    public float ChildRounding { get; set; }

    public float PopupRounding { get; set; }

    public Vector2 FramePadding { get; set; }

    public float FrameRounding { get; set; }

    public Vector2 ItemSpacing { get; set; }

    public Vector2 ItemInnerSpacing { get; set; }

    public Vector2 CellPadding { get; set; }

    public Vector2 TouchExtraPadding { get; set; }

    public float IndentSpacing { get; set; }

    public float ColumnsMinSpacing { get; set; }

    public float ScrollbarSize { get; set; }

    public float ScrollbarRounding { get; set; }

    public float GrabMinSize { get; set; }

    public float GrabRounding { get; set; }

    public float LogSliderDeadzone { get; set; }

    public float TabRounding { get; set; }

    public float TabMinWidthForCloseButton { get; set; }

    public float TabBarOverlineSize { get; set; }

    public Vector2 SeparatorTextPadding { get; set; }

    public float DockingSeparatorSize { get; set; }

    public Vector2 DisplayWindowPadding { get; set; }

    public Vector2 DisplaySafeAreaPadding { get; set; }

    public float MouseCursorScale { get; set; }

    public ImGuiSizeConfig Scale(float scale)
    {
        ImGuiSizeConfig imGuiSizeConfig = new()
        {
            WindowPadding = ScaleVector2(WindowPadding, scale),
            WindowRounding = ScaleFloat(WindowRounding, scale),
            WindowMinSize = ScaleVector2(WindowMinSize, scale),
            ChildRounding = ScaleFloat(ChildRounding, scale),
            PopupRounding = ScaleFloat(PopupRounding, scale),
            FramePadding = ScaleVector2(FramePadding, scale),
            FrameRounding = ScaleFloat(FrameRounding, scale),
            ItemSpacing = ScaleVector2(ItemSpacing, scale),
            ItemInnerSpacing = ScaleVector2(ItemInnerSpacing, scale),
            CellPadding = ScaleVector2(CellPadding, scale),
            TouchExtraPadding = ScaleVector2(TouchExtraPadding, scale),
            IndentSpacing = ScaleFloat(IndentSpacing, scale),
            ColumnsMinSpacing = ScaleFloat(ColumnsMinSpacing, scale),
            ScrollbarSize = ScaleFloat(ScrollbarSize, scale),
            ScrollbarRounding = ScaleFloat(ScrollbarRounding, scale),
            GrabMinSize = ScaleFloat(GrabMinSize, scale),
            GrabRounding = ScaleFloat(GrabRounding, scale),
            LogSliderDeadzone = ScaleFloat(LogSliderDeadzone, scale),
            TabRounding = ScaleFloat(TabRounding, scale),
            TabMinWidthForCloseButton = ScaleFloat(TabMinWidthForCloseButton, scale),
            TabBarOverlineSize = ScaleFloat(TabBarOverlineSize, scale),
            SeparatorTextPadding = ScaleVector2(SeparatorTextPadding, scale),
            DockingSeparatorSize = ScaleFloat(DockingSeparatorSize, scale),
            DisplayWindowPadding = ScaleVector2(DisplayWindowPadding, scale),
            DisplaySafeAreaPadding = ScaleVector2(DisplaySafeAreaPadding, scale),
            MouseCursorScale = ScaleFloat(MouseCursorScale, scale)
        };

        return imGuiSizeConfig;
    }

    public readonly void Apply(ImGuiStylePtr style)
    {
        style.WindowPadding = WindowPadding;
        style.WindowRounding = WindowRounding;
        style.WindowMinSize = WindowMinSize;
        style.ChildRounding = ChildRounding;
        style.PopupRounding = PopupRounding;
        style.FramePadding = FramePadding;
        style.FrameRounding = FrameRounding;
        style.ItemSpacing = ItemSpacing;
        style.ItemInnerSpacing = ItemInnerSpacing;
        style.CellPadding = CellPadding;
        style.TouchExtraPadding = TouchExtraPadding;
        style.IndentSpacing = IndentSpacing;
        style.ColumnsMinSpacing = ColumnsMinSpacing;
        style.ScrollbarSize = ScrollbarSize;
        style.ScrollbarRounding = ScrollbarRounding;
        style.GrabMinSize = GrabMinSize;
        style.GrabRounding = GrabRounding;
        style.LogSliderDeadzone = LogSliderDeadzone;
        style.TabRounding = TabRounding;
        style.TabMinWidthForCloseButton = TabMinWidthForCloseButton;
        style.TabBarOverlineSize = TabBarOverlineSize;
        style.SeparatorTextPadding = SeparatorTextPadding;
        style.DockingSeparatorSize = DockingSeparatorSize;
        style.DisplayWindowPadding = DisplayWindowPadding;
        style.DisplaySafeAreaPadding = DisplaySafeAreaPadding;
        style.MouseCursorScale = MouseCursorScale;
    }

    private static float ScaleFloat(float value, float scale) => Convert.ToInt32(value * scale);

    private static Vector2 ScaleVector2(Vector2 value, float scale) => new(Convert.ToInt32(value.X * scale), Convert.ToInt32(value.Y * scale));
}
