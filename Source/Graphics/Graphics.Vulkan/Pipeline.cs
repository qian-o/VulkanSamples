using System.Runtime.CompilerServices;
using Graphics.Core;
using Graphics.Core.Helpers;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

public unsafe class Pipeline : VulkanObject<VkPipeline>
{
    internal Pipeline(VulkanResources vkRes, ref readonly GraphicsPipelineDescription description) : base(vkRes, ObjectType.Pipeline)
    {
        GraphicsPipelineCreateInfo createInfo = new()
        {
            SType = StructureType.GraphicsPipelineCreateInfo
        };

        if (VkRes.DescriptorBufferSupported)
        {
            createInfo.Flags |= PipelineCreateFlags.CreateDescriptorBufferBitExt;
        }

        // blend state
        {
            PipelineColorBlendStateCreateInfo blendStateCreateInfo = new()
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo
            };

            uint attachmentCount = (uint)description.BlendState.AttachmentStates.Length;

            PipelineColorBlendAttachmentState[] colorBlendAttachmentStates = new PipelineColorBlendAttachmentState[attachmentCount];

            for (uint i = 0; i < attachmentCount; i++)
            {
                BlendAttachmentDescription attachmentState = description.BlendState.AttachmentStates[i];

                colorBlendAttachmentStates[i] = new PipelineColorBlendAttachmentState
                {
                    BlendEnable = attachmentState.BlendEnabled,
                    SrcColorBlendFactor = Formats.GetBlendFactor(attachmentState.SourceColorFactor),
                    DstColorBlendFactor = Formats.GetBlendFactor(attachmentState.DestinationColorFactor),
                    ColorBlendOp = Formats.GetBlendOp(attachmentState.ColorFunction),
                    SrcAlphaBlendFactor = Formats.GetBlendFactor(attachmentState.SourceAlphaFactor),
                    DstAlphaBlendFactor = Formats.GetBlendFactor(attachmentState.DestinationAlphaFactor),
                    AlphaBlendOp = Formats.GetBlendOp(attachmentState.AlphaFunction),
                    ColorWriteMask = Formats.GetColorWriteMask(attachmentState.ColorWriteMask)
                };
            }

            blendStateCreateInfo.AttachmentCount = attachmentCount;
            blendStateCreateInfo.PAttachments = colorBlendAttachmentStates.AsPointer();
            blendStateCreateInfo.BlendConstants[0] = description.BlendState.BlendFactor.R;
            blendStateCreateInfo.BlendConstants[1] = description.BlendState.BlendFactor.G;
            blendStateCreateInfo.BlendConstants[2] = description.BlendState.BlendFactor.B;
            blendStateCreateInfo.BlendConstants[3] = description.BlendState.BlendFactor.A;

            createInfo.PColorBlendState = &blendStateCreateInfo;
        }

        // rasterization state
        {
            PipelineRasterizationStateCreateInfo rasterizationStateCreateInfo = new()
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,
                PolygonMode = Formats.GetPolygonMode(description.RasterizerState.FillMode),
                CullMode = Formats.GetCullMode(description.RasterizerState.CullMode),
                FrontFace = Formats.GetFrontFace(description.RasterizerState.FrontFace),
                LineWidth = 1.0f,
                DepthClampEnable = !description.RasterizerState.DepthClipEnabled
            };

            createInfo.PRasterizationState = &rasterizationStateCreateInfo;
        }

        // dynamic state
        {
            PipelineDynamicStateCreateInfo dynamicStateCreateInfo = new()
            {
                SType = StructureType.PipelineDynamicStateCreateInfo
            };

            DynamicState[] dynamicStates = [DynamicState.Viewport, DynamicState.Scissor];

            dynamicStateCreateInfo.DynamicStateCount = (uint)dynamicStates.Length;
            dynamicStateCreateInfo.PDynamicStates = dynamicStates.AsPointer();

            createInfo.PDynamicState = &dynamicStateCreateInfo;
        }

        // depth stencil state
        {
            PipelineDepthStencilStateCreateInfo depthStencilStateCreateInfo = new()
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable = description.DepthStencilState.DepthTestEnabled,
                DepthWriteEnable = description.DepthStencilState.DepthWriteEnabled,
                DepthCompareOp = Formats.GetCompareOp(description.DepthStencilState.DepthComparison),
                DepthBoundsTestEnable = false,
                StencilTestEnable = description.DepthStencilState.StencilTestEnabled,
                Front = new StencilOpState
                {
                    FailOp = Formats.GetStencilOp(description.DepthStencilState.StencilFront.Fail),
                    PassOp = Formats.GetStencilOp(description.DepthStencilState.StencilFront.Pass),
                    DepthFailOp = Formats.GetStencilOp(description.DepthStencilState.StencilFront.DepthFail),
                    CompareOp = Formats.GetCompareOp(description.DepthStencilState.StencilFront.Comparison),
                    CompareMask = description.DepthStencilState.StencilReadMask,
                    WriteMask = description.DepthStencilState.StencilWriteMask,
                    Reference = description.DepthStencilState.StencilReference
                },
                Back = new StencilOpState
                {
                    FailOp = Formats.GetStencilOp(description.DepthStencilState.StencilBack.Fail),
                    PassOp = Formats.GetStencilOp(description.DepthStencilState.StencilBack.Pass),
                    DepthFailOp = Formats.GetStencilOp(description.DepthStencilState.StencilBack.DepthFail),
                    CompareOp = Formats.GetCompareOp(description.DepthStencilState.StencilBack.Comparison),
                    CompareMask = description.DepthStencilState.StencilReadMask,
                    WriteMask = description.DepthStencilState.StencilWriteMask,
                    Reference = description.DepthStencilState.StencilReference
                }
            };

            createInfo.PDepthStencilState = &depthStencilStateCreateInfo;
        }

        // multisample state
        {
            PipelineMultisampleStateCreateInfo multisampleStateCreateInfo = new()
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                RasterizationSamples = Formats.GetSampleCount(description.Outputs.SampleCount),
                AlphaToCoverageEnable = description.BlendState.AlphaToCoverageEnabled
            };

            createInfo.PMultisampleState = &multisampleStateCreateInfo;
        }

        // input assembly state
        {
            PipelineInputAssemblyStateCreateInfo inputAssemblyStateCreateInfo = new()
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = Formats.GetPrimitiveTopology(description.PrimitiveTopology),
                PrimitiveRestartEnable = false
            };

            createInfo.PInputAssemblyState = &inputAssemblyStateCreateInfo;
        }

        // vertex input state
        {
            uint bindingCount = (uint)description.Shaders.VertexLayouts.Length;
            uint attributeCount = (uint)description.Shaders.VertexLayouts.Sum(x => x.Elements.Length);

            VertexInputBindingDescription[] bindingDescriptions = new VertexInputBindingDescription[bindingCount];
            VertexInputAttributeDescription[] attributeDescriptions = new VertexInputAttributeDescription[attributeCount];

            uint targetIndex = 0;
            uint targetLocation = 0;
            for (uint binding = 0; binding < bindingCount; binding++)
            {
                VertexLayoutDescription vertexLayout = description.Shaders.VertexLayouts[binding];

                bindingDescriptions[binding] = new VertexInputBindingDescription
                {
                    Binding = binding,
                    Stride = vertexLayout.Stride,
                    InputRate = vertexLayout.InstanceStepRate == 0 ? VertexInputRate.Vertex : VertexInputRate.Instance
                };

                uint currentOffset = 0;
                for (uint location = 0; location < vertexLayout.Elements.Length; location++)
                {
                    VertexElementDescription elementDescription = vertexLayout.Elements[location];

                    attributeDescriptions[targetIndex++] = new VertexInputAttributeDescription
                    {
                        Binding = binding,
                        Location = targetLocation + location,
                        Format = Formats.GetVertexElementFormat(elementDescription.Format),
                        Offset = elementDescription.Offset != 0 ? elementDescription.Offset : currentOffset
                    };

                    currentOffset += FormatSizeHelpers.GetSizeInBytes(elementDescription.Format);
                }

                targetLocation += (uint)vertexLayout.Elements.Length;
            }

            PipelineVertexInputStateCreateInfo vertexInputStateCreateInfo = new()
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = bindingCount,
                PVertexBindingDescriptions = bindingDescriptions.AsPointer(),
                VertexAttributeDescriptionCount = attributeCount,
                PVertexAttributeDescriptions = attributeDescriptions.AsPointer()
            };

            createInfo.PVertexInputState = &vertexInputStateCreateInfo;
        }

        // shader stage
        {
            Shader[] shaders = description.Shaders.Shaders;
            SpecializationConstant[] specializations = description.Shaders.Specializations;

            SpecializationInfo specializationInfo = new();
            if (specializations.Length > 0)
            {
                uint specDataSize = (uint)specializations.Sum(x => FormatSizeHelpers.GetSizeInBytes(x.Type));

                byte* specData = stackalloc byte[(int)specDataSize];

                SpecializationMapEntry[] specializationMapEntries = new SpecializationMapEntry[specializations.Length];

                uint offset = 0;
                for (uint i = 0; i < specializations.Length; i++)
                {
                    SpecializationConstant specialization = specializations[i];

                    ulong data = specialization.Data;
                    uint dataSize = FormatSizeHelpers.GetSizeInBytes(specialization.Type);

                    specializationMapEntries[i] = new SpecializationMapEntry
                    {
                        ConstantID = specialization.ID,
                        Offset = offset,
                        Size = dataSize
                    };

                    Unsafe.CopyBlock(specData + offset, &data, dataSize);

                    offset += FormatSizeHelpers.GetSizeInBytes(specialization.Type);
                }

                specializationInfo.MapEntryCount = (uint)specializations.Length;
                specializationInfo.PMapEntries = specializationMapEntries.AsPointer();
                specializationInfo.DataSize = specDataSize;
                specializationInfo.PData = specData;
            }

            PipelineShaderStageCreateInfo[] shaderStageCreateInfos = new PipelineShaderStageCreateInfo[shaders.Length];
            for (int i = 0; i < shaders.Length; i++)
            {
                Shader shader = shaders[i];

                PipelineShaderStageCreateInfo pipelineShaderStageCreateInfo = shader.GetPipelineShaderStageCreateInfo();
                pipelineShaderStageCreateInfo.PSpecializationInfo = &specializationInfo;

                shaderStageCreateInfos[i] = pipelineShaderStageCreateInfo;
            }

            createInfo.StageCount = (uint)shaders.Length;
            createInfo.PStages = shaderStageCreateInfos.AsPointer();
        }

        // viewport state
        {
            PipelineViewportStateCreateInfo viewportStateCreateInfo = new()
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1,
                ScissorCount = 1
            };

            createInfo.PViewportState = &viewportStateCreateInfo;
        }

        // layout
        {
            PipelineLayoutCreateInfo layoutCreateInfo = new()
            {
                SType = StructureType.PipelineLayoutCreateInfo
            };

            uint descriptorSetLayoutCount = (uint)description.ResourceLayouts.Length;

            DescriptorSetLayout[] descriptorSetLayouts = new DescriptorSetLayout[descriptorSetLayoutCount];

            for (uint i = 0; i < descriptorSetLayoutCount; i++)
            {
                descriptorSetLayouts[i] = description.ResourceLayouts[i].Handle;
            }

            layoutCreateInfo.SetLayoutCount = descriptorSetLayoutCount;
            layoutCreateInfo.PSetLayouts = descriptorSetLayouts.AsPointer();

            PipelineLayout pipelineLayout;
            VkRes.Vk.CreatePipelineLayout(VkRes.VkDevice, &layoutCreateInfo, null, &pipelineLayout).ThrowCode();
            createInfo.Layout = pipelineLayout;
        }

        // compatible render target
        {
            RenderPassCreateInfo renderPassCreateInfo = new()
            {
                SType = StructureType.RenderPassCreateInfo
            };

            bool hasDepth = description.Outputs.DepthAttachment.HasValue;

            uint colorAttachmentCount = (uint)description.Outputs.ColorAttachments.Length;
            uint depthAttachmentCount = hasDepth ? 1u : 0u;
            uint attachmentCount = colorAttachmentCount + depthAttachmentCount;

            AttachmentDescription[] attachments = new AttachmentDescription[attachmentCount];
            AttachmentReference[] references = new AttachmentReference[attachmentCount];

            for (uint i = 0; i < colorAttachmentCount; i++)
            {
                attachments[i] = new AttachmentDescription
                {
                    Format = Formats.GetPixelFormat(description.Outputs.ColorAttachments[i].Format, false),
                    Samples = Formats.GetSampleCount(description.Outputs.SampleCount),
                    LoadOp = AttachmentLoadOp.DontCare,
                    StoreOp = AttachmentStoreOp.Store,
                    StencilLoadOp = AttachmentLoadOp.DontCare,
                    StencilStoreOp = AttachmentStoreOp.DontCare,
                    InitialLayout = ImageLayout.Undefined,
                    FinalLayout = ImageLayout.ShaderReadOnlyOptimal
                };

                references[i] = new AttachmentReference
                {
                    Attachment = i,
                    Layout = ImageLayout.ColorAttachmentOptimal
                };
            }

            if (hasDepth)
            {
                bool hasStencil = FormatHelpers.IsStencilFormat(description.Outputs.DepthAttachment!.Value.Format);

                attachments[^1] = new AttachmentDescription
                {
                    Format = Formats.GetPixelFormat(description.Outputs.DepthAttachment.Value.Format, true),
                    Samples = Formats.GetSampleCount(description.Outputs.SampleCount),
                    LoadOp = AttachmentLoadOp.DontCare,
                    StoreOp = AttachmentStoreOp.Store,
                    StencilLoadOp = AttachmentLoadOp.DontCare,
                    StencilStoreOp = hasStencil ? AttachmentStoreOp.Store : AttachmentStoreOp.DontCare,
                    InitialLayout = ImageLayout.Undefined,
                    FinalLayout = ImageLayout.DepthStencilAttachmentOptimal
                };

                references[^1] = new AttachmentReference
                {
                    Attachment = attachmentCount - 1,
                    Layout = ImageLayout.DepthStencilAttachmentOptimal
                };
            }

            SubpassDescription subpass = new()
            {
                PipelineBindPoint = PipelineBindPoint.Graphics
            };

            if (colorAttachmentCount > 0)
            {
                subpass.ColorAttachmentCount = colorAttachmentCount;
                subpass.PColorAttachments = references.AsPointer();
            }

            if (hasDepth)
            {
                subpass.PDepthStencilAttachment = references[^1].AsPointer();
            }

            SubpassDependency subpassDependency = new()
            {
                SrcSubpass = Vk.SubpassExternal,
                DstSubpass = 0,
                SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
                SrcAccessMask = AccessFlags.None,
                DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
                DstAccessMask = AccessFlags.ColorAttachmentReadBit | AccessFlags.ColorAttachmentWriteBit
            };

            renderPassCreateInfo.AttachmentCount = attachmentCount;
            renderPassCreateInfo.PAttachments = attachments.AsPointer();
            renderPassCreateInfo.SubpassCount = 1;
            renderPassCreateInfo.PSubpasses = &subpass;
            renderPassCreateInfo.DependencyCount = 1;
            renderPassCreateInfo.PDependencies = &subpassDependency;

            VkRenderPass renderPass;
            VkRes.Vk.CreateRenderPass(VkRes.VkDevice, &renderPassCreateInfo, null, &renderPass).ThrowCode();
            createInfo.RenderPass = renderPass;
        }

        VkPipeline pipeline;
        VkRes.Vk.CreateGraphicsPipelines(VkRes.VkDevice, default, 1, &createInfo, null, &pipeline).ThrowCode();

        Handle = pipeline;
        Layout = createInfo.Layout;
        RenderPass = createInfo.RenderPass;
        ShaderTable = null;
        PipelineBindPoint = PipelineBindPoint.Graphics;
        ResourceSetCount = (uint)description.ResourceLayouts.Length;
    }

    internal Pipeline(VulkanResources vkRes, ref readonly ComputePipelineDescription description) : base(vkRes, ObjectType.Pipeline)
    {
        ComputePipelineCreateInfo createInfo = new()
        {
            SType = StructureType.ComputePipelineCreateInfo
        };

        if (VkRes.DescriptorBufferSupported)
        {
            createInfo.Flags |= PipelineCreateFlags.CreateDescriptorBufferBitExt;
        }

        // shader stage
        {
            Shader shader = description.Shader;
            SpecializationConstant[] specializations = description.Specializations;

            SpecializationInfo specializationInfo = new();
            if (specializations.Length > 0)
            {
                uint specDataSize = (uint)specializations.Sum(x => FormatSizeHelpers.GetSizeInBytes(x.Type));

                byte* specData = stackalloc byte[(int)specDataSize];

                SpecializationMapEntry[] specializationMapEntries = new SpecializationMapEntry[specializations.Length];

                uint offset = 0;
                for (uint i = 0; i < specializations.Length; i++)
                {
                    SpecializationConstant specialization = specializations[i];

                    ulong data = specialization.Data;
                    uint dataSize = FormatSizeHelpers.GetSizeInBytes(specialization.Type);

                    specializationMapEntries[i] = new SpecializationMapEntry
                    {
                        ConstantID = specialization.ID,
                        Offset = offset,
                        Size = dataSize
                    };

                    Unsafe.CopyBlock(specData + offset, &data, dataSize);

                    offset += FormatSizeHelpers.GetSizeInBytes(specialization.Type);
                }

                specializationInfo.MapEntryCount = (uint)specializations.Length;
                specializationInfo.PMapEntries = specializationMapEntries.AsPointer();
                specializationInfo.DataSize = specDataSize;
                specializationInfo.PData = specData;
            }

            PipelineShaderStageCreateInfo pipelineShaderStageCreateInfo = shader.GetPipelineShaderStageCreateInfo();
            pipelineShaderStageCreateInfo.PSpecializationInfo = &specializationInfo;

            createInfo.Stage = pipelineShaderStageCreateInfo;
        }

        // layout
        {
            PipelineLayoutCreateInfo layoutCreateInfo = new()
            {
                SType = StructureType.PipelineLayoutCreateInfo
            };

            uint descriptorSetLayoutCount = (uint)description.ResourceLayouts.Length;

            DescriptorSetLayout[] descriptorSetLayouts = new DescriptorSetLayout[descriptorSetLayoutCount];

            for (uint i = 0; i < descriptorSetLayoutCount; i++)
            {
                descriptorSetLayouts[i] = description.ResourceLayouts[i].Handle;
            }

            layoutCreateInfo.SetLayoutCount = descriptorSetLayoutCount;
            layoutCreateInfo.PSetLayouts = descriptorSetLayouts.AsPointer();

            PipelineLayout pipelineLayout;
            VkRes.Vk.CreatePipelineLayout(VkRes.VkDevice, &layoutCreateInfo, null, &pipelineLayout).ThrowCode();
            createInfo.Layout = pipelineLayout;
        }

        VkPipeline pipeline;
        VkRes.Vk.CreateComputePipelines(VkRes.VkDevice, default, 1, &createInfo, null, &pipeline).ThrowCode();

        Handle = pipeline;
        Layout = createInfo.Layout;
        RenderPass = default;
        ShaderTable = null;
        PipelineBindPoint = PipelineBindPoint.Compute;
        ResourceSetCount = (uint)description.ResourceLayouts.Length;
    }

    internal Pipeline(VulkanResources vkRes, ref readonly RaytracingPipelineDescription description) : base(vkRes, ObjectType.Pipeline)
    {
        RayTracingPipelineCreateInfoKHR createInfo = new()
        {
            SType = StructureType.RayTracingPipelineCreateInfoKhr,
            MaxPipelineRayRecursionDepth = description.MaxTraceRecursionDepth
        };

        if (VkRes.DescriptorBufferSupported)
        {
            createInfo.Flags |= PipelineCreateFlags.CreateDescriptorBufferBitExt;
        }

        // shader stage
        {
            List<PipelineShaderStageCreateInfo> pipelineShaderStageCreateInfos = [];
            List<RayTracingShaderGroupCreateInfoKHR> rayTracingShaderGroupCreateInfos = [];

            if (description.Shaders.RayGenerationShader != null)
            {
                Shader shader = description.Shaders.RayGenerationShader;

                pipelineShaderStageCreateInfos.Add(shader.GetPipelineShaderStageCreateInfo());
                rayTracingShaderGroupCreateInfos.Add(new RayTracingShaderGroupCreateInfoKHR
                {
                    SType = StructureType.RayTracingShaderGroupCreateInfoKhr,
                    Type = RayTracingShaderGroupTypeKHR.GeneralKhr,
                    GeneralShader = (uint)(pipelineShaderStageCreateInfos.Count - 1),
                    ClosestHitShader = Vk.ShaderUnusedKhr,
                    AnyHitShader = Vk.ShaderUnusedKhr,
                    IntersectionShader = Vk.ShaderUnusedKhr
                });
            }

            if (description.Shaders.MissShader != null)
            {
                foreach (Shader shader in description.Shaders.MissShader)
                {
                    pipelineShaderStageCreateInfos.Add(shader.GetPipelineShaderStageCreateInfo());
                    rayTracingShaderGroupCreateInfos.Add(new RayTracingShaderGroupCreateInfoKHR
                    {
                        SType = StructureType.RayTracingShaderGroupCreateInfoKhr,
                        Type = RayTracingShaderGroupTypeKHR.GeneralKhr,
                        GeneralShader = (uint)(pipelineShaderStageCreateInfos.Count - 1),
                        ClosestHitShader = Vk.ShaderUnusedKhr,
                        AnyHitShader = Vk.ShaderUnusedKhr,
                        IntersectionShader = Vk.ShaderUnusedKhr
                    });
                }
            }

            if (description.Shaders.HitGroupShader != null)
            {
                foreach (HitGroupDescription hitGroupDescription in description.Shaders.HitGroupShader)
                {
                    uint closestHitShader = hitGroupDescription.ClosestHitShader != null ? (uint)pipelineShaderStageCreateInfos.Count : Vk.ShaderUnusedKhr;
                    uint anyHitShader = hitGroupDescription.AnyHitShader != null ? (uint)pipelineShaderStageCreateInfos.Count + 1 : Vk.ShaderUnusedKhr;
                    uint intersectionShader = hitGroupDescription.IntersectionShader != null ? (uint)pipelineShaderStageCreateInfos.Count + 2 : Vk.ShaderUnusedKhr;

                    if (closestHitShader != Vk.ShaderUnusedKhr)
                    {
                        pipelineShaderStageCreateInfos.Add(hitGroupDescription.ClosestHitShader!.GetPipelineShaderStageCreateInfo());
                    }

                    if (anyHitShader != Vk.ShaderUnusedKhr)
                    {
                        pipelineShaderStageCreateInfos.Add(hitGroupDescription.AnyHitShader!.GetPipelineShaderStageCreateInfo());
                    }

                    if (intersectionShader != Vk.ShaderUnusedKhr)
                    {
                        pipelineShaderStageCreateInfos.Add(hitGroupDescription.IntersectionShader!.GetPipelineShaderStageCreateInfo());
                    }

                    rayTracingShaderGroupCreateInfos.Add(new RayTracingShaderGroupCreateInfoKHR
                    {
                        SType = StructureType.RayTracingShaderGroupCreateInfoKhr,
                        Type = RayTracingShaderGroupTypeKHR.TrianglesHitGroupKhr,
                        GeneralShader = Vk.ShaderUnusedKhr,
                        ClosestHitShader = closestHitShader,
                        AnyHitShader = anyHitShader,
                        IntersectionShader = intersectionShader
                    });
                }
            }

            PipelineShaderStageCreateInfo[] stageCreateInfos = [.. pipelineShaderStageCreateInfos];
            RayTracingShaderGroupCreateInfoKHR[] rayTracingShaderGroups = [.. rayTracingShaderGroupCreateInfos];

            createInfo.StageCount = (uint)stageCreateInfos.Length;
            createInfo.PStages = stageCreateInfos.AsPointer();
            createInfo.GroupCount = (uint)rayTracingShaderGroups.Length;
            createInfo.PGroups = rayTracingShaderGroups.AsPointer();
        }

        // layout
        {
            PipelineLayoutCreateInfo layoutCreateInfo = new()
            {
                SType = StructureType.PipelineLayoutCreateInfo
            };

            uint descriptorSetLayoutCount = (uint)description.ResourceLayouts.Length;

            DescriptorSetLayout[] descriptorSetLayouts = new DescriptorSetLayout[descriptorSetLayoutCount];

            for (uint i = 0; i < descriptorSetLayoutCount; i++)
            {
                descriptorSetLayouts[i] = description.ResourceLayouts[i].Handle;
            }

            layoutCreateInfo.SetLayoutCount = descriptorSetLayoutCount;
            layoutCreateInfo.PSetLayouts = descriptorSetLayouts.AsPointer();

            PipelineLayout pipelineLayout;
            VkRes.Vk.CreatePipelineLayout(VkRes.VkDevice, &layoutCreateInfo, null, &pipelineLayout).ThrowCode();
            createInfo.Layout = pipelineLayout;
        }

        VkPipeline pipeline;
        VkRes.KhrRayTracingPipeline.CreateRayTracingPipelines(VkRes.VkDevice, default, default, 1, &createInfo, null, &pipeline).ThrowCode();

        Handle = pipeline;
        Layout = createInfo.Layout;
        RenderPass = default;
        ShaderTable = new ShaderTable(VkRes, this, in description);
        PipelineBindPoint = PipelineBindPoint.RayTracingKhr;
        ResourceSetCount = (uint)description.ResourceLayouts.Length;
    }

    internal override VkPipeline Handle { get; }

    internal VkPipelineLayout Layout { get; }

    internal VkRenderPass RenderPass { get; }

    internal ShaderTable? ShaderTable { get; }

    internal PipelineBindPoint PipelineBindPoint { get; }

    internal uint ResourceSetCount { get; set; }

    public bool IsGraphics => PipelineBindPoint == PipelineBindPoint.Graphics;

    public bool IsCompute => PipelineBindPoint == PipelineBindPoint.Compute;

    public bool IsRaytracing => PipelineBindPoint == PipelineBindPoint.RayTracingKhr;

    internal override ulong[] GetHandles()
    {
        return [Handle.Handle];
    }

    internal override void DestroyObject()
    {
        VkRes.Vk.DestroyPipeline(VkRes.VkDevice, Handle, null);
        VkRes.Vk.DestroyPipelineLayout(VkRes.VkDevice, Layout, null);

        if (IsGraphics)
        {
            VkRes.Vk.DestroyRenderPass(VkRes.VkDevice, RenderPass, null);
        }

        ShaderTable?.Dispose();
    }
}
