using System.Text;
using Graphics.Core;
using Graphics.Vulkan.Descriptions;

namespace Graphics.Vulkan.Helpers;

public static class ResourceFactoryExtensions
{
    public static Shader[] CreateShaderByHLSL(this ResourceFactory factory,
                                              ShaderDescription[] descriptions,
                                              Func<string, byte[]>? includeResolver = null)
    {
        Shader[] shaders = new Shader[descriptions.Length];

        for (int i = 0; i < descriptions.Length; i++)
        {
            ShaderDescription description = descriptions[i];

            byte[] spirv = DxcHelpers.Compile(description.Stage,
                                              Encoding.UTF8.GetString(description.ShaderBytes),
                                              description.EntryPoint,
                                              includeResolver);

            shaders[i] = factory.CreateShader(new ShaderDescription(description.Stage, spirv, description.EntryPoint));
        }

        return shaders;
    }

    public static Shader[] CreateShaderByHLSL(this ResourceFactory factory, params ShaderDescription[] descriptions)
    {
        return factory.CreateShaderByHLSL(descriptions, null);
    }
}
