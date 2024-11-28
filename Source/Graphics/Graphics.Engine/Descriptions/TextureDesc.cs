using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct TextureDesc
{
    /// <summary>
    /// The type of the texture.
    /// </summary>
    public TextureType Type { get; set; }

    /// <summary>
    /// The format of individual texture elements.
    /// </summary>
    public PixelFormat Format { get; set; }

    /// <summary>
    /// The total width, in texels.
    /// </summary>
    public uint Width { get; set; }

    /// <summary>
    /// The total height, in texels.
    /// </summary>
    public uint Height { get; set; }

    /// <summary>
    /// The total depth, in texels.
    /// </summary>
    public uint Depth { get; set; }

    /// <summary>
    /// The number of mipmap levels.
    /// </summary>
    public uint MipLevels { get; set; }

    /// <summary>
    /// Controls how the Texture is permitted to be used. If the Texture will be sampled from a shader.
    /// </summary>
    public TextureUsage Usage { get; set; }

    /// <summary>
    /// The number of samples.
    /// </summary>
    public TextureSampleCount SampleCount { get; set; }

    public static TextureDesc Default1D(uint width,
                                        uint mipLevels,
                                        PixelFormat format = PixelFormat.R8G8B8A8UNorm)
    {
        return new()
        {
            Type = TextureType.Texture1D,
            Format = format,
            Width = width,
            Height = 1,
            Depth = 1,
            MipLevels = mipLevels,
            Usage = TextureUsage.Sampled,
            SampleCount = TextureSampleCount.Count1
        };
    }

    public static TextureDesc Default2D(uint width,
                                        uint height,
                                        uint mipLevels,
                                        PixelFormat format = PixelFormat.R8G8B8A8UNorm)
    {
        return new()
        {
            Type = TextureType.Texture2D,
            Format = format,
            Width = width,
            Height = height,
            Depth = 1,
            MipLevels = mipLevels,
            Usage = TextureUsage.Sampled,
            SampleCount = TextureSampleCount.Count1
        };
    }

    public static TextureDesc Default3D(uint width,
                                        uint height,
                                        uint depth,
                                        uint mipLevels,
                                        PixelFormat format = PixelFormat.R8G8B8A8UNorm)
    {
        return new()
        {
            Type = TextureType.Texture3D,
            Format = format,
            Width = width,
            Height = height,
            Depth = depth,
            MipLevels = mipLevels,
            Usage = TextureUsage.Sampled,
            SampleCount = TextureSampleCount.Count1
        };
    }

    public static TextureDesc DefaultCube(uint width,
                                          uint height,
                                          uint mipLevels,
                                          PixelFormat format = PixelFormat.R8G8B8A8UNorm)
    {
        return new()
        {
            Type = TextureType.TextureCube,
            Format = format,
            Width = width,
            Height = height,
            Depth = 1,
            MipLevels = mipLevels,
            Usage = TextureUsage.Sampled,
            SampleCount = TextureSampleCount.Count1
        };
    }
}
