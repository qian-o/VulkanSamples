using System.Runtime.InteropServices;

namespace Graphics.Core.Helpers;

/// <summary>
/// Provides a persistent memory allocator.
/// </summary>
public unsafe class Allocator : DisposableObject
{
    private readonly object _locker = new();
    private readonly List<nint> _marshal = [];
    private readonly List<nint> _nativeMemory = [];

    public byte* Alloc(string value)
    {
        lock (_locker)
        {
            nint ptr = Marshal.StringToHGlobalAnsi(value);

            _marshal.Add(ptr);

            return (byte*)ptr;
        }
    }

    public byte** Alloc(string[] values)
    {
        lock (_locker)
        {
            byte** ptr = (byte**)NativeMemory.Alloc((uint)(nint.Size * values.Length));

            for (int i = 0; i < values.Length; i++)
            {
                ptr[i] = Alloc(values[i]);
            }

            _nativeMemory.Add((nint)ptr);

            return ptr;
        }
    }

    public T* Alloc<T>(int length = 1) where T : unmanaged
    {
        if (length < 1)
        {
            return null;
        }

        lock (_locker)
        {
            T* ptr = (T*)NativeMemory.Alloc((uint)(sizeof(T) * length));

            _nativeMemory.Add((nint)ptr);

            return ptr;
        }
    }

    public T* Alloc<T>(params T[] values) where T : unmanaged
    {
        lock (_locker)
        {
            T* ptr = (T*)NativeMemory.Alloc((uint)(sizeof(T) * values.Length));

            for (int i = 0; i < values.Length; i++)
            {
                ptr[i] = values[i];
            }

            _nativeMemory.Add((nint)ptr);

            return ptr;
        }
    }

    public void Clear()
    {
        lock (_locker)
        {
            foreach (nint ptr in _marshal)
            {
                Marshal.FreeHGlobal(ptr);
            }

            foreach (nint ptr in _nativeMemory)
            {
                NativeMemory.Free((void*)ptr);
            }

            _marshal.Clear();
            _nativeMemory.Clear();
        }
    }

    public static unsafe string GetString(byte* stringPtr)
    {
        return Marshal.PtrToStringAnsi((nint)stringPtr) ?? string.Empty;
    }

    public static unsafe string[] GetStrings(byte** stringsPtr, int count)
    {
        string[] strings = new string[count];

        for (int i = 0; i < count; i++)
        {
            strings[i] = GetString((byte*)Marshal.ReadIntPtr((nint)stringsPtr, i * nint.Size));
        }

        return strings;
    }

    protected override void Destroy()
    {
        Clear();
    }
}
