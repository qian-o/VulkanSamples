using Graphics.Core;
using Graphics.Core.Helpers;
using Graphics.Vulkan.Helpers;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphics.Vulkan;

public unsafe class Context : DisposableObject
{
    private const string ValidationLayerName = "VK_LAYER_KHRONOS_validation";

    private readonly Allocator _alloter;
    private readonly Vk _vk;
    private readonly VkInstance _instance;
    private readonly KhrSurface _surface;
    private readonly VulkanDebug? _vkDebug;
    private readonly Dictionary<PhysicalDevice, VulkanResources> _physicalDeviceMap;
    private readonly Dictionary<GraphicsDevice, VulkanResources> _graphicsDeviceMap;

    static Context()
    {
        ApiVersion = Vk.Version13;

#if DEBUG
        Debugging = true;
#else
        Debugging = false;
#endif
    }

    public Context()
    {
        _alloter = new();
        _vk = Vk.GetApi();

        _instance = CreateInstance();

        _surface = CreateInstanceExtension<KhrSurface>();

        _vkDebug = Debugging ? new(_vk, _instance) : null;

        _physicalDeviceMap = [];
        _graphicsDeviceMap = [];

        _alloter.Clear();
    }

    public static Version32 ApiVersion { get; }

    public static bool Debugging { get; }

    public PhysicalDevice[] EnumeratePhysicalDevices()
    {
        uint physicalDeviceCount = 0;
        _vk.EnumeratePhysicalDevices(_instance, &physicalDeviceCount, null).ThrowCode("Failed to enumerate physical devices!");

        if (physicalDeviceCount == 0)
        {
            throw new InvalidOperationException("No physical devices found!");
        }

        VkPhysicalDevice[] physicalDevices = new VkPhysicalDevice[(int)physicalDeviceCount];
        _vk.EnumeratePhysicalDevices(_instance, &physicalDeviceCount, physicalDevices).ThrowCode("Failed to enumerate physical devices!");

        return physicalDevices.Select(Create).ToArray();

        PhysicalDevice Create(VkPhysicalDevice vkPhysicalDevice)
        {
            lock (_physicalDeviceMap)
            {
                VulkanResources vulkanResources = new();
                vulkanResources.InitializeContext(_vk, _instance, GetInstanceExtensions(), _surface, _vkDebug);

                PhysicalDevice physicalDevice = new(vulkanResources, vkPhysicalDevice);

                _physicalDeviceMap.Add(physicalDevice, vulkanResources);

                return physicalDevice;
            }
        }
    }

    public PhysicalDevice GetBestPhysicalDevice()
    {
        PhysicalDevice[] physicalDevices = EnumeratePhysicalDevices();

        return physicalDevices.OrderByDescending(item => item.Score).First();
    }

    public GraphicsDevice CreateGraphicsDevice(PhysicalDevice physicalDevice)
    {
        float queuePriority = 1.0f;

        uint graphicsQueueFamilyIndex = physicalDevice.GetQueueFamilyIndex(QueueFlags.GraphicsBit | QueueFlags.ComputeBit);
        uint computeQueueFamilyIndex = physicalDevice.GetQueueFamilyIndex(QueueFlags.ComputeBit);
        uint transferQueueFamilyIndex = physicalDevice.GetQueueFamilyIndex(QueueFlags.TransferBit);

        HashSet<uint> queueFamilyIndices =
        [
            graphicsQueueFamilyIndex,
            computeQueueFamilyIndex,
            transferQueueFamilyIndex
        ];

        DeviceQueueCreateInfo* createInfos = _alloter.Alloc<DeviceQueueCreateInfo>(queueFamilyIndices.Count);

        for (int i = 0; i < queueFamilyIndices.Count; i++)
        {
            createInfos[i] = new DeviceQueueCreateInfo
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = queueFamilyIndices.ElementAt(i),
                QueueCount = 1,
                PQueuePriorities = &queuePriority
            };
        }

        string[] extensions = GetDeviceExtensions(physicalDevice);

        DeviceCreateInfo createInfo = new()
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = (uint)queueFamilyIndices.Count,
            PQueueCreateInfos = createInfos,
            EnabledExtensionCount = (uint)extensions.Length,
            PpEnabledExtensionNames = _alloter.Alloc(extensions)
        };

        createInfo.AddNext(out PhysicalDeviceFeatures2 features2)
                  .AddNext(out PhysicalDeviceVulkan13Features _)
                  .AddNext(out PhysicalDeviceScalarBlockLayoutFeatures _)
                  .AddNext(out PhysicalDeviceDescriptorIndexingFeatures _)
                  .AddNext(out PhysicalDeviceBufferDeviceAddressFeatures _);

        if (physicalDevice.DescriptorBufferSupported)
        {
            createInfo.AddNext(out PhysicalDeviceDescriptorBufferFeaturesEXT _);
        }

        if (physicalDevice.RayQuerySupported)
        {
            createInfo.AddNext(out PhysicalDeviceRayQueryFeaturesKHR _);
        }

        if (physicalDevice.RayTracingSupported)
        {
            createInfo.AddNext(out PhysicalDeviceRayTracingPipelineFeaturesKHR _);
        }

        if (physicalDevice.RayQuerySupported || physicalDevice.RayTracingSupported)
        {
            createInfo.AddNext(out PhysicalDeviceAccelerationStructureFeaturesKHR _);
        }

        _vk.GetPhysicalDeviceFeatures2(physicalDevice.Handle, &features2);

        VkDevice device;
        _vk.CreateDevice(physicalDevice.Handle, &createInfo, null, &device).ThrowCode("Failed to create device.");

        return Create(device);

        GraphicsDevice Create(VkDevice vkDevice)
        {
            lock (_graphicsDeviceMap)
            {
                VulkanResources vulkanResources = new();
                vulkanResources.InitializeContext(_vk, _instance, GetInstanceExtensions(), _surface, _vkDebug);
                vulkanResources.InitializePhysicalDevice(physicalDevice, GetDeviceExtensions(physicalDevice));

                GraphicsDevice graphicsDevice = new(vulkanResources,
                                                    vkDevice,
                                                    graphicsQueueFamilyIndex,
                                                    computeQueueFamilyIndex,
                                                    transferQueueFamilyIndex);

                _graphicsDeviceMap.Add(graphicsDevice, vulkanResources);

                return graphicsDevice;
            }
        }
    }

    protected override void Destroy()
    {
        foreach (KeyValuePair<PhysicalDevice, VulkanResources> pair in _physicalDeviceMap)
        {
            pair.Value.Dispose();
        }
        foreach (KeyValuePair<GraphicsDevice, VulkanResources> pair in _graphicsDeviceMap)
        {
            pair.Value.Dispose();
        }

        _physicalDeviceMap.Clear();
        _graphicsDeviceMap.Clear();

        _vkDebug?.Dispose();

        _surface.Dispose();

        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();

        _alloter.Dispose();
    }

    /// <summary>
    /// Check validation layer support.
    /// </summary>
    /// <returns></returns>
    private bool CheckValidationLayerSupport()
    {
        uint layerCount = 0;
        _vk.EnumerateInstanceLayerProperties(&layerCount, null);

        LayerProperties[] availableLayers = new LayerProperties[(int)layerCount];
        _vk.EnumerateInstanceLayerProperties(&layerCount, availableLayers);

        for (int i = 0; i < layerCount; i++)
        {
            LayerProperties layer = availableLayers[i];

            if (Allocator.GetString(layer.LayerName) == ValidationLayerName)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Create instance.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">InvalidOperationException</exception>
    private VkInstance CreateInstance()
    {
        ApplicationInfo applicationInfo = new()
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = _alloter.Alloc(AppDomain.CurrentDomain.FriendlyName),
            ApplicationVersion = new Version32(1, 0, 0),
            PEngineName = _alloter.Alloc("Graphics"),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = ApiVersion
        };

        InstanceCreateInfo createInfo = new()
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &applicationInfo
        };

        if (Debugging && !CheckValidationLayerSupport())
        {
            throw new InvalidOperationException("Validation layers requested, but not available!");
        }

        if (Debugging)
        {
            createInfo.EnabledLayerCount = 1;
            createInfo.PpEnabledLayerNames = _alloter.Alloc([ValidationLayerName]);
        }

        string[] extensions = GetInstanceExtensions();

        createInfo.EnabledExtensionCount = (uint)extensions.Length;
        createInfo.PpEnabledExtensionNames = _alloter.Alloc(extensions);

        VkInstance instance;
        _vk.CreateInstance(&createInfo, null, &instance).ThrowCode("Failed to create instance!");

        return instance;
    }

    /// <summary>
    /// Create instance extension.
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">InvalidOperationException</exception>
    private T CreateInstanceExtension<T>() where T : NativeExtension<Vk>
    {
        if (!_vk.TryGetInstanceExtension(_instance, out T ext))
        {
            throw new InvalidOperationException($"Failed to load extension {typeof(T).Name}!");
        }

        return ext;
    }

    private static string[] GetInstanceExtensions()
    {
        string[] extensions = [KhrSurface.ExtensionName];

        if (OperatingSystem.IsWindows())
        {
            extensions = [.. extensions, KhrWin32Surface.ExtensionName];
        }
        else if (OperatingSystem.IsLinux())
        {
            extensions = [.. extensions, KhrXlibSurface.ExtensionName];
        }
        else if (OperatingSystem.IsAndroid())
        {
            extensions = [.. extensions, KhrAndroidSurface.ExtensionName];
        }

        if (Debugging)
        {
            extensions = [.. extensions, VulkanDebug.ExtensionName];
        }

        return extensions;
    }

    private static string[] GetDeviceExtensions(PhysicalDevice physicalDevice)
    {
        string[] extensions = [KhrSwapchain.ExtensionName];

        if (OperatingSystem.IsWindows())
        {
            extensions = [.. extensions, KhrExternalMemoryWin32.ExtensionName];
        }

        if (physicalDevice.DescriptorBufferSupported)
        {
            extensions = [.. extensions, ExtDescriptorBuffer.ExtensionName];
        }

        if (physicalDevice.RayQuerySupported)
        {
            extensions = [.. extensions, KhrRayQuery.ExtensionName];
        }

        if (physicalDevice.RayTracingSupported)
        {
            extensions = [.. extensions, KhrRayTracingPipeline.ExtensionName];
        }

        if (physicalDevice.RayQuerySupported || physicalDevice.RayTracingSupported)
        {
            extensions = [.. extensions, KhrAccelerationStructure.ExtensionName, KhrDeferredHostOperations.ExtensionName];
        }

        return extensions;
    }
}
