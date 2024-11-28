namespace Graphics.Engine.Enums;

public enum CommandProcessorType
{
    /// <summary>
    /// The command processor is a graphics command processor.
    /// </summary>
    Graphics,

    /// <summary>
    /// The command processor is a compute command processor.
    /// </summary>
    Compute,

    /// <summary>
    /// The command processor is a transfer command processor.
    /// </summary>
    Transfer
}
