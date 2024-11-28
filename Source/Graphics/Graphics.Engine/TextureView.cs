using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class TextureView(Context context,
                                  ref readonly TextureViewDesc desc) : DeviceResource(context)
{
    public TextureViewDesc Desc { get; } = desc;
}
