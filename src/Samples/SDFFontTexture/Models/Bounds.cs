namespace Tests.SDFFontTexture.Models;

internal struct Bounds
{
    public float Left { get; set; }

    public float Top { get; set; }

    public float Right { get; set; }

    public float Bottom { get; set; }

    public readonly float Width => Right - Left;

    public readonly float Height => Top - Bottom;
}
