namespace Graphics.Engine.Helpers;

internal sealed class Utils
{
    public static uint Lerp(uint a, uint b, float step)
    {
        return (uint)(a + ((b - a) * step));
    }

    public static uint GetMipLevels(uint width, uint height)
    {
        return (uint)MathF.Floor(MathF.Log2(MathF.Max(width, height))) + 1;
    }

    public static uint GetMipLevels(uint width, uint height, uint depth)
    {
        return (uint)MathF.Floor(MathF.Log2(MathF.Max(MathF.Max(width, height), depth))) + 1;
    }

    public static void GetMipDimensions(uint width, uint height, uint mipLevel, out uint mipWidth, out uint mipHeight)
    {
        mipWidth = Math.Max(1, width >> (int)mipLevel);
        mipHeight = Math.Max(1, height >> (int)mipLevel);
    }

    public static void GetMipDimensions(uint width, uint height, uint depth, uint mipLevel, out uint mipWidth, out uint mipHeight, out uint mipDepth)
    {
        mipWidth = Math.Max(1, width >> (int)mipLevel);
        mipHeight = Math.Max(1, height >> (int)mipLevel);
        mipDepth = Math.Max(1, depth >> (int)mipLevel);
    }
}
