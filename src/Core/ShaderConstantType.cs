namespace Graphics.Core;

public enum ShaderConstantType : byte
{
    /// <summary>
    /// A boolean.
    /// </summary>
    ConstBool,

    /// <summary>
    /// A 16-bit signed integer.
    /// </summary>
    ConstInt16,

    /// <summary>
    /// A 16-bit unsigned integer.
    /// </summary>
    ConstUInt16,

    /// <summary>
    /// A 32-bit signed integer.
    /// </summary>
    ConstInt32,

    /// <summary>
    /// A 32-bit unsigned integer.
    /// </summary>
    ConstUInt32,

    /// <summary>
    /// A 64-bit signed integer.
    /// </summary>
    ConstInt64,

    /// <summary>
    /// A 64-bit unsigned integer.
    /// </summary>
    ConstUInt64,

    /// <summary>
    /// A 32-bit floating-point value.
    /// </summary>
    ConstFloat,

    /// <summary>
    /// A 64-bit floating-point value.
    /// </summary>
    ConstDouble
}
