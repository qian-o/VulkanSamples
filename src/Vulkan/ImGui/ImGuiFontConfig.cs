using Hexa.NET.ImGui;

namespace Graphics.Vulkan.ImGui;

public record struct ImGuiFontConfig
{
    private const string DefaultFontPath = "ProggyClean.ttf";

    public static ImGuiFontConfig Default => new(DefaultFontPath, 13);

    public ImGuiFontConfig(string fontPath, int fontSize, Func<ImGuiIOPtr, nint>? getGlyphRange = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(fontSize);

        FontPath = fontPath;
        FontSize = fontSize;
        GetGlyphRange = getGlyphRange;
    }

    public string FontPath { get; set; }

    public int FontSize { get; set; }

    public Func<ImGuiIOPtr, nint>? GetGlyphRange { get; set; }

    public readonly bool IsDefault => FontPath == DefaultFontPath;
}
