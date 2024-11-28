using Graphics.Core.Helpers;
using Graphics.Engine.Exceptions;
using Silk.NET.Vulkan;

namespace Graphics.Engine.Vulkan;

internal unsafe partial class VKContext
{
    public VkPhysicalDevice PhysicalDevice { get; private set; }

    public uint FindMemoryTypeIndex(uint typeFilter, MemoryPropertyFlags flags)
    {
        PhysicalDeviceMemoryProperties memoryProperties;
        Vk.GetPhysicalDeviceMemoryProperties(PhysicalDevice, &memoryProperties);

        for (int i = 0; i < memoryProperties.MemoryTypeCount; i++)
        {
            if ((typeFilter & (1 << i)) != 0 && memoryProperties.MemoryTypes[i].PropertyFlags.HasFlag(flags))
            {
                return (uint)i;
            }
        }

        throw new BackendException("Failed to find suitable memory type.");
    }

    private void InitPhysicalDevice()
    {
        uint physicalDeviceCount = 0;
        Vk.EnumeratePhysicalDevices(Instance, &physicalDeviceCount, null);

        if (physicalDeviceCount == 0)
        {
            throw new BackendException("No physical devices found.");
        }

        VkPhysicalDevice[] physicalDevices = new VkPhysicalDevice[physicalDeviceCount];
        Vk.EnumeratePhysicalDevices(Instance, &physicalDeviceCount, physicalDevices.AsPointer());

        PhysicalDevice = GetBestPhysicalDevice(physicalDevices);
        Capabilities.Init(Vk, PhysicalDevice);
    }

    private VkPhysicalDevice GetBestPhysicalDevice(VkPhysicalDevice[] physicalDevices)
    {
        VkPhysicalDevice bestPhysicalDevice = physicalDevices[0];

        foreach (VkPhysicalDevice physicalDevice in physicalDevices)
        {
            uint score = GetPhysicalDeviceScore(physicalDevice);
            uint bestScore = GetPhysicalDeviceScore(bestPhysicalDevice);

            if (score > bestScore)
            {
                bestPhysicalDevice = physicalDevice;
            }
        }

        return bestPhysicalDevice;
    }

    private uint GetPhysicalDeviceScore(VkPhysicalDevice physicalDevice)
    {
        PhysicalDeviceFeatures features;
        Vk.GetPhysicalDeviceFeatures(physicalDevice, &features);

        PhysicalDeviceProperties properties;
        Vk.GetPhysicalDeviceProperties(physicalDevice, &properties);

        uint score = 0;

        if (features.GeometryShader)
        {
            score += 1000;
        }

        if (features.TessellationShader)
        {
            score += 1000;
        }

        if (features.ShaderInt16)
        {
            score += 1000;
        }

        if (features.ShaderInt64)
        {
            score += 1000;
        }

        if (features.ShaderFloat64)
        {
            score += 1000;
        }

        if (features.SparseBinding)
        {
            score += 1000;
        }

        if (features.SparseResidencyBuffer)
        {
            score += 1000;
        }

        if (features.SparseResidencyImage2D)
        {
            score += 1000;
        }

        if (features.SparseResidencyImage3D)
        {
            score += 1000;
        }

        if (features.SparseResidency2Samples)
        {
            score += 1000;
        }

        if (features.SparseResidency4Samples)
        {
            score += 1000;
        }

        if (features.SparseResidency8Samples)
        {
            score += 1000;
        }

        if (features.SparseResidency16Samples)
        {
            score += 1000;
        }

        if (features.SparseResidencyAliased)
        {
            score += 1000;
        }

        if (features.VariableMultisampleRate)
        {
            score += 1000;
        }

        if (features.InheritedQueries)
        {
            score += 1000;
        }

        score += properties.ApiVersion;

        if (properties.DeviceType is PhysicalDeviceType.IntegratedGpu)
        {
            score += 4000;
        }

        if (properties.DeviceType is PhysicalDeviceType.DiscreteGpu)
        {
            score += 6000;
        }

        if (properties.DeviceType is PhysicalDeviceType.VirtualGpu)
        {
            score += 2000;
        }

        if (properties.DeviceType is PhysicalDeviceType.Cpu)
        {
            score += 1000;
        }

        return score;
    }
}
