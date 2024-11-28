using Graphics.Engine.Enums;
using System.Numerics;

namespace Graphics.Engine;

public struct ClearValue
{
    public Vector4[] ColorValues { get; set; }

    public float Depth { get; set; }

    public byte Stencil { get; set; }

    public ClearOptions Options { get; set; }

    public static ClearValue Default(int colorCount = 1,
                                     Vector4? color = null,
                                     ClearOptions options = ClearOptions.All)
    {
        color ??= Vector4.Zero;

        Vector4[] colorValues = new Vector4[colorCount];
        Array.Fill(colorValues, color.Value);

        return new ClearValue
        {
            ColorValues = colorValues,
            Depth = 1.0f,
            Stencil = 0,
            Options = options
        };
    }
}
