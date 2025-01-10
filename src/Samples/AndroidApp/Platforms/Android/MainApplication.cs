using Android.App;
using Android.Runtime;

#pragma warning disable IDE0130
namespace Tests.AndroidApp;
#pragma warning restore IDE0130

[Application]
public class MainApplication(nint handle, JniHandleOwnership ownership) : MauiApplication(handle, ownership)
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
