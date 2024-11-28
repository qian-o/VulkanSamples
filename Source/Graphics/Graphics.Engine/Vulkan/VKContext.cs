using Graphics.Core.Helpers;
using Graphics.Engine.Enums;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Core;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe partial class VKContext : Context
{
    public VKContext()
    {
        Backend = Backend.Vulkan;
        Capabilities = new VKDeviceCapabilities();
        Factory = new VKResourceFactory(this);

        Vk = Vk.GetApi();
        Version = Vk.Version13;
    }

    public override Backend Backend { get; }

    public override VKDeviceCapabilities Capabilities { get; }

    public override ResourceFactory Factory { get; }

    public Vk Vk { get; }

    public Version32 Version { get; }

    public VkInstance Instance { get; private set; }

    public VKDebug? Debug { get; private set; }

    public KhrSurface KhrSurface { get; private set; } = null!;

    public override void CreateDevice(bool useValidationLayers = false)
    {
        if (Instance.Handle != 0)
        {
            return;
        }

        InitInstance(useValidationLayers);
        InitPhysicalDevice();
        InitDevice();
    }

    public void SetDebugName(ObjectType objectType, ulong handle, string name)
    {
        Debug?.SetObjectName(Device, objectType, handle, name);
    }

    protected override void Destroy()
    {
        if (Instance.Handle == 0)
        {
            return;
        }

        DestroyDevice();

        KhrSurface.Dispose();
        Debug?.Dispose();
        Vk.DestroyInstance(Instance, null);

        Vk.Dispose();
    }

    private void InitInstance(bool useValidationLayers)
    {
        using Allocator allocator = new();

        ApplicationInfo applicationInfo = new()
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = allocator.Alloc("Graphics"),
            ApplicationVersion = new Version32(1, 0, 0),
            PEngineName = allocator.Alloc("Graphics Engine"),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = Version
        };

        InstanceCreateInfo createInfo = new()
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &applicationInfo
        };

        if (useValidationLayers)
        {
            const string ValidationLayerName = "VK_LAYER_KHRONOS_validation";

            uint layerCount = 0;
            Vk.EnumerateInstanceLayerProperties(&layerCount, null);

            LayerProperties[] availableLayers = new LayerProperties[(int)layerCount];
            Vk.EnumerateInstanceLayerProperties(&layerCount, availableLayers);

            bool layerFound = false;

            for (int i = 0; i < layerCount; i++)
            {
                LayerProperties layer = availableLayers[i];

                if (Allocator.GetString(layer.LayerName) == ValidationLayerName)
                {
                    layerFound = true;

                    break;
                }
            }

            if (!layerFound)
            {
                throw new NotSupportedException($"{ValidationLayerName} validation layer not supported");
            }

            createInfo.EnabledLayerCount = 1;
            createInfo.PpEnabledLayerNames = allocator.Alloc([ValidationLayerName]);
        }

        string[] extensions = GetInstanceExtensions();

        if (useValidationLayers)
        {
            extensions = [.. extensions, .. VKDebug.ExtensionNames];
        }

        createInfo.EnabledExtensionCount = (uint)extensions.Length;
        createInfo.PpEnabledExtensionNames = allocator.Alloc(extensions);

        VkInstance instance;
        Vk.CreateInstance(&createInfo, null, &instance).ThrowCode();

        Instance = instance;
        Debug = useValidationLayers ? new VKDebug(this) : null;
        KhrSurface = Vk.GetExtension<KhrSurface>(Instance);
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

        return extensions;
    }
}
