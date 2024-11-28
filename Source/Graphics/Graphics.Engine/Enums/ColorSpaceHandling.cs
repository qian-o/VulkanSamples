namespace Graphics.Engine.Enums;

public enum ColorSpaceHandling
{
    /// <summary>
    /// Legacy-style color space handling. In this mode, the renderer will not convert sRGB vertex colors into linear space
    /// before blending them.
    /// </summary>
    Legacy,

    /// <summary>
    /// Improved color space handling. In this mode, the render will convert sRGB vertex colors into linear space before
    /// blending them with colors from user Textures.
    /// </summary>
    Linear
}
