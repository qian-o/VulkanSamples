using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class Texture(Context context,
                              ref readonly TextureDesc desc) : DeviceResource(context)
{
    public TextureDesc Desc { get; } = desc;
}
