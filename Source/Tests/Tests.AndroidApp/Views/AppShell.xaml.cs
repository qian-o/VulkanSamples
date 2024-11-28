using Tests.AndroidApp.ViewModels;

namespace Tests.AndroidApp.Views;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        BindingContext = new AppShellViewModel();
    }
}
