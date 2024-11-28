#include "GLTF.hlsl"

VSOutput main(VSInput input)
{
    float4 position = mul(cbo.Model, float4(input.Position, 1.0));
    
    VSOutput output;
    
    output.Position = mul(cbo.Projection, mul(cbo.View, position));
    output.Normal = normalize(mul(cbo.Model, float4(input.Normal, 0.0)).xyz);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    output.ViewVec = cbo.ViewPos.xyz - position.xyz;
    output.LightVec = cbo.LightPos.xyz - position.xyz;
    output.Tangent = input.Tangent;
    output.ColorMapIndex = input.ColorMapIndex;
    output.NormalMapIndex = input.NormalMapIndex;
    
    return output;
}