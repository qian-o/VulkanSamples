using Graphics.Windowing.Enums;

namespace Graphics.Windowing.Events;

public class KeyEventArgs(Key key, KeyModifiers modifiers) : EventArgs
{
    public Key Key { get; } = key;

    public KeyModifiers Modifiers { get; } = modifiers;
}
