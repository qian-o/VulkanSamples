using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AndroidApp.ViewModels;

public partial class SampleViewModel : ObservableRecipient
{
    [RelayCommand]
    private static void ShowSettings()
    {
    }
}
