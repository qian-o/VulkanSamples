namespace Graphics.Vulkan.Descriptions;

public record struct FramebufferAttachmentDescription
{
    public FramebufferAttachmentDescription(Texture target, uint arrayLayer, uint mipLevel)
    {
        if (arrayLayer >= target.ArrayLayers)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayLayer), "Array layer is out of range.");
        }

        if (mipLevel >= target.MipLevels)
        {
            throw new ArgumentOutOfRangeException(nameof(mipLevel), "Mip level is out of range.");
        }

        Target = target;
        ArrayLayer = arrayLayer;
        MipLevel = mipLevel;
    }

    public FramebufferAttachmentDescription(Texture target) : this(target, 0, 0)
    {
    }

    /// <summary>
    /// The target texture to render into.
    /// </summary>
    public Texture Target { get; set; }

    /// <summary>
    /// The array layer to render to.
    /// </summary>
    public uint ArrayLayer { get; set; }

    /// <summary>
    /// The mip level to render to.
    /// </summary>
    public uint MipLevel { get; set; }
}
