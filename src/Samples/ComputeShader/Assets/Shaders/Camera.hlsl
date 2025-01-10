struct Camera
{
    float3 position;
    
    float3 forward;
    
    float3 right;
    
    float3 up;
    
    float nearPlane;
    
    float farPlane;
    
    float fov;
    
    int width;
    
    int height;
    
    float3 background;
    
    int antiAliasing;
    
    int maxSteps;
    
    float epsilon;
};
