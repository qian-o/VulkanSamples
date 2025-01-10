using CommunityToolkit.Mvvm.ComponentModel;

namespace Tests.AndroidApp.ViewModels;

public partial class AppShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool rayQuerySupported = App.PhysicalDevice.RayQuerySupported;
}
