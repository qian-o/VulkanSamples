using Graphics.Engine.Enums;

namespace Graphics.Engine;

public struct TexturePosition
{
    public uint X { get; set; }

    public uint Y { get; set; }

    public uint Z { get; set; }

    public uint MipLevel { get; set; }

    public CubeMapFace Face { get; set; }

    public static TexturePosition Default(Texture target,
                                          uint mipLevel = 0,
                                          CubeMapFace face = CubeMapFace.PositiveX)
    {
        return new TexturePosition
        {
            X = 0,
            Y = 0,
            Z = 0,
            MipLevel = mipLevel,
            Face = face
        };
    }
}
