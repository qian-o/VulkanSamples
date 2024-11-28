using Graphics.Engine.Enums;
using Graphics.Engine.Helpers;

namespace Graphics.Engine;

public struct TextureRegion
{
    public uint X { get; set; }

    public uint Y { get; set; }

    public uint Z { get; set; }

    public uint Width { get; set; }

    public uint Height { get; set; }

    public uint Depth { get; set; }

    public uint MipLevel { get; set; }

    public CubeMapFace Face { get; set; }

    public static TextureRegion Default(Texture target,
                                        uint mipLevel = 0,
                                        CubeMapFace face = CubeMapFace.PositiveX)
    {
        Utils.GetMipDimensions(target.Desc.Width,
                               target.Desc.Height,
                               target.Desc.Depth,
                               mipLevel,
                               out uint width,
                               out uint height,
                               out uint depth);

        return new TextureRegion
        {
            X = 0,
            Y = 0,
            Z = 0,
            Width = width,
            Height = height,
            Depth = depth,
            MipLevel = mipLevel,
            Face = face
        };
    }

    public bool SizeEquals(TextureRegion other)
    {
        return Width == other.Width &&
               Height == other.Height &&
               Depth == other.Depth;
    }
}
