using Graphics.Vulkan;
using Tests.AndroidApp.Controls;

namespace Tests.AndroidApp.Samples;

public interface ISample
{
    void Load(Swapchain swapchain, CameraController camera);

    void Update(Swapchain swapchain, float width, float height, CameraController camera, float deltaTime, float totalTime);

    void Render(CommandList commandList, Swapchain swapchain, float deltaTime, float totalTime);

    void Unload();
}
