using Graphics.Engine.Enums;
using Silk.NET.Maths;

namespace Graphics.Engine;

public abstract class CommandBuffer(Context context) : DeviceResource(context)
{
    /// <summary>
    /// Sets the initial state for this command buffer. This function must be called
    /// before other graphics commands can be issued.
    /// </summary>
    public abstract void Begin();

    /// <summary>
    /// Completes the command buffer.
    /// </summary>
    public void End()
    {
        ClearCache();
        EndInternal();
    }

    /// <summary>
    /// Resets the command buffer to the initial state.
    /// </summary>
    public abstract void Reset();

    /// <summary>
    /// Commits this command buffer to the command queue.
    /// </summary>
    public abstract void Commit();

    /// <summary>
    /// Updates the data in a buffer.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="source"></param>
    /// <param name="sourceSizeInBytes"></param>
    /// <param name="destinationOffsetInBytes"></param>
    public abstract void UpdateBufferData(Buffer buffer,
                                          nint source,
                                          uint sourceSizeInBytes,
                                          uint destinationOffsetInBytes = 0);

    /// <summary>
    /// Copies data from one buffer to another.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="sourceSizeInBytes"></param>
    /// <param name="destinationOffsetInBytes"></param>
    public abstract void CopyBuffer(Buffer source,
                                    Buffer destination,
                                    uint sourceSizeInBytes,
                                    uint destinationOffsetInBytes = 0);

    /// <summary>
    /// Updates the data in a texture.
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="source"></param>
    /// <param name="sourceSizeInBytes"></param>
    /// <param name="region"></param>
    public abstract void UpdateTextureData(Texture texture,
                                           nint source,
                                           uint sourceSizeInBytes,
                                           TextureRegion region);

    /// <summary>
    /// Copies data from a buffer to a texture.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="sourceX"></param>
    /// <param name="sourceY"></param>
    /// <param name="sourceZ"></param>
    /// <param name="sourceMipLevel"></param>
    /// <param name="sourceBaseFace"></param>
    /// <param name="destination"></param>
    /// <param name="destinationX"></param>
    /// <param name="destinationY"></param>
    /// <param name="destinationZ"></param>
    /// <param name="destinationMipLevel"></param>
    /// <param name="destinationBaseFace"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="depth"></param>
    /// <param name="faceCount"></param>
    public abstract void CopyTexture(Texture source,
                                     TextureRegion sourceRegion,
                                     Texture destination,
                                     TextureRegion destinationRegion);

    /// <summary>
    /// Resolve a multisampled texture into a non-multisampled texture.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="sourcePosition"></param>
    /// <param name="destination"></param>
    /// <param name="destinationPosition"></param>
    public abstract void ResolveTexture(Texture source,
                                        TexturePosition sourcePosition,
                                        Texture destination,
                                        TexturePosition destinationPosition);

    /// <summary>
    /// Begin rendering to a specific frame buffer.
    /// </summary>
    /// <param name="frameBuffer"></param>
    /// <param name="clearValue"></param>
    public abstract void BeginRendering(FrameBuffer frameBuffer, ClearValue clearValue);

    /// <summary>
    /// End rendering to a specific frame buffer.
    /// </summary>
    public abstract void EndRendering();

    /// <summary>
    /// Sets a viewport in a specific slot.
    /// </summary>
    /// <param name="viewport"></param>
    public abstract void SetViewports(Viewport[] viewports);

    /// <summary>
    /// Sets a scissor rectangle in a specific slot.
    /// </summary>
    /// <param name="scissors"></param>
    public abstract void SetScissorRectangles(Rectangle<int>[] scissors);

    /// <summary>
    /// Sets full viewports for this command buffer.
    /// </summary>
    /// <param name="minDepth"></param>
    /// <param name="maxDepth"></param>
    public abstract void SetFullViewports(float minDepth = 0, float maxDepth = 1);

    /// <summary>
    /// Sets full scissor rectangles for this command buffer.
    /// </summary>
    public abstract void SetFullScissorRectangles();

    /// <summary>
    /// Sets the graphics pipeline state object for this command buffer.
    /// </summary>
    /// <param name="pipeline"></param>
    public abstract void SetGraphicsPipeline(GraphicsPipeline pipeline);

    /// <summary>
    /// Sets buffers to the input-assembler stage.
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    public abstract void SetVertexBuffer(uint slot, Buffer buffer, uint offset = 0);

    /// <summary>
    /// Sets an array of index buffers to the input-assembler stage.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="format"></param>
    /// <param name="offset"></param>
    public abstract void SetIndexBuffer(Buffer buffer,
                                        IndexFormat format = IndexFormat.U16Bit,
                                        uint offset = 0);

    /// <summary>
    /// Sets the active Evergine.Common.Graphics.ResourceSet for the given index.
    /// </summary>
    /// <param name="resourceSet"></param>
    /// <param name="index"></param>
    /// <param name="constantBufferOffsets"></param>
    public abstract void SetResourceSet(ResourceSet resourceSet,
                                        uint index = 0,
                                        uint[]? constantBufferOffsets = null);

    /// <summary>
    /// Draws non-indexed, instanced primitives.
    /// </summary>
    /// <param name="vertexCountPerInstance"></param>
    /// <param name="instanceCount"></param>
    /// <param name="startVertexLocation"></param>
    /// <param name="startInstanceLocation"></param>
    public abstract void DrawInstanced(uint vertexCountPerInstance,
                                       uint instanceCount,
                                       uint startVertexLocation = 0,
                                       uint startInstanceLocation = 0);

    /// <summary>
    /// Draws instanced, GPU-generated primitives.
    /// </summary>
    /// <param name="argBuffer"></param>
    /// <param name="offset"></param>
    /// <param name="drawCount"></param>
    /// <param name="stride"></param>
    public abstract void DrawInstancedIndirect(Buffer argBuffer,
                                               uint offset,
                                               uint drawCount,
                                               uint stride);

    /// <summary>
    /// Draws indexed, non-instanced primitives.
    /// </summary>
    /// <param name="indexCount"></param>
    /// <param name="startIndexLocation"></param>
    /// <param name="baseVertexLocation"></param>
    public abstract void DrawIndexed(uint indexCount,
                                     uint startIndexLocation = 0,
                                     uint baseVertexLocation = 0);

    /// <summary>
    /// Draws indexed, instanced primitives.
    /// </summary>
    /// <param name="indexCountPerInstance"></param>
    /// <param name="instanceCount"></param>
    /// <param name="startIndexLocation"></param>
    /// <param name="baseVertexLocation"></param>
    /// <param name="startInstanceLocation"></param>
    public abstract void DrawIndexedInstanced(uint indexCountPerInstance,
                                              uint instanceCount,
                                              uint startIndexLocation = 0,
                                              uint baseVertexLocation = 0,
                                              uint startInstanceLocation = 0);

    /// <summary>
    /// Draw indexed, instanced, GPU-generated primitives.
    /// </summary>
    /// <param name="argBuffer"></param>
    /// <param name="offset"></param>
    /// <param name="drawCount"></param>
    /// <param name="stride"></param>
    public abstract void DrawIndexedInstancedIndirect(Buffer argBuffer,
                                                      uint offset,
                                                      uint drawCount,
                                                      uint stride);

    /// <summary>
    /// Execute commands in a compute shader from a thread group.
    /// </summary>
    /// <param name="groupCountX"></param>
    /// <param name="groupCountY"></param>
    /// <param name="groupCountZ"></param>
    public abstract void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ);

    /// <summary>
    /// Clears all cached values of this command buffer.
    /// </summary>
    protected abstract void ClearCache();

    /// <summary>
    /// Finalizes the command buffer.
    /// </summary>
    protected abstract void EndInternal();
}
