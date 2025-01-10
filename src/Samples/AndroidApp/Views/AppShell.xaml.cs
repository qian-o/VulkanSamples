using AndroidApp.ViewModels;

namespace AndroidApp.Views;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        BindingContext = new AppShellViewModel();
    }
}
