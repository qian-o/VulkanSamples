using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct FrameBufferAttachmentDesc
{
    /// <summary>
    /// The target texture to render into.
    /// </summary>
    public Texture Target { get; set; }

    /// <summary>
    /// The format of the texture.
    /// </summary>
    public PixelFormat Format { get; set; }

    /// <summary>
    /// If the target is a cube map, the face to render to. (Cube Map exclusive)
    /// </summary>
    public CubeMapFace Face { get; set; }

    /// <summary>
    /// The mip level to render to.
    /// </summary>
    public uint MipLevel { get; set; }

    public static FrameBufferAttachmentDesc Default(Texture target)
    {
        return new()
        {
            Target = target,
            Format = target.Desc.Format,
            Face = CubeMapFace.PositiveX,
            MipLevel = 0
        };
    }
}
