using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

internal sealed class BufferPool(Context context) : DeviceResource(context)
{
    private const uint MinBufferSize = 1024 * 4;
    private const uint MaxBufferCount = 100;

    private readonly List<Buffer> availableBuffers = [];
    private readonly List<Buffer> usedBuffers = [];

    public bool IsUsed => usedBuffers.Count > 0;

    public Buffer Buffer(uint sizeInBytes)
    {
        lock (this)
        {
            Buffer? buffer = null;

            foreach (Buffer availableBuffer in availableBuffers)
            {
                if (availableBuffer.Desc.SizeInBytes >= sizeInBytes)
                {
                    availableBuffers.Remove(availableBuffer);

                    buffer = availableBuffer;
                }
            }

            if (buffer == null)
            {
                uint size = Math.Max(MinBufferSize, sizeInBytes);

                BufferDesc desc = BufferDesc.Default(size);

                buffer = Context.Factory.CreateBuffer(in desc);
            }

            usedBuffers.Add(buffer);

            return buffer;
        }
    }

    public void Release()
    {
        lock (this)
        {
            foreach (Buffer usedBuffer in usedBuffers)
            {
                availableBuffers.Add(usedBuffer);
            }

            usedBuffers.Clear();

            if (availableBuffers.Count > MaxBufferCount)
            {
                foreach (Buffer buffer in availableBuffers.Take((int)MaxBufferCount))
                {
                    buffer.Dispose();
                }

                availableBuffers.RemoveAll(item => item.IsDisposed);
            }
        }
    }

    protected override void SetName(string name)
    {
    }

    protected override void Destroy()
    {
        Release();

        foreach (Buffer buffer in availableBuffers)
        {
            buffer.Dispose();
        }

        availableBuffers.Clear();
    }
}
