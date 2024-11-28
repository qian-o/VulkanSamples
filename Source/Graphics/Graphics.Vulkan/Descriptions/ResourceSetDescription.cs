namespace Graphics.Vulkan.Descriptions;

public record struct ResourceSetDescription
{
    public ResourceSetDescription(ResourceLayout layout, params IBindableResource?[] boundResources)
    {
        Layout = layout;
        BoundResources = boundResources;
    }

    /// <summary>
    /// Describes the number of resources and the layout.
    /// </summary>
    public ResourceLayout Layout { get; set; }

    /// <summary>
    /// Bound resources.
    /// Resource count and types must match the descriptions in Layout.
    /// If a null reference exists, it means that there is currently no bound resource at that position, and resources can be bound in subsequent operations.
    /// </summary>
    public IBindableResource?[] BoundResources { get; set; }
}
