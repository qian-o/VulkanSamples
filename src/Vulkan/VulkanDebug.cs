using System.Globalization;
using System.Text;
using Graphics.Core;
using Graphics.Core.Helpers;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;

namespace Graphics.Vulkan;

public unsafe class VulkanDebug : DisposableObject
{
    private static readonly bool _debugUtils;
    private static readonly bool _debugReport;
    private static readonly bool _setObjectName;

    private readonly Allocator _alloter = new();
    private readonly Vk _vk;
    private readonly VkInstance _instance;
    private readonly ExtDebugUtils? _debugUtilsExt;
    private readonly ExtDebugReport? _debugReportExt;
    private readonly ExtDebugMarker? _debugMarkerExt;
    private readonly DebugUtilsMessengerEXT? _debugUtilsMessenger;
    private readonly DebugReportCallbackEXT? _debugReportCallback;

    static VulkanDebug()
    {
        Vk vk = Vk.GetApi();

        uint extensionCount = 0;
        vk.EnumerateInstanceExtensionProperties((string)null!, &extensionCount, null);

        ExtensionProperties[] availableExtensions = new ExtensionProperties[(int)extensionCount];
        vk.EnumerateInstanceExtensionProperties((string)null!, &extensionCount, availableExtensions);

        bool debugUtils = false;
        bool debugReport = false;
        bool setObjectName = false;

        foreach (ExtensionProperties extension in availableExtensions)
        {
            string name = Allocator.GetString(extension.ExtensionName);

            if (name == ExtDebugUtils.ExtensionName)
            {
                debugUtils = true;
                setObjectName = true;

                break;
            }
            else if (name == ExtDebugReport.ExtensionName)
            {
                debugReport = true;
            }
            else if (name == ExtDebugMarker.ExtensionName)
            {
                setObjectName = true;
            }
        }

        debugReport = debugReport && !debugUtils;

        _debugUtils = debugUtils;
        _debugReport = debugReport;
        _setObjectName = setObjectName;

        vk.Dispose();
    }

    public VulkanDebug(Vk vk, VkInstance instance)
    {
        _vk = vk;
        _instance = instance;

        if (_debugUtils)
        {
            _debugUtilsExt = CreateInstanceExtension<ExtDebugUtils>();

            DebugUtilsMessengerCreateInfoEXT createInfo = new()
            {
                SType = StructureType.DebugUtilsMessengerCreateInfoExt,
                MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt
                                  | DebugUtilsMessageSeverityFlagsEXT.InfoBitExt
                                  | DebugUtilsMessageSeverityFlagsEXT.WarningBitExt
                                  | DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt,
                MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt
                              | DebugUtilsMessageTypeFlagsEXT.ValidationBitExt
                              | DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt
                              | DebugUtilsMessageTypeFlagsEXT.DeviceAddressBindingBitExt,
                PfnUserCallback = (PfnDebugUtilsMessengerCallbackEXT)DebugMessageCallback
            };

            DebugUtilsMessengerEXT debugUtilsMessenger;
            _debugUtilsExt.CreateDebugUtilsMessenger(instance, &createInfo, null, &debugUtilsMessenger);

            _debugUtilsMessenger = debugUtilsMessenger;
        }

        if (_debugReport)
        {
            _debugReportExt = CreateInstanceExtension<ExtDebugReport>();

            DebugReportCallbackCreateInfoEXT createInfo = new()
            {
                SType = StructureType.DebugReportCallbackCreateInfoExt,
                Flags = DebugReportFlagsEXT.InformationBitExt
                        | DebugReportFlagsEXT.WarningBitExt
                        | DebugReportFlagsEXT.PerformanceWarningBitExt
                        | DebugReportFlagsEXT.ErrorBitExt
                        | DebugReportFlagsEXT.DebugBitExt,
                PfnCallback = (PfnDebugReportCallbackEXT)DebugMessageCallback
            };

            DebugReportCallbackEXT debugReportCallback;
            _debugReportExt.CreateDebugReportCallback(instance, &createInfo, null, &debugReportCallback);

            _debugReportCallback = debugReportCallback;
        }

        if (!_debugUtils && _setObjectName)
        {
            _debugMarkerExt = CreateInstanceExtension<ExtDebugMarker>();
        }

        T CreateInstanceExtension<T>() where T : NativeExtension<Vk>
        {
            if (!vk.TryGetInstanceExtension(instance, out T ext))
            {
                throw new InvalidOperationException($"Failed to load extension {typeof(T).Name}!");
            }

            return ext;
        }
    }

    public static string ExtensionName => _debugUtils ? ExtDebugUtils.ExtensionName : _debugReport ? ExtDebugReport.ExtensionName : string.Empty;

    public void SetObjectName<THandle>(VulkanObject<THandle> vkObject, ObjectType[] objectTypes)
    {
        if (!_setObjectName)
        {
            return;
        }

        ulong[] handles = vkObject.GetHandles();
        string[] objNames = new string[Math.Min(handles.Length, objectTypes.Length)];

        for (int i = 0; i < objNames.Length; i++)
        {
            objNames[i] = $"{vkObject.Name} ({objectTypes[i]})";
        }

        if (_debugUtilsExt != null)
        {
            for (int i = 0; i < objNames.Length; i++)
            {
                DebugUtilsObjectNameInfoEXT nameInfo = new()
                {
                    SType = StructureType.DebugUtilsObjectNameInfoExt,
                    ObjectType = objectTypes[i],
                    ObjectHandle = handles[i],
                    PObjectName = _alloter.Alloc(objNames[i])
                };

                _debugUtilsExt.SetDebugUtilsObjectName(vkObject.VkRes.VkDevice, &nameInfo);
            }
        }

        if (_debugMarkerExt != null)
        {
            for (int i = 0; i < objNames.Length; i++)
            {
                DebugMarkerObjectNameInfoEXT nameInfo = new()
                {
                    SType = StructureType.DebugMarkerObjectNameInfoExt,
                    ObjectType = (DebugReportObjectTypeEXT)objectTypes[i],
                    Object = handles[i],
                    PObjectName = _alloter.Alloc(objNames[i])
                };

                _debugMarkerExt.DebugMarkerSetObjectName(vkObject.VkRes.VkDevice, &nameInfo);
            }
        }
    }

    protected override void Destroy()
    {
        if (_debugUtilsMessenger.HasValue)
        {
            _debugUtilsExt!.DestroyDebugUtilsMessenger(_instance, _debugUtilsMessenger.Value, null);
        }

        if (_debugReportCallback.HasValue)
        {
            _debugReportExt!.DestroyDebugReportCallback(_instance, _debugReportCallback.Value, null);
        }

        _debugUtilsExt?.Dispose();
        _debugReportExt?.Dispose();
        _debugMarkerExt?.Dispose();

        _alloter.Dispose();
    }

    private uint DebugMessageCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity,
                                      DebugUtilsMessageTypeFlagsEXT messageTypes,
                                      DebugUtilsMessengerCallbackDataEXT* pCallbackData,
                                      void* pUserData)
    {
        string message = Allocator.GetString(pCallbackData->PMessage);
        string[] strings = message.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"[{messageSeverity}] [{messageTypes}]");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Name: {Allocator.GetString(pCallbackData->PMessageIdName)}");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Number: {pCallbackData->MessageIdNumber}");
        foreach (string str in strings)
        {
            stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{str}");
        }

        PrintMessage(stringBuilder.ToString(), messageSeverity switch
        {
            DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt => ConsoleColor.DarkGray,
            DebugUtilsMessageSeverityFlagsEXT.InfoBitExt => ConsoleColor.Blue,
            DebugUtilsMessageSeverityFlagsEXT.WarningBitExt => ConsoleColor.Yellow,
            DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt => ConsoleColor.Red,
            _ => Console.ForegroundColor
        });

        return Vk.False;
    }


    private uint DebugMessageCallback(uint flags,
                                      DebugReportObjectTypeEXT objectType,
                                      ulong @object,
                                      nuint location,
                                      int messageCode,
                                      byte* pLayerPrefix,
                                      byte* pMessage,
                                      void* pUserData)
    {
        string message = Allocator.GetString(pMessage);
        string[] strings = message.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"[{(DebugReportFlagsEXT)flags}] [{objectType}]");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Location: {location}");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Message Code: {messageCode}");
        stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Layer Prefix: {Allocator.GetString(pLayerPrefix)}");
        foreach (string str in strings)
        {
            stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{str}");
        }

        PrintMessage(stringBuilder.ToString(), flags switch
        {
            (uint)DebugReportFlagsEXT.InformationBitExt => ConsoleColor.Blue,
            (uint)DebugReportFlagsEXT.WarningBitExt => ConsoleColor.Yellow,
            (uint)DebugReportFlagsEXT.PerformanceWarningBitExt => ConsoleColor.DarkYellow,
            (uint)DebugReportFlagsEXT.ErrorBitExt => ConsoleColor.Red,
            (uint)DebugReportFlagsEXT.DebugBitExt => ConsoleColor.DarkGray,
            _ => Console.ForegroundColor
        });

        return Vk.False;
    }

    private static void PrintMessage(string message, ConsoleColor color)
    {
        if (OperatingSystem.IsWindows())
        {
            Console.ForegroundColor = color;
        }

        Console.WriteLine(message);

        if (OperatingSystem.IsWindows())
        {
            Console.ResetColor();
        }
    }
}
