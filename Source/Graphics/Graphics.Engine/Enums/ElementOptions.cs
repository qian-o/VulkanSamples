namespace Graphics.Engine.Enums;

[Flags]
public enum ElementOptions
{
    /// <summary>
    /// No special options.
    /// </summary>
    None = 0,

    /// <summary>
    /// Create a dynamic binding.
    /// </summary>
    DynamicBinding = 1 << 0
}
