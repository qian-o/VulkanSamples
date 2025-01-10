# Vulkan Samples

This repository is a C# Vulkan sample library, which includes GLTF model loading, 
ImGui drawing, ray tracing, WPF integration, Android integration, etc.

## Features
- Factory pattern to create Vulkan resources.
- Abstract Vulkan objects to simplify Vulkan API calls.
- Support DXC to compile HLSL code.
- Acceleration structure support.
- Compute and ray tracing pipelines.
- Resource sharing between DX11 and Vulkan.

## Compile and Precautions
- Ensure that `Vulkan 1.3 SDK` is installed and the graphics card driver supports `Vulkan 1.3`.
- Ensure that `Visual Studio 2022` and `.NET 9.0` are installed.
- For the `Android` project, you need to install `Android NDK` and `Android SDK`. The virtual machine does not support `Vulkan 1.3`, you need a real machine with Snapdragon 8 Gen2 or higher chip.

## Samples
|AndroidApp||||
|-|-|-|-|
|||||

|ComputeShader|
|-|
||

|GLTFAnimation|
|-|
||

|GLTFScene|
|-|
||

|MultiViewports|
|-|
||

|RayQuery|
|-|
||

|RayTracing|
|-|
||

|SDFFontTexture|
|-|
||

|Skia|
|-|
||

|WPF|
|-|
||

## Reference
- [veldrid](https://github.com/veldrid/veldrid)
- [Evergine](https://evergine.com)
