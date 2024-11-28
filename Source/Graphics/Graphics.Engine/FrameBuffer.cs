using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class FrameBuffer(Context context,
                                  ref readonly FrameBufferDesc desc) : DeviceResource(context)
{
    public FrameBufferDesc Desc { get; } = desc;

    public abstract uint Width { get; }

    public abstract uint Height { get; }
}
