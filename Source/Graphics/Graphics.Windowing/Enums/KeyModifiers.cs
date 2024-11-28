namespace Graphics.Windowing.Enums;

[Flags]
public enum KeyModifiers
{
    /// <summary>
    /// No modifiers.
    /// </summary>
    None = 0,

    /// <summary>
    /// The shift key.
    /// </summary>
    Shift = 1 << 0,

    /// <summary>
    /// The control key.
    /// </summary>
    Control = 1 << 1,

    /// <summary>
    /// The alt key.
    /// </summary>
    Alt = 1 << 2,

    /// <summary>
    /// The super key.
    /// </summary>
    Super = 1 << 3
}
