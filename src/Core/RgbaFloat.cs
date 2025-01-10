namespace Graphics.Core;

public record struct RgbaFloat
{
    public static readonly RgbaFloat Red = new(1.0f, 0.0f, 0.0f, 1.0f);

    public static readonly RgbaFloat DarkRed = new(0.6f, 0.0f, 0.0f, 1.0f);

    public static readonly RgbaFloat Green = new(0.0f, 1.0f, 0.0f, 1.0f);

    public static readonly RgbaFloat Blue = new(0.0f, 0.0f, 1.0f, 1.0f);

    public static readonly RgbaFloat Yellow = new(1.0f, 1.0f, 0.0f, 1.0f);

    public static readonly RgbaFloat Grey = new(0.25f, 0.25f, 0.25f, 1.0f);

    public static readonly RgbaFloat LightGrey = new(0.65f, 0.65f, 0.65f, 1.0f);

    public static readonly RgbaFloat Cyan = new(0.0f, 1.0f, 1.0f, 1.0f);

    public static readonly RgbaFloat White = new(1.0f, 1.0f, 1.0f, 1.0f);

    public static readonly RgbaFloat CornflowerBlue = new(0.3921f, 0.5843f, 0.9294f, 1.0f);

    public static readonly RgbaFloat Clear = new(0.0f, 0.0f, 0.0f, 0.0f);

    public static readonly RgbaFloat Black = new(0.0f, 0.0f, 0.0f, 1.0f);

    public static readonly RgbaFloat Pink = new(1.0f, 0.45f, 0.75f, 1.0f);

    public static readonly RgbaFloat Orange = new(1.0f, 0.36f, 0.0f, 1.0f);

    public RgbaFloat(float r, float g, float b, float a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public float R { get; set; }

    public float G { get; set; }

    public float B { get; set; }

    public float A { get; set; }
}
