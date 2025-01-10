using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Activity;
using AndroidX.Core.View;

#pragma warning disable IDE0130
namespace Tests.AndroidApp;
#pragma warning restore IDE0130

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ScreenOrientation = ScreenOrientation.UserLandscape, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        EdgeToEdge.Enable(this);

        base.OnCreate(savedInstanceState);

        if (Window != null)
        {
            if (Window.Attributes != null)
            {
                Window.Attributes.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
            }

            WindowInsetsControllerCompat windowInsetsController = WindowCompat.GetInsetsController(Window, Window.DecorView);

            windowInsetsController.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;

            windowInsetsController.Hide(WindowInsetsCompat.Type.StatusBars());
            windowInsetsController.Hide(WindowInsetsCompat.Type.NavigationBars());
        }
    }
}
