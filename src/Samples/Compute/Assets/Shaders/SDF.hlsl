#include "Camera.hlsl"
#include "Maths.hlsl"
#include "SDFShapes.hlsl"

ConstantBuffer<Camera> camera : register(b0, space0);
RWTexture2D<float4> outputTexture : register(u1, space0);

float2 map(float3 pos)
{
    float2 result = float2(1.0, -1.0);

    float3 position = pos - float3(0.0, 2.0, 0.0);
    result = opU(result, float2(sdSphere(position), 2.0));

    position = pos - float3(-1.2, 3.0, 0.0);
    result = opU(result, float2(sdBox(position, 0.8), 3.0));

    position = pos - float3(1.2, 1.0, 0.0);
    result = opU(result, float2(sdBox(position, 0.8), 4.0));

    position = pos - float3(-1.2, 1.0, 0.0);
    result = opU(result, float2(sdBox(position, 0.8), 5.0));

    position = pos - float3(1.2, 3.0, 0.0);
    result = opU(result, float2(sdBox(position, 0.8), 6.0));

    return result;
}

float3 calcNormal(float3 pos)
{
    float2 e = float2(1.0, -1.0) * 0.5773 * 0.0005;
    return normalize(e.xyy * map(pos + e.xyy).x + e.yyx * map(pos + e.yyx).x + e.yxy * map(pos + e.yxy).x +
                     e.xxx * map(pos + e.xxx).x);
}

float calcAO(float3 pos, float3 nor)
{
    float occ = 0.0;
    float sca = 1.0;

    for (int i = 0; i < 5; i++)
    {
        float h = 0.01 + 0.12 * float(i) / 4.0;
        float d = map(pos + h * nor).x;
        occ += (h - d) * sca;
        sca *= 0.95;
        if (occ > 0.35)
            break;
    }

    return clamp(1.0 - 3.0 * occ, 0.0, 1.0) * (0.5 + 0.5 * nor.y);
}

float calcSoftshadow(float3 ro, float3 rd, float mint, float tmax)
{
    float tp = (0.8 - ro.y) / rd.y;
    if (tp > 0.0)
        tmax = min(tmax, tp);

    float res = 1.0;
    float t = mint;
    for (int i = 0; i < 24; i++)
    {
        float h = map(ro + rd * t).x;
        float s = clamp(8.0 * h / t, 0.0, 1.0);
        res = min(res, s);
        t += clamp(h, 0.01, 0.2);
        if (res < 0.004 || t > tmax)
            break;
    }
    res = clamp(res, 0.0, 1.0);
    return res * res * (3.0 - 2.0 * res);
}

float2 raycast(float3 position, float3 direction)
{
    float2 res = float2(-1.0, -1.0);
    
    float tmin = camera.nearPlane;
    float tmax = camera.farPlane;
    
    float tp1 = (0.0 - position.y) / direction.y;
    if (tp1 > 0.0)
    {
        tmax = min(tmax, tp1);

        res = float2(tp1, 1.0);
    }
    
    for (int i = 0; i < camera.maxSteps && tmin < tmax; i++)
    {
        if (tmin > tmax)
        {
            break;
        }

        float2 hit = map(position + tmin * direction);

        if (hit.x < camera.epsilon)
        {
            res = float2(tmin, hit.y);

            break;
        }

        tmin += hit.x;
    }

    return res;
};

float3 render(float2 pixel)
{
    float x = pixel.x;
    float y = pixel.y;
    
    float scale = tan(camera.fov);
    float aspectRatio = (float) camera.width / camera.height;
    
    x = rangeMap(x, 0, camera.width - 1, -1.0, 1.0);
    y = rangeMap(y, 0, camera.height - 1, 1.0, -1.0);

    if (aspectRatio > 1.0)
    {
        x *= aspectRatio * scale;
        y *= scale;
    }
    else
    {
        x *= scale;
        y *= scale / aspectRatio;
    }
    
    float3 position = camera.position;
    float3 direction = normalize(camera.forward + x * camera.right + y * camera.up);
    
    float2 result = raycast(position, direction);
    
    if (result.y < 0)
    {
        return camera.background;
    }
    
    float3 pos = position + result.x * direction;
    float3 nor = result.y == 1.0 ? float3(0.0, 1.0, 0.0) : calcNormal(pos);
    float3 ref = reflect(direction, nor);
    float occ = calcAO(pos, nor);
    float ks = 1.0;

    float3 color = 0.0;
    float3 lin = 0.0;

    if (result.y == 1.0)
    {
        float f = checkersGradBox(pos.xz);

        color = 0.15 + f * 0.05;
        ks = 0.4;
    }
    else
    {
        color = 0.2 + 0.2 * sin(result.y * 2.0 + float3(0.0, 1.0, 2.0));
    }

    float3 lights[2] =
    {
        float3(5.0, 5.0, 5.0),
        float3(-10.0, 5.0, 5.0)
    };

    for (int i = 0; i < 2; i++)
    {
        float3 lightPosition = lights[i];

        float3 light = normalize(lightPosition);

        lin += color * 0.8;

        float spec = pow(clamp(dot(ref, light), 0.0, 1.0), 32.0);

        lin += spec * 0.5;

        float shadow = calcSoftshadow(pos, light, 0.02, 2.5);

        lin *= shadow;
    }

    // sky
    {
        float dif = sqrt(clamp(0.5 + 0.5 * nor.y, 0.0, 1.0));
        dif *= occ;
        float spe = smoothstep(-0.2, 0.2, ref.y);
        spe *= dif;
        spe *= 0.04 + 0.96 * pow(clamp(1.0 + dot(nor, direction), 0.0, 1.0), 5.0);
        spe *= calcSoftshadow(pos, ref, 0.02, 2.5);
        lin += color * 0.60 * dif * float3(0.40, 0.60, 1.15);
        lin += 2.00 * spe * float3(0.40, 0.60, 1.30) * ks;
    }

    // back
    {
        float dif = clamp(dot(nor, normalize(float3(0.5, 0.0, 0.6))), 0.0, 1.0) * clamp(1.0 - pos.y, 0.0, 1.0);
        dif *= occ;

        lin += color * 0.55 * dif * float3(0.25, 0.25, 0.25);
    }

    color = lin;

    return lerp(color, camera.background, 1.0 - exp(-0.0001 * result.x * result.x * result.x));
};

[numthreads(32, 32, 1)]
void main(uint3 id : SV_DispatchThreadID)
{
    if (id.x >= camera.width || id.y >= camera.height)
    {
        return;
    }
    
    float3 color = 0.0;
    
    float halfAA = camera.antiAliasing / 2.0;
    
    for (uint i = 0; i < camera.antiAliasing; i++)
    {
        for (uint j = 0; j < camera.antiAliasing; j++)
        {
            float2 offset = float2(i, j) / halfAA;
            
            float2 pixel = float2(id.x, id.y) + offset;
            
            color += render(pixel);
        }
    }
    
    color /= float(camera.antiAliasing * camera.antiAliasing);
    
    outputTexture[id.xy] = float4(color, 1);
}