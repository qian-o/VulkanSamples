using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Tests.AndroidApp.Controls;
using Tests.AndroidApp.Handlers;

namespace Tests.AndroidApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder()
                                        .UseMauiApp<App>()
                                        .UseSkiaSharp()
                                        .UseMauiCommunityToolkit()
                                        .ConfigureFonts(fonts =>
                                        {
                                            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                                            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                                        })
                                        .ConfigureMauiHandlers(handlers =>
                                        {
                                            handlers.AddHandler<SwapChainPanel, SwapChainPanelHandler>();
                                        });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
