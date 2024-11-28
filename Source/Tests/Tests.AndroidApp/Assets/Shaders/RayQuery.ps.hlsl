#include "RayQuery.hlsl"

bool isInShadow(inout uint randSeed, float3 worldPosition)
{
    float3 coneSample = getConeSample(randSeed, worldPosition, lights[0].position, 0.2);
    
    RayQuery < RAY_FLAG_ACCEPT_FIRST_HIT_AND_END_SEARCH > rayQuery;
    ProceedRayQuery(rayQuery, worldPosition, coneSample, camera.nearPlane, camera.farPlane);
    
    return rayQuery.CommittedStatus() == COMMITTED_TRIANGLE_HIT;
}

bool isAO(inout uint randSeed, float3 worldPosition, float3 N)
{
    float3 aoDir = getCosHemisphereSample(randSeed, N);
    
    RayQuery < RAY_FLAG_ACCEPT_FIRST_HIT_AND_END_SEARCH > rayQuery;
    ProceedRayQuery(rayQuery, worldPosition, aoDir, 0.01, 1.2);
    
    return rayQuery.CommittedStatus() == COMMITTED_TRIANGLE_HIT;
}

float4 main(VSOutput input) : SV_TARGET
{
    GeometryNode node = geometryNodes[input.NodeIndex];

    float4 texColor =
        textureArray[node.baseColorTextureIndex].Sample(samplerArray[0], input.TexCoord) * float4(input.Color, 1.0);

    if (node.alphaMask && texColor.a < node.alphaCutoff)
    {
        discard;
    }

    float3 N = normalize(input.Normal);
    float3 L = normalize(lights[0].position - input.WorldPosition);
    float3 V = normalize(camera.position - input.WorldPosition);
    float3 R = normalize(-reflect(L, N));
    
    uint2 launchDim = uint2(param.width, param.height);
    uint2 launchIndex = input.Position.xy;

    uint randSeed = initRand(launchIndex.x + launchIndex.y * launchDim.x, 10, 16);
    
    bool shadow = isInShadow(randSeed, input.WorldPosition.xyz);
    bool ao = isAO(randSeed, input.WorldPosition.xyz, N);
    
    float3 diffuse = max(dot(N, L), 0.1).rrr * texColor.rgb * lights[0].diffuseColor.rgb;
    float3 specular = pow(max(dot(R, V), 0.0), 32.0);
    
    if (shadow)
    {
        diffuse *= 0.5;
        specular = 0.0;
    }
    
    if (ao)
    {
        diffuse *= 0.5;
    }
    
    float4 ambientColor = lights[0].ambientColor;
    float4 ambientColorMin = lights[0].ambientColor - 0.1;
    float4 ambientColorMax = lights[0].ambientColor;
    float a = 1 - saturate(dot(N, float3(0, -1, 0)));
    ambientColor = texColor * lerp(ambientColorMin, ambientColorMax, a);

    return float4(diffuse + specular + ambientColor.rgb, texColor.a);
}
