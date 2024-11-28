using CommunityToolkit.Maui.Behaviors;

namespace Tests.AndroidApp.Controls;

public class MenuButton : ImageButton
{
    public MenuButton()
    {
        WidthRequest = 30;
        HeightRequest = 30;
        CornerRadius = 4;
        Padding = new Thickness(2, 0);
        Opacity = 0.6;

        IconTintColorBehavior iconTintColorBehavior = new();
        iconTintColorBehavior.SetAppThemeColor(IconTintColorBehavior.TintColorProperty,
                                              (Color)Application.Current!.Resources["Black"],
                                              (Color)Application.Current!.Resources["White"]);

        Behaviors.Add(iconTintColorBehavior);

        this.SetAppThemeColor(BackgroundColorProperty,
                             (Color)Application.Current!.Resources["Gray200"],
                             (Color)Application.Current!.Resources["Gray600"]);
    }
}
