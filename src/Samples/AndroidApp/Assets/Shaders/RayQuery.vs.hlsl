#include "RayQuery.hlsl"

VSOutput main(VSInput input)
{
    GeometryNode node = geometryNodes[input.NodeIndex];

    float4 worldPosition = mul(node.transform, float4(input.Position, 1.0));

    VSOutput output;
    output.Position = mul(camera.projection, mul(camera.view, worldPosition));
    output.WorldPosition = worldPosition.xyz;
    output.Normal = normalize(mul(node.transform, float4(input.Normal, 0.0)).xyz);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    output.Tangent = input.Tangent;
    output.NodeIndex = input.NodeIndex;

    return output;
}
