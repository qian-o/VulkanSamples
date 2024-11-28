using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct TextureViewDesc
{
    /// <summary>
    /// The desired target.
    /// </summary>
    public Texture Target { get; set; }

    /// <summary>
    /// The format of the view.
    /// </summary>
    public PixelFormat Format { get; set; }

    /// <summary>
    /// The base mip level visible in the view. Must be less than the number of mip levels in the target Texture.
    /// </summary>
    public uint BaseMipLevel { get; set; }

    /// <summary>
    /// The number of mip levels visible in the view.
    /// </summary>
    public uint MipLevels { get; set; }

    /// <summary>
    /// If it is a cube map, it indicates the starting face to view. (Cube Map exclusive)
    /// </summary>
    public CubeMapFace BaseFace { get; set; }

    /// <summary>
    /// Number of faces to view. (Cube Map exclusive)
    /// </summary>
    public uint FaceCount { get; set; }

    public static TextureViewDesc Default(Texture target)
    {
        return new()
        {
            Target = target,
            Format = target.Desc.Format,
            BaseMipLevel = 0,
            MipLevels = target.Desc.MipLevels,
            BaseFace = CubeMapFace.PositiveX,
            FaceCount = 1
        };
    }
}
