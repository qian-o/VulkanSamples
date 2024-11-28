namespace Graphics.Engine.Descriptions;

public struct ResourceSetDesc
{
    /// <summary>
    /// The layout of the resource set.
    /// </summary>
    public ResourceLayout Layout { get; set; }

    /// <summary>
    /// An array of resources that are bound to the resource set.
    /// </summary>
    public DeviceResource[] Resources { get; set; }

    public static ResourceSetDesc Default(ResourceLayout layout, params DeviceResource[] resources)
    {
        return new()
        {
            Layout = layout,
            Resources = resources
        };
    }
}
