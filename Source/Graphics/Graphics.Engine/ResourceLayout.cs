using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class ResourceLayout(Context context,
                                     ref readonly ResourceLayoutDesc desc) : DeviceResource(context)
{
    public ResourceLayoutDesc Desc { get; } = desc;
}
