using Graphics.Core.Helpers;
using Graphics.Engine.Descriptions;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKShader : Shader
{
    public VKShader(Context context,
                    ref readonly ShaderDesc desc) : base(context, in desc)
    {
        ShaderModuleCreateInfo createInfo = new()
        {
            SType = StructureType.ShaderModuleCreateInfo,
            CodeSize = (uint)desc.ShaderBytes.Length,
            PCode = (uint*)desc.ShaderBytes.AsPointer()
        };

        VkShader shader;
        Context.Vk.CreateShaderModule(Context.Device, &createInfo, null, &shader).ThrowCode();

        Shader = shader;
        ShaderStateInfo = new()
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = Formats.GetShaderStageFlags(desc.Stage),
            Module = Shader,
            PName = Allocator.Alloc(desc.EntryPoint)
        };
    }

    public new VKContext Context => (VKContext)base.Context;

    public VkShader Shader { get; }

    public PipelineShaderStageCreateInfo ShaderStateInfo { get; }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.ShaderModule, Shader.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyShaderModule(Context.Device, Shader, null);
    }
}
