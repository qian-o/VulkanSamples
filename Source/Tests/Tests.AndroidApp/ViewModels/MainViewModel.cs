using CommunityToolkit.Mvvm.ComponentModel;

namespace Tests.AndroidApp.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string title = "Tests.AndroidApp";

    [ObservableProperty]
    private string deviceName = App.PhysicalDevice.Name;

    [ObservableProperty]
    private string apiVersion = $"{App.PhysicalDevice.ApiVersion.Major}.{App.PhysicalDevice.ApiVersion.Minor}.{App.PhysicalDevice.ApiVersion.Patch}";

    [ObservableProperty]
    private bool rayQuerySupported = App.PhysicalDevice.RayQuerySupported;

    [ObservableProperty]
    private bool rayTracingSupported = App.PhysicalDevice.RayTracingSupported;
}
