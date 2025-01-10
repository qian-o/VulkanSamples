#include "GLTF.hlsl"

float4 main(VSOutput input) : SV_TARGET
{
    float4 color = textureMap[input.ColorMapIndex].Sample(textureSampler[0], input.TexCoord) * float4(input.Color, 1.0);
    
    if (AlphaMask && color.a < AlphaCutoff)
        discard;
    
    float3 N = normalize(input.Normal);
    float3 T = normalize(input.Tangent.xyz);
    float3 B = cross(input.Normal, input.Tangent.xyz) * input.Tangent.w;
    float3x3 TBN = float3x3(T, B, N);
    N = mul(normalize(textureMap[input.NormalMapIndex].Sample(textureSampler[1], input.TexCoord).xyz * 2.0 - float3(1.0, 1.0, 1.0)), TBN);
    
    const float ambientStrength = 0.1;
    float3 L = normalize(input.LightVec);
    float3 V = normalize(input.ViewVec);
    float3 R = reflect(-L, N);
    float3 diffuse = max(dot(N, L), ambientStrength).rrr;
    float3 specular = pow(max(dot(R, V), 0.0), 32.0);
    
    return float4((color.rgb + ambientStrength) * diffuse + specular, color.a);
}