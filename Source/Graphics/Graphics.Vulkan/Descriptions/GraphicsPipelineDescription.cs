using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct GraphicsPipelineDescription
{
    public GraphicsPipelineDescription(BlendStateDescription blendState,
                                       DepthStencilStateDescription depthStencilState,
                                       RasterizerStateDescription rasterizerState,
                                       PrimitiveTopology primitiveTopology,
                                       GraphicsShaderDescription shaders,
                                       ResourceLayout[] resourceLayouts,
                                       OutputDescription outputs)
    {
        BlendState = blendState;
        DepthStencilState = depthStencilState;
        RasterizerState = rasterizerState;
        PrimitiveTopology = primitiveTopology;
        Shaders = shaders;
        ResourceLayouts = resourceLayouts;
        Outputs = outputs;
    }

    /// <summary>
    /// A description of the blend state, which controls how color values are blended into each color target.
    /// </summary>
    public BlendStateDescription BlendState { get; set; }

    /// <summary>
    /// A description of the depth stencil state, which controls depth tests, writing, and comparisons.
    /// </summary>
    public DepthStencilStateDescription DepthStencilState { get; set; }

    /// <summary>
    /// A description of the rasterizer state, which controls culling, clipping, scissor, and polygon-fill behavior.
    /// </summary>
    public RasterizerStateDescription RasterizerState { get; set; }

    /// <summary>
    /// This controls the primitive topology type.
    /// </summary>
    public PrimitiveTopology PrimitiveTopology { get; set; }

    /// <summary>
    /// A description of the shader set to be used.
    /// </summary>
    public GraphicsShaderDescription Shaders { get; set; }

    /// <summary>
    /// This controls the resource layout of the shaders.
    /// </summary>
    public ResourceLayout[] ResourceLayouts { get; set; }

    /// <summary>
    /// Describes the color and depth stencil outputs.
    /// </summary>
    public OutputDescription Outputs { get; set; }
}
