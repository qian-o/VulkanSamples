[[vk::constant_id(0)]] const bool AlphaMask = false;
[[vk::constant_id(1)]] const float AlphaCutoff = 0.0;

struct CBO
{
    float4x4 Projection;
    float4x4 View;
    float4x4 Model;
    float4 LightPos;
    float4 ViewPos;
};

struct VSInput
{
    [[vk::location(0)]] float3 Position : POSITION0;
    [[vk::location(1)]] float3 Normal : NORMAL0;
    [[vk::location(2)]] float2 TexCoord : TEXCOORD0;
    [[vk::location(3)]] float3 Color : COLOR0;
    [[vk::location(4)]] float4 Tangent : TEXCOORD1;
    [[vk::location(5)]] int ColorMapIndex : TEXCOORD2;
    [[vk::location(6)]] int NormalMapIndex : TEXCOORD3;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    [[vk::location(0)]] float3 Normal : NORMAL0;
    [[vk::location(1)]] float2 TexCoord : TEXCOORD;
    [[vk::location(2)]] float3 Color : COLOR0;
    [[vk::location(3)]] float3 ViewVec : TEXCOORD1;
    [[vk::location(4)]] float3 LightVec : TEXCOORD2;
    [[vk::location(5)]] float4 Tangent : TEXCOORD3;
    [[vk::location(6)]] nointerpolation int ColorMapIndex : TEXCOORD4;
    [[vk::location(7)]] nointerpolation int NormalMapIndex : TEXCOORD5;
};

ConstantBuffer<CBO> cbo : register(b0, space0);
Texture2D textureMap[] : register(t0, space1);
SamplerState textureSampler[] : register(s0, space2);