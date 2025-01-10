namespace Graphics.Core;

public abstract class DisposableObject : IDisposable
{
    private volatile uint _isDisposed;

    ~DisposableObject()
    {
        Dispose();
    }

    public bool IsDisposed => _isDisposed != 0;

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
        {
            return;
        }

        Destroy();

        GC.SuppressFinalize(this);
    }

    protected abstract void Destroy();
}
