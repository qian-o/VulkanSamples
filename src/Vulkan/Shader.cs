using Graphics.Core;
using Graphics.Core.Helpers;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Shader : VulkanObject<VkShaderModule>
{
    internal Shader(VulkanResources vkRes, ref readonly ShaderDescription description) : base(vkRes, ObjectType.ShaderModule)
    {
        ShaderModuleCreateInfo createInfo = new()
        {
            SType = StructureType.ShaderModuleCreateInfo,
            CodeSize = (uint)description.ShaderBytes.Length,
            PCode = (uint*)description.ShaderBytes.AsPointer()
        };

        VkShaderModule shaderModule;
        VkRes.Vk.CreateShaderModule(VkRes.VkDevice, &createInfo, null, &shaderModule).ThrowCode();

        Handle = shaderModule;
        Stage = description.Stage;
        EntryPoint = description.EntryPoint;
    }

    internal override VkShaderModule Handle { get; }

    internal ShaderStages Stage { get; }

    internal string EntryPoint { get; }

    internal byte* PointerName => Alloter.Alloc(EntryPoint);

    internal PipelineShaderStageCreateInfo GetPipelineShaderStageCreateInfo()
    {
        return new()
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = Formats.GetShaderStage(Stage),
            Module = Handle,
            PName = PointerName
        };
    }

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    internal override void DestroyObject()
    {
        VkRes.Vk.DestroyShaderModule(VkRes.VkDevice, Handle, null);
    }
}
