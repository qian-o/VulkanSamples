using Graphics.Vulkan;
using Tests.AndroidApp.Controls;

namespace Tests.AndroidApp.Samples;

public abstract class BaseSample : ISample
{
    private readonly Lock updateLock = new();
    private readonly Queue<Action<object[]>> updateTasks = new();
    private readonly Queue<object[]> updateObjects = new();
    private readonly Lock renderLock = new();
    private readonly Queue<Action<CommandList, object[]>> renderTasks = new();
    private readonly Queue<object[]> renderObjects = new();

    public virtual void Load(Swapchain swapchain, CameraController camera)
    {
    }

    public virtual void Update(Swapchain swapchain, float width, float height, CameraController camera, float deltaTime, float totalTime)
    {
        if (updateTasks.TryDequeue(out Action<object[]>? task) && updateObjects.TryDequeue(out object[]? args))
        {
            task(args);
        }

        camera.Update();
    }

    public virtual void Render(CommandList commandList, Swapchain swapchain, float deltaTime, float totalTime)
    {
        if (renderTasks.TryDequeue(out Action<CommandList, object[]>? task) && renderObjects.TryDequeue(out object[]? args))
        {
            task(commandList, args);
        }
    }

    public virtual void Unload()
    {
    }

    protected void AddUpdateTask(Action<object[]> task, params object[] args)
    {
        lock (updateLock)
        {
            updateTasks.Enqueue(task);
            updateObjects.Enqueue(args);
        }
    }

    protected void AddRenderTask(Action<CommandList, object[]> task, params object[] args)
    {
        lock (renderLock)
        {
            renderTasks.Enqueue(task);
            renderObjects.Enqueue(args);
        }
    }

    protected static void AddBackgroundTask(Action<object[]> task, params object[] args)
    {
        Task.Run(() => task(args));
    }
}
