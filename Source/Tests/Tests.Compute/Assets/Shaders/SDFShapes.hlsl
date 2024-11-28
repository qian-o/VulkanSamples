float sdBox(float3 p, float3 b)
{
    float3 d = abs(p) - b;
    return min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0));
}

float sdSphere(float3 pos)
{
    return length(pos) - 1.0;
}

float sdPlane(float3 p, float3 n, float h)
{
    return dot(p, n) + h;
}

float checkersGradBox(float2 p)
{
    float2 w = 0.001;

    float2 i = 2.0 * (abs(frac((p - 0.5 * w) * 0.5) - 0.5) - abs(frac((p + 0.5 * w) * 0.5) - 0.5)) / w;

    return 0.5 - 0.5 * i.x * i.y;
}