namespace Graphics.Engine.Descriptions;

public struct ResourceLayoutDesc
{
    public LayoutElementDesc[] Elements { get; set; }

    public static ResourceLayoutDesc Default(params LayoutElementDesc[] elements)
    {
        return new()
        {
            Elements = elements
        };
    }
}
