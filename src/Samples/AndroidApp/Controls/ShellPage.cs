using Tests.AndroidApp.Views;

namespace Tests.AndroidApp.Controls;

public abstract class ShellPage : ContentPage
{
    public ShellPage()
    {
        ControlTemplate = new ControlTemplate(() =>
        {
            MenuButton menuButton = new()
            {
                Source = "menu.svg",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(15)
            };

            menuButton.Clicked += ShellPage_Clicked;

            Grid grid =
            [
                new ContentPresenter(),
                menuButton
            ];

            return grid;
        });
    }

    private void ShellPage_Clicked(object? sender, EventArgs e)
    {
        if (Application.Current!.Windows[0].Page is AppShell appShell)
        {
            appShell.FlyoutIsPresented = true;
        }
    }
}