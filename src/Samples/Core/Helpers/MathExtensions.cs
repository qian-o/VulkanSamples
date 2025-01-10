namespace Tests.Core.Helpers;

public static class MathExtensions
{
    public static float ToRadians(this float degrees)
    {
        return degrees * MathF.PI / 180.0f;
    }

    public static float ToDegrees(this float radians)
    {
        return radians * 180.0f / MathF.PI;
    }
}
