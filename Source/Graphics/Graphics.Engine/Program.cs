using Graphics.Core.Helpers;
using Graphics.Engine;
using Graphics.Engine.Descriptions;
using Graphics.Engine.Enums;
using Graphics.Engine.Helpers;
using Graphics.Windowing;
using Graphics.Windowing.Events;
using Silk.NET.Maths;
using StbImageSharp;
using System.Numerics;

// 1. Use Dynamic Rendering instead of RenderPass.
// 2. Reduce unnecessary assignment operations.
// 3. Use slang instead of HLSL.
// 4. Silk.NET 3.0 use VkStruct == NULL.
// 5. Use Silk.NET.Maths instead of System.Numerics.
// 6. Remove TextureView.

using Context context = Context.Create(Backend.Vulkan);

context.CreateDevice(true);

using CommandProcessor commandProcessor = context.Factory.CreateCommandProcessor();

SwapChain swapChain = null!;
Texture texture1 = null!;
Texture texture2 = null!;

SdlWindow window = new();
window.Loaded += Loaded;
window.Update += Update;
window.Render += Render;
window.SizeChanged += SizeChanged;
window.Unloaded += Unloaded;

window.Show();

WindowManager.Loop();

swapChain.Dispose();

unsafe void Loaded(object? sender, EventArgs e)
{
    SwapChainDesc swapChainDesc = SwapChainDesc.Default(window.Surface);
    swapChain = context.Factory.CreateSwapChain(in swapChainDesc);

    using FileStream fileStream = File.OpenRead(@"C:\Users\13247\Desktop\硝子.png");

    ImageResult imageResult = ImageResult.FromStream(fileStream, ColorComponents.RedGreenBlueAlpha);

    TextureDesc textureDesc1 = TextureDesc.Default2D((uint)imageResult.Width, (uint)imageResult.Height, 1);
    texture1 = context.Factory.CreateTexture(in textureDesc1);
    texture1.Name = "Texture1";

    TextureDesc textureDesc2 = TextureDesc.Default2D((uint)imageResult.Width,
                                                     (uint)imageResult.Height,
                                                     Utils.GetMipLevels((uint)imageResult.Width, (uint)imageResult.Height));
    texture2 = context.Factory.CreateTexture(in textureDesc2);
    texture2.Name = "Texture2";

    context.UpdateTextureData(texture1,
                              (nint)imageResult.Data.AsPointer(),
                              (uint)imageResult.Data.Length,
                              TextureRegion.Default(texture1));

    Console.WriteLine("Initialization completed.");
}

void Update(object? sender, TimeEventArgs e)
{
}

void Render(object? sender, TimeEventArgs e)
{
    CommandBuffer commandBuffer = commandProcessor.CommandBuffer();

    commandBuffer.Begin();

    TextureRegion source = TextureRegion.Default(texture1);

    for (uint i = 0; i < texture2.Desc.MipLevels; i++)
    {
        commandBuffer.CopyTexture(texture1, source, texture2, TextureRegion.Default(texture2, i));
    }

    commandBuffer.BeginRendering(swapChain.FrameBuffer, ClearValue.Default(color: Vector4.UnitX));

    commandBuffer.EndRendering();
    commandBuffer.End();
    commandBuffer.Commit();

    commandProcessor.Submit();
    commandProcessor.WaitIdle();

    swapChain.Present();
}

void SizeChanged(object? sender, ValueEventArgs<Vector2D<int>> e)
{
    swapChain.Resize();
}

void Unloaded(object? sender, EventArgs e)
{
    swapChain.Dispose();
    texture1.Dispose();
    texture2.Dispose();
}