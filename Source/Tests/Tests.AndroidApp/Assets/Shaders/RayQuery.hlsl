uint initRand(uint val0, uint val1, uint backoff = 16)
{
    uint v0 = val0, v1 = val1, s0 = 0;

    [unroll]
    for (uint n = 0; n < backoff; n++)
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

template <
uint Flags>
void ProceedRayQuery(RayQuery< Flags> rayQuery, float3 origin, float3 direction, float tmin, float tmax)
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
