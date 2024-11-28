using Graphics.Core;

namespace Graphics.Vulkan.Descriptions;

public record struct GraphicsShaderDescription
{
    public GraphicsShaderDescription(VertexLayoutDescription[] vertexLayouts, Shader[] shaders, SpecializationConstant[] specializations)
    {
        VertexLayouts = vertexLayouts;
        Shaders = shaders;
        Specializations = specializations;
    }

    public GraphicsShaderDescription(VertexLayoutDescription[] vertexLayouts, Shader[] shaders) : this(vertexLayouts, shaders, [])
    {
    }

    /// <summary>
    /// An array describing the layout of each vertex buffer.
    /// Each element describes the layout of a buffer, including a description of each vertex attribute.
    /// </summary>
    public VertexLayoutDescription[] VertexLayouts { get; set; }

    /// <summary>
    /// An array describing the layout of each shader.
    /// Each element describes the layout of a shader, including the inputs and outputs of each shader.
    /// </summary>
    public Shader[] Shaders { get; set; }

    /// <summary>
    /// An array describing the value of each specialization constant.
    /// </summary>
    public SpecializationConstant[] Specializations { get; set; }
}
