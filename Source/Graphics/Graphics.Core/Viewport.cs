namespace Graphics.Core;

public record struct Viewport
{
    public Viewport(float x, float y, float width, float height, float minDepth, float maxDepth)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        MinDepth = minDepth;
        MaxDepth = maxDepth;
    }

    public float X { get; set; }

    public float Y { get; set; }

    public float Width { get; set; }

    public float Height { get; set; }

    public float MinDepth { get; set; }

    public float MaxDepth { get; set; }
}
