namespace Graphics.Engine;

public abstract class CommandProcessor(Context context) : DeviceResource(context)
{
    /// <summary>
    /// Gets the next available command buffer.
    /// </summary>
    /// <returns></returns>
    public abstract CommandBuffer CommandBuffer();

    /// <summary>
    /// Submits the command buffer to be executed by the GPU.
    /// </summary>
    /// <param name="isSyncUpToGpu"></param>
    public abstract void Submit(bool isSyncUpToGpu = true);

    /// <summary>
    /// Waits for the GPU to finish executing all commands.
    /// </summary>
    public abstract void WaitIdle();
}
