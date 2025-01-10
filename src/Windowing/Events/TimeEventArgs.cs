namespace Graphics.Windowing.Events;

public class TimeEventArgs(double deltaTime, double totalTime) : EventArgs
{
    public double DeltaTime { get; } = deltaTime;

    public double TotalTime { get; } = totalTime;
}
