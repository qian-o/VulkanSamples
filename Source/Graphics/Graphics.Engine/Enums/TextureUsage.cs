namespace Graphics.Engine.Enums;

[Flags]
public enum TextureUsage
{
    /// <summary>
    /// No usage specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// The Texture can be used as the target of a read-only, and can be accessed from a shader.
    /// </summary>
    Sampled = 1 << 0,

    /// <summary>
    /// The Texture can be used as the target of a read-write, and can be accessed from a shader.
    /// </summary>
    Storage = 1 << 1,

    /// <summary>
    /// The Texture can be used as the color targe.
    /// </summary>
    RenderTarget = 1 << 2,

    /// <summary>
    /// The Texture can be used as the depth target.
    /// </summary>
    DepthStencil = 1 << 3,

    /// <summary>
    /// The Texture supports automatic generation of mipmaps.
    /// </summary>
    GenerateMipmaps = 1 << 4
}
