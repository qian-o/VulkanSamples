using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class GraphicsPipeline(Context context,
                                       ref readonly GraphicsPipelineDesc desc) : Pipeline(context)
{
    public GraphicsPipelineDesc Desc { get; } = desc;
}
