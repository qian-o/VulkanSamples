using Microsoft.Maui.Handlers;
using AndroidSwapChainPanel = AndroidApp.Platforms.Android.Controls.SwapChainPanel;
using SwapChainPanel = AndroidApp.Controls.SwapChainPanel;

namespace AndroidApp.Handlers;

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
