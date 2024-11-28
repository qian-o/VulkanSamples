using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class Shader(Context context,
                             ref readonly ShaderDesc desc) : DeviceResource(context)
{
    public ShaderDesc Desc { get; } = desc;
}
