using Silk.NET.Maths;

namespace Graphics.Windowing.Structs;

public struct Display(int index,
                      string name,
                      Vector2D<int> mainPosition,
                      Vector2D<int> mainSize,
                      Vector2D<int> workPosition,
                      Vector2D<int> workSize,
                      float dpiScale)
{
    public int Index { get; set; } = index;

    public string Name { get; set; } = name;

    public Vector2D<int> MainPosition { get; set; } = mainPosition;

    public Vector2D<int> MainSize { get; set; } = mainSize;

    public Vector2D<int> WorkPosition { get; set; } = workPosition;

    public Vector2D<int> WorkSize { get; set; } = workSize;

    public float DpiScale { get; set; } = dpiScale;
}
