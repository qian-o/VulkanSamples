namespace Graphics.Engine;

public struct Viewport(float x, float y, float width, float height, float minDepth, float maxDepth)
{
    public float X { get; set; } = x;

    public float Y { get; set; } = y;

    public float Width { get; set; } = width;

    public float Height { get; set; } = height;

    public float MinDepth { get; set; } = minDepth;

    public float MaxDepth { get; set; } = maxDepth;
}
