using Microsoft.Maui.Handlers;
using Tests.AndroidApp.Controls;
using AndroidSwapChainPanel = Tests.AndroidApp.Platforms.Android.Controls.SwapChainPanel;

namespace Tests.AndroidApp.Handlers;

internal sealed class SwapChainPanelHandler : ViewHandler<SwapChainPanel, AndroidSwapChainPanel>
{
    public static PropertyMapper<SwapChainPanel, SwapChainPanelHandler> mapper = new(ViewMapper);

    public static CommandMapper<SwapChainPanel, SwapChainPanelHandler> commandMapper = new(ViewCommandMapper);

    public SwapChainPanelHandler() : base(mapper, commandMapper)
    {
    }

    protected override AndroidSwapChainPanel CreatePlatformView()
    {
        return new AndroidSwapChainPanel(Context, VirtualView);
    }
}
