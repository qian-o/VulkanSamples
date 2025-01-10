using AndroidApp.Controls;
using AndroidApp.ViewModels;

namespace AndroidApp.Views;

public partial class MainPage : ShellPage
{
    public MainPage()
    {
        InitializeComponent();

        BindingContext = new MainViewModel();
    }
}

