using Graphics.Core.Helpers;
using Graphics.Engine.Descriptions;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKResourceLayout : ResourceLayout
{
    public VKResourceLayout(Context context,
                            ref readonly ResourceLayoutDesc desc) : base(context, in desc)
    {
        DescriptorSetLayoutBinding[] bindings = new DescriptorSetLayoutBinding[desc.Elements.Length];

        for (uint i = 0; i < desc.Elements.Length; i++)
        {
            LayoutElementDesc element = desc.Elements[i];

            DescriptorSetLayoutBinding binding = new()
            {
                Binding = VKHelpers.GetBinding(element),
                DescriptorType = Formats.GetDescriptorType(element.Type, element.Options),
                DescriptorCount = 1,
                StageFlags = Formats.GetShaderStageFlags(element.Stages)
            };

            bindings[i] = binding;
        }

        DescriptorSetLayoutCreateInfo createInfo = new()
        {
            SType = StructureType.DescriptorSetLayoutCreateInfo,
            BindingCount = (uint)bindings.Length,
            PBindings = bindings.AsPointer()
        };

        VkDescriptorSetLayout descriptorSetLayout;
        Context.Vk.CreateDescriptorSetLayout(Context.Device, &createInfo, null, &descriptorSetLayout).ThrowCode();

        DescriptorSetLayout = descriptorSetLayout;
    }

    public new VKContext Context => (VKContext)base.Context;

    public VkDescriptorSetLayout DescriptorSetLayout { get; }

    protected override void SetName(string name)
    {
        Context.SetDebugName(ObjectType.DescriptorSetLayout, DescriptorSetLayout.Handle, name);
    }

    protected override void Destroy()
    {
        Context.Vk.DestroyDescriptorSetLayout(Context.Device, DescriptorSetLayout, null);
    }
}
