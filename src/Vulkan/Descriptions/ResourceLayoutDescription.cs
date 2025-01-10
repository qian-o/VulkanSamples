namespace Graphics.Vulkan.Descriptions;

public record struct ResourceLayoutDescription
{
    public ResourceLayoutDescription(params ElementDescription[] elements)
    {
        Elements = elements;
    }

    /// <summary>
    /// The array describes the elements in the resource layout.
    /// </summary>
    public ElementDescription[] Elements { get; set; }

    /// <summary>
    /// Last element in the array is a bindless resource.
    /// </summary>
    public bool IsLastBindless { get; set; }

    /// <summary>
    /// If the last element is bindless, this is the maximum number of descriptors.
    /// </summary>
    public uint MaxDescriptorCount { get; set; }

    public static ResourceLayoutDescription Bindless(uint maxDescriptorCount, params ElementDescription[] elements)
    {
        ResourceLayoutDescription description = new(elements)
        {
            IsLastBindless = true,
            MaxDescriptorCount = maxDescriptorCount
        };

        return description;
    }
}
