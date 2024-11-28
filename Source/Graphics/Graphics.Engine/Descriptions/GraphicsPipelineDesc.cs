using Graphics.Engine.Enums;

namespace Graphics.Engine.Descriptions;

public struct GraphicsPipelineDesc
{
    /// <summary>
    /// The render state description.
    /// </summary>
    public RenderStateDesc RenderStates { get; set; }

    /// <summary>
    /// The shader state description.
    /// </summary>
    public GraphicsShaderDesc Shaders { get; set; }

    /// <summary>
    /// vertex input layout.
    /// </summary>
    public InputLayouts InputLayouts { get; set; }

    /// <summary>
    /// Describes the resource layouts input array.
    /// </summary>
    public ResourceLayout[] ResourceLayouts { get; set; }

    /// <summary>
    /// Define how vertices are interpreted and rendered by the pipeline.
    /// </summary>
    public PrimitiveTopology PrimitiveTopology { get; set; }

    /// <summary>
    /// A description of the output attachments of the pipeline.
    /// </summary>
    public OutputDesc Outputs { get; set; }

    public static GraphicsPipelineDesc Default(GraphicsShaderDesc shaders,
                                               InputLayouts inputLayouts,
                                               OutputDesc outputs,
                                               params ResourceLayout[] resourceLayouts)
    {
        return new()
        {
            RenderStates = RenderStateDesc.Default(),
            Shaders = shaders,
            InputLayouts = inputLayouts,
            ResourceLayouts = resourceLayouts,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            Outputs = outputs
        };
    }
}
