namespace Graphics.Windowing.Events;

public class ValueEventArgs<T>(T value) : EventArgs
{
    public T Value { get; } = value;
}
