using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Tests.WPF;

public class GammaCorrectionEffect : ShaderEffect
{
    public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty(nameof(Input), typeof(GammaCorrectionEffect), 0);

    public GammaCorrectionEffect()
    {
        PixelShader pixelShader = new()
        {
            UriSource = new Uri("/Tests.WPF;component/Assets/Shaders/GammaCorrectionEffect.ps", UriKind.Relative)
        };

        PixelShader = pixelShader;

        UpdateShaderValue(InputProperty);
    }

    public Brush Input
    {
        get
        {
            return (Brush)GetValue(InputProperty);
        }
        set
        {
            SetValue(InputProperty, value);
        }
    }
}
