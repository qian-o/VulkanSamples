using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class Buffer(Context context,
                             ref readonly BufferDesc desc) : DeviceResource(context)
{
    public BufferDesc Desc { get; } = desc;
}
