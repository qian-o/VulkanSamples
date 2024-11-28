using Graphics.Engine.Descriptions;
using System.Collections;

namespace Graphics.Engine;

public class InputLayouts : IEnumerable<LayoutDesc>
{
    private readonly List<LayoutDesc> layouts = [];

    public LayoutDesc this[int index] { get => layouts[index]; set => layouts[index] = value; }

    public int Count => layouts.Count;

    public InputLayouts Add(LayoutDesc item)
    {
        layouts.Add(item);

        return this;
    }

    public IEnumerator<LayoutDesc> GetEnumerator()
    {
        return layouts.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
