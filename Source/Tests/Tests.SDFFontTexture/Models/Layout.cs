using System.Text.Json;

namespace Tests.SDFFontTexture.Models;

internal sealed class Layout(Atlas atlas, Metrics metrics, Glyph[] glyphs)
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerOptions.Default)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Atlas Atlas { get; } = atlas;

    public Metrics Metrics { get; } = metrics;

    public Glyph[] Glyphs { get; } = glyphs;

    public string? PngPath { get; set; }

    public bool IsValid => Atlas != null && Metrics != null && Glyphs != null && Glyphs.Length > 0;

    public static Layout Get(string json, string pngPath)
    {
        Layout layout = JsonSerializer.Deserialize<Layout>(json, jsonSerializerOptions)!;

        layout.PngPath = pngPath;

        return layout;
    }
}
