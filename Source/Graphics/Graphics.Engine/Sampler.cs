using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class Sampler(Context context,
                              ref readonly SamplerDesc desc) : DeviceResource(context)
{
    public SamplerDesc Desc { get; } = desc;
}
