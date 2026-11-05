#ifndef VF_CUSTOM_SHADERGRAPH_FUNCTIONS
#define VF_CUSTOM_SHADERGRAPH_FUNCTIONS
#define EG_PI 3.14159265

// WAVES
// float3(WaveLength, Speed, Amplitude)

void CustomVertex_float(float3 vertex, float2 vertexCenter, float3 a, float3 b, float3 c, float2 offset, float time, out float3 displacedVertex, out float3 normal, out float3 tangent) {
    float3 p = vertex;
    float3 pos = p;
				
    pos.x += offset.x;
    pos.z += offset.y;
   
    const float kA = 2 * EG_PI / a.x;
    const float kB = 2 * EG_PI / b.x;
    const float kC = 2 * EG_PI / c.x;
   
    float f = kA * (pos.x + pos.z - a.y * time);
    p.y = a.z * sin(f);
   
    f = kB * (pos.x - b.y * time);
    p.y += b.z * sin(f);
   
    f = kC * (pos.z - c.y * time);
    p.y += c.z * sin(f);
   
   
    float2 uv = vertexCenter;
    uv.x += offset.x;
    uv.y += offset.y;
   
    f = kA * (uv.x + uv.y - a.y * time);
    float height = kA * a.z * cos(f);
    f = kB * (uv.x  - b.y * time);
    height += kB * b.z * cos(f);
    f = kC * (uv.y - c.y * time);
    height += kC * c.z * cos(f);
   
    tangent = normalize(float3(1 - height, height, 0));
    normal = float3(-tangent.y, tangent.x, 0);
	displacedVertex = p;
}

#endif