struct CBO
{
    float4x4 Model;
    float4x4 View;
    float4x4 Projection;
};

struct Properties
{
    float PxRange;
};

struct VSInput
{
    [[vk::location(0)]] float3 Position : POSITION0;
    [[vk::location(1)]] float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    [[vk::location(0)]] float2 TexCoord : TEXCOORD;
};

ConstantBuffer<CBO> cbo : register(b0, space0);
ConstantBuffer<Properties> properties : register(b1, space0);
Texture2D msdf : register(t2, space0);
SamplerState msdfSampler : register(s3, space0);

VSOutput mainVS(VSInput input)
{
    VSOutput output;
    
    output.Position = mul(cbo.Projection, mul(cbo.View, mul(cbo.Model, float4(input.Position, 1.0))));
    output.TexCoord = input.TexCoord;
    
    return output;
}

float median(float a, float b, float c)
{
    return max(min(a, b), min(max(a, b), c));
}

float screenPxRange(float2 texCoord)
{
    uint width, height, numberOfLevels;
    msdf.GetDimensions(0, width, height, numberOfLevels);
    
    float2 unitRange = properties.PxRange / float2(width, height);
    float2 screenTexSize = 1.0 / fwidth(texCoord);
    
    return max(0.5 * dot(unitRange, screenTexSize), 1.0);
}

float4 mainPS(VSOutput input) : SV_TARGET
{
    float4 sdf = msdf.Sample(msdfSampler, input.TexCoord);
    
    float sd = median(sdf.r, sdf.g, sdf.b);
    float screenPxDistance = screenPxRange(input.TexCoord) * (sd - 0.5);
    float opacity = saturate(screenPxDistance + 0.5);
    
    float4 bgColor = float4(0.0, 0.0, 0.0, 0.0);
    float4 fgColor = float4(1.0, 1.0, 1.0, 1.0);
    
    return lerp(bgColor, fgColor, opacity);
}