using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class ResourceSet(Context context,
                                  ref readonly ResourceSetDesc desc) : DeviceResource(context)
{
    public ResourceSetDesc Desc { get; } = desc;
}
