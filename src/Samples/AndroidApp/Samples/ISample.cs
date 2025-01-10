using Vulkan;
using AndroidApp.Controls;

namespace AndroidApp.Samples;

public interface ISample
{
    void Load(Swapchain swapchain, CameraController camera);

    void Update(Swapchain swapchain, float width, float height, CameraController camera, float deltaTime, float totalTime);

    void Render(CommandList commandList, Swapchain swapchain, float deltaTime, float totalTime);

    void Unload();
}
