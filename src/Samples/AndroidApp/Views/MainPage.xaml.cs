using Tests.AndroidApp.Controls;
using Tests.AndroidApp.ViewModels;

namespace Tests.AndroidApp.Views;

public partial class MainPage : ShellPage
{
    public MainPage()
    {
        InitializeComponent();

        BindingContext = new MainViewModel();
    }
}

