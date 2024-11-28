uint initRand(uint val0, uint val1, uint backoff = 16)
{
    uint v0 = val0, v1 = val1, s0 = 0;

    [unroll] for (uint n = 0; n < backoff; n++)
    {
        s0 += 0x9e3779b9;
        v0 += ((v1 << 4) + 0xa341316c) ^ (v1 + s0) ^ ((v1 >> 5) + 0xc8013ea4);
        v1 += ((v0 << 4) + 0xad90777d) ^ (v0 + s0) ^ ((v0 >> 5) + 0x7e95761e);
    }
    return v0;
}
// Takes our seed, updates it, and returns a pseudorandom float in [0..1]
float nextRand(inout uint s)
{
    s = (1664525u * s + 1013904223u);
    return float(s & 0x00FFFFFF) / float(0x01000000);
}

// Rotation with angle (in radians) and axis
float3x3 angleAxis3x3(float angle, float3 axis)
{
    float c, s;
    sincos(angle, s, c);

    float t = 1 - c;
    float x = axis.x;
    float y = axis.y;
    float z = axis.z;

    return float3x3(t * x * x + c, t * x * y - s * z, t * x * z + s * y, t * x * y + s * z, t * y * y + c,
                    t * y * z - s * x, t * x * z - s * y, t * y * z + s * x, t * z * z + c);
}

// Utility function to get a vector perpendicular to an input vector
//    (from "Efficient Construction of Perpendicular Vectors Without Branching")
float3 getPerpendicularVector(float3 u)
{
    float3 a = abs(u);
    uint xm = ((a.x - a.y) < 0 && (a.x - a.z) < 0) ? 1 : 0;
    uint ym = (a.y - a.z) < 0 ? (1 ^ xm) : 0;
    uint zm = 1 ^ (xm | ym);
    return cross(u, float3(xm, ym, zm));
}

float3 getConeSample(inout uint randSeed, float3 shadePosition, float3 lightPosition, float radius)
{
    float3 toLight = normalize(lightPosition - shadePosition);

    float3 perpL = getPerpendicularVector(toLight);

    float3 toLightEdge = normalize((lightPosition + perpL * radius) - shadePosition);

    float coneAngle = radius > 0 ? acos(dot(toLight, toLightEdge)) * 2.0 : 0.0;

    float cosAngle = cos(coneAngle);

    float2 randVal = float2(nextRand(randSeed), nextRand(randSeed));

    float z = randVal.x * (1.0f - cosAngle) + cosAngle;
    float phi = randVal.y * 2.0f * 3.14159265f;

    float x = sqrt(1.0f - z * z) * cos(phi);
    float y = sqrt(1.0f - z * z) * sin(phi);
    float3 north = float3(0.f, 0.f, 1.f);

    // Find the rotation axis `u` and rotation angle `rot` [1]
    float3 axis = normalize(cross(north, toLight));
    float angle = acos(dot(toLight, north));

    // Convert rotation axis and angle to 3x3 rotation matrix [2]
    float3x3 R = angleAxis3x3(angle, axis);

    return mul(R, float3(x, y, z));
}

// Get a cosine-weighted random vector centered around a specified normal direction.
float3 getCosHemisphereSample(inout uint randSeed, float3 hitNorm)
{
    // Get 2 random numbers to select our sample with
    float2 randVal = float2(nextRand(randSeed), nextRand(randSeed));

    // Cosine weighted hemisphere sample from RNG
    float3 bitangent = getPerpendicularVector(hitNorm);
    float3 tangent = cross(bitangent, hitNorm);
    float r = sqrt(randVal.x);
    float phi = 2.0f * 3.14159265f * randVal.y;

    // Get our cosine-weighted hemisphere lobe sample direction
    return tangent * (r * cos(phi).x) + bitangent * (r * sin(phi)) + hitNorm.xyz * sqrt(1 - randVal.x);
}

struct GeometryNode
{
    float4x4 transform;

    bool alphaMask;

    float alphaCutoff;

    bool doubleSided;

    float4 baseColorFactor;

    uint baseColorTextureIndex;

    uint normalTextureIndex;

    uint roughnessTextureIndex;
};

struct Camera
{
    float3 position;

    float3 forward;

    float3 right;

    float3 up;

    float nearPlane;

    float farPlane;

    float fov;

    float4x4 view;

    float4x4 projection;

    float4x4 invView;

    float4x4 invProjection;
};

struct Light
{
    float3 position;

    float4 ambientColor;

    float4 diffuseColor;
};

struct Param
{
    uint width;

    uint height;

    float2 pixelOffsets[127];

    uint samples;
};

struct VSInput
{
    [[vk::location(0)]]
    float3 Position : POSITION0;

    [[vk::location(1)]]
    float3 Normal : NORMAL0;

    [[vk::location(2)]]
    float2 TexCoord : TEXCOORD0;

    [[vk::location(3)]]
    float3 Color : COLOR0;

    [[vk::location(4)]]
    float4 Tangent : TEXCOORD1;

    [[vk::location(5)]]
    uint NodeIndex : TEXCOORD2;
};

struct VSOutput
{
    float4 Position : SV_POSITION;

    [[vk::location(0)]]
    float3 WorldPosition : POSITION0;

    [[vk::location(1)]]
    float3 Normal : NORMAL0;

    [[vk::location(2)]]
    float2 TexCoord : TEXCOORD0;

    [[vk::location(3)]]
    float3 Color : COLOR0;

    [[vk::location(4)]]
    float4 Tangent : TEXCOORD1;

    [[vk::location(5)]]
    nointerpolation uint NodeIndex : TEXCOORD2;
};

RaytracingAccelerationStructure as : register(t0, space0);
StructuredBuffer<GeometryNode> geometryNodes : register(t1, space0);
ConstantBuffer<Camera> camera : register(b2, space0);
StructuredBuffer<Light> lights : register(t3, space0);
ConstantBuffer<Param> param : register(b4, space0);
Texture2D textureArray[] : register(t0, space1);
SamplerState samplerArray[] : register(s0, space2);

VSOutput mainVS(VSInput input)
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

template <uint Flags>
void ProceedRayQuery(RayQuery<Flags> rayQuery, float3 origin, float3 direction, float tmin, float tmax)
{
    RayDesc ray;
    ray.Origin = origin;
    ray.Direction = direction;
    ray.TMin = tmin;
    ray.TMax = tmax;

    rayQuery.TraceRayInline(as, 0, 0xff, ray);

    while (rayQuery.Proceed())
    {
        uint candidateType = rayQuery.CandidateType();

        if (candidateType == CANDIDATE_NON_OPAQUE_TRIANGLE)
        {
            rayQuery.CommitNonOpaqueTriangleHit();
            
            rayQuery.Abort();
        }
    }
}

float3 ScreenToWorld(float2 screenPos, float depth, float4x4 invProjection, float4x4 invView)
{
    float2 ndcPos;
    ndcPos.x = (screenPos.x / param.width) * 2.0 - 1.0;
    ndcPos.y = 1.0 - (screenPos.y / param.height) * 2.0;

    float4 clipSpacePos = float4(ndcPos, depth, 1.0);

    float4 viewSpacePos = mul(invProjection, clipSpacePos);
    viewSpacePos /= viewSpacePos.w;

    float4 worldSpacePos = mul(invView, viewSpacePos);

    return worldSpacePos.xyz;
}

float4 mainPS(VSOutput input) : SV_TARGET
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
    float3 V = normalize(-input.WorldPosition);
    float3 R = normalize(-reflect(L, N));
    float3 diffuse = max(dot(N, L), 0.1);

    float3 finalColor = float3(0, 0, 0);

    for (uint i = 0; i < param.samples; i++)
    {
        float2 screenPos = input.Position.xy + param.pixelOffsets[i];

        float3 worldPos = ScreenToWorld(screenPos, input.Position.z, camera.invProjection, camera.invView);

        float3 tempColor = diffuse;

        uint2 launchDim = uint2(param.width, param.height);
        uint2 launchIndex = input.Position.xy;

        uint randSeed = initRand(launchIndex.x + launchIndex.y * launchDim.x, i, 16);

        float3 coneSample = getConeSample(randSeed, worldPos, lights[0].position, 0.2);

        RayQuery<RAY_FLAG_ACCEPT_FIRST_HIT_AND_END_SEARCH> rayQuery;
        ProceedRayQuery(rayQuery, worldPos, coneSample, camera.nearPlane, camera.farPlane);

        if (rayQuery.CommittedStatus() == COMMITTED_TRIANGLE_HIT)
        {
            tempColor *= 0.2;
        }

        float ambientOcclusion = 0.0f;
        for (int i = 0; i < 4; i++)
        {
            float3 aoDir = getCosHemisphereSample(randSeed, N);

            ProceedRayQuery(rayQuery, worldPos, aoDir, 0.01, 1.2);

            if (rayQuery.CommittedStatus() != COMMITTED_TRIANGLE_HIT)
            {
                ambientOcclusion += 1.0;
            }
        }

        float aoColor = ambientOcclusion / 4;
        tempColor *= aoColor;

        tempColor = lerp(tempColor, finalColor, (float)i / param.samples);

        finalColor = tempColor;
    }

    return float4(finalColor, 1);
}
