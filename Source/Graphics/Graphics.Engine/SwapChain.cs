using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class SwapChain(Context context,
                                ref readonly SwapChainDesc desc) : DeviceResource(context)
{
    public SwapChainDesc Desc { get; } = desc;

    public abstract FrameBuffer FrameBuffer { get; }

    public abstract void Present();

    public abstract void Resize();
}
