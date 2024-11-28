namespace Graphics.Engine.Descriptions;

public struct GraphicsShaderDesc
{
    public Shader? Vertex { get; set; }

    public Shader? Hull { get; set; }

    public Shader? Domain { get; set; }

    public Shader? Geometry { get; set; }

    public Shader? Pixel { get; set; }

    public static GraphicsShaderDesc Default(Shader? vertex = null,
                                             Shader? hull = null,
                                             Shader? domain = null,
                                             Shader? geometry = null,
                                             Shader? pixel = null)
    {
        return new()
        {
            Vertex = vertex,
            Hull = hull,
            Domain = domain,
            Geometry = geometry,
            Pixel = pixel
        };
    }
}
