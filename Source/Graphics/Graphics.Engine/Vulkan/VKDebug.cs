using System.Globalization;
using System.Text;
using Graphics.Core;
using Graphics.Core.Helpers;
using Graphics.Engine.Vulkan.Helpers;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;

namespace Graphics.Engine.Vulkan;

internal sealed unsafe class VKDebug : DisposableObject
{
    private static readonly bool debugUtils;
    private static readonly bool debugReport;
    private static readonly bool setObjectName;

    static VKDebug()
    {
        using Vk vk = Vk.GetApi();

        uint extensionCount = 0;
        vk.EnumerateInstanceExtensionProperties((string)null!, &extensionCount, null);

        ExtensionProperties[] availableExtensions = new ExtensionProperties[(int)extensionCount];
        vk.EnumerateInstanceExtensionProperties((string)null!, &extensionCount, availableExtensions);

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

        debugReport = !debugUtils && debugReport;
        setObjectName = !debugUtils && setObjectName;

        if (debugUtils)
        {
            ExtensionNames = [ExtDebugUtils.ExtensionName];
        }
        else
        {
            if (debugReport)
            {
                ExtensionNames = [ExtDebugReport.ExtensionName];

                if (setObjectName)
                {
                    ExtensionNames = [.. ExtensionNames, ExtDebugMarker.ExtensionName];
                }
            }
        }
    }

    private readonly Allocator allocator = new();

    private readonly VkInstance instance;
    private readonly ExtDebugUtils? extDebugUtils;
    private readonly ExtDebugReport? extDebugReport;
    private readonly ExtDebugMarker? extDebugMarker;
    private readonly DebugUtilsMessengerEXT? debugUtilsMessengerEXT;
    private readonly DebugReportCallbackEXT? debugReportCallbackEXT;

    public VKDebug(VKContext context)
    {
        instance = context.Instance;

        if (debugUtils)
        {
            extDebugUtils = context.Vk.GetExtension<ExtDebugUtils>(instance);

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
            extDebugUtils.CreateDebugUtilsMessenger(instance, &createInfo, null, &debugUtilsMessenger);

            debugUtilsMessengerEXT = debugUtilsMessenger;
        }
        else if (debugReport)
        {
            extDebugReport = context.Vk.GetExtension<ExtDebugReport>(instance);

            if (setObjectName)
            {
                extDebugMarker = context.Vk.GetExtension<ExtDebugMarker>(instance);
            }

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
            extDebugReport.CreateDebugReportCallback(instance, &createInfo, null, &debugReportCallback);

            debugReportCallbackEXT = debugReportCallback;
        }
    }

    public static string[] ExtensionNames { get; } = [];

    public void SetObjectName(VkDevice device, ObjectType objectType, ulong objectHandle, string objectName)
    {
        if (extDebugUtils != null)
        {
            DebugUtilsObjectNameInfoEXT nameInfo = new()
            {
                SType = StructureType.DebugUtilsObjectNameInfoExt,
                ObjectType = objectType,
                ObjectHandle = objectHandle,
                PObjectName = allocator.Alloc(objectName)
            };

            extDebugUtils.SetDebugUtilsObjectName(device, &nameInfo);
        }

        if (extDebugMarker != null)
        {
            DebugMarkerObjectNameInfoEXT nameInfo = new()
            {
                SType = StructureType.DebugMarkerObjectNameInfoExt,
                ObjectType = (DebugReportObjectTypeEXT)objectType,
                Object = objectHandle,
                PObjectName = allocator.Alloc(objectName)
            };

            extDebugMarker.DebugMarkerSetObjectName(device, &nameInfo);
        }
    }

    protected override void Destroy()
    {
        if (debugReportCallbackEXT.HasValue)
        {
            extDebugReport!.DestroyDebugReportCallback(instance, debugReportCallbackEXT.Value, null);
        }

        if (debugUtilsMessengerEXT.HasValue)
        {
            extDebugUtils!.DestroyDebugUtilsMessenger(instance, debugUtilsMessengerEXT.Value, null);
        }

        extDebugMarker?.Dispose();
        extDebugReport?.Dispose();
        extDebugUtils?.Dispose();

        allocator.Dispose();
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
