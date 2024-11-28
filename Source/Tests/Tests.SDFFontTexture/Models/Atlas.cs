using System.Text.Json.Serialization;

namespace Tests.SDFFontTexture.Models;

internal sealed class Atlas
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AtlasType Type { get; set; }

    public int DistanceRange { get; set; }

    public int DistanceRangeMiddle { get; set; }

    public float Size { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public string? YOrigin { get; set; }
}
