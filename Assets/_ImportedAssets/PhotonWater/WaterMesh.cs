using System;
using UnityEngine;
using Fusion;

public class WaterMesh : MonoBehaviour
{
    public bool CircularMesh = false;
    public int Side = 32;
    public float Extent = 0.5f;

    public bool UseLocalTime = false;

    private MeshFilter _filter;
    private Renderer _renderer;
    private int _offsetIndex;
    private int _timeIndex;
    private float _time;

    private Vector3 _desiredPosition;

    public WaveSample Waves;
    public NetworkRunner NetworkRunner;

    public void SetPosition(Vector3 position)
    {
        _desiredPosition = position;
    }

    public void SetTime(float time)
    {
        _time = time;
        UseLocalTime = false;
    }

    private void Update()
    {
        //if (NetworkRunner.IsRunning)
        //{
        //    SetTime(NetworkRunner.LocalRenderTime);
        //}
    }
    private void Start()
    {
        _desiredPosition = transform.position;
        _filter = GetComponent<MeshFilter>();
        GeneratePlaneMesh(Side, Extent, _filter.mesh);
        _renderer = GetComponent<Renderer>();

        _offsetIndex = Shader.PropertyToID("_Offset");
        _timeIndex = Shader.PropertyToID("_RenderTime");

        if (Waves)
        {
            var source = Waves.WaveSources[0];
            _renderer.material.SetVector("_A", new Vector3(source.Wavelength, source.Speed, source.Amplitude));
            source = Waves.WaveSources[1];
            _renderer.material.SetVector("_B", new Vector3(source.Wavelength, source.Speed, source.Amplitude));
            source = Waves.WaveSources[2];
            _renderer.material.SetVector("_C", new Vector3(source.Wavelength, source.Speed, source.Amplitude));
            Debug.Log("init waves");
        }
    }

    void LateUpdate()
    {
        var current = transform.position;
        int size = (int)(Extent * 2);
        float x = (int)_desiredPosition.x;
        if (x % size != 0)
            x = current.x;
        float z = (int)_desiredPosition.z;
        if (z % size != 0)
            z = current.z;
        _renderer.material.SetVector(_offsetIndex, new Vector4(x, z, 0, 0));
        transform.position = new Vector3(x, 0, z);

        if (UseLocalTime)
            _time = Time.time;
        _renderer.material.SetFloat(_timeIndex, _time);
    }
    void GeneratePlaneMesh(int count, float extent, Mesh mesh)
    {
        var half = count / 2;
        var size = extent * 2;
        Vector3[] vertices = new Vector3[count * count * 4 * 3];
        Vector3[] normals = new Vector3[count * count * 4 * 3];
        int[] tris = new int[count * count * 4 * 3];
        Vector2[] UVs = new Vector2[count * count * 4 * 3];
        int vertexIndex = 0;
        for (int i = half; i > -half; i--)
        {
            for (int j = half; j > -half; j--)
            {
                var a = new Vector3(j * size - extent, 0, i * size + extent);
                var b = new Vector3(j * size + extent, 0, i * size + extent);
                var c = new Vector3(j * size - extent, 0, i * size - extent);
                var d = new Vector3(j * size + extent, 0, i * size - extent);
                var center = new Vector3(j * size, 0, i * size);

                if (center.magnitude > half * size && CircularMesh)
                    continue;

                CreateTri(a, b, center, ref vertexIndex, vertices, UVs);
                CreateTri(a, center, c, ref vertexIndex, vertices, UVs);
                CreateTri(b, d, center, ref vertexIndex, vertices, UVs);
                CreateTri(center, d, c, ref vertexIndex, vertices, UVs);
            }
        }

        Debug.LogFormat("Generated {0} triangles", (vertexIndex / 3));
        if (CircularMesh)
        {
            Array.Resize(ref vertices, vertexIndex);
            Array.Resize(ref normals, vertexIndex);
            Array.Resize(ref UVs, vertexIndex);
            Array.Resize(ref tris, vertexIndex);
        }

        // tris are sequential (low poly look)
        for (int t = 0; t < tris.Length; t++)
        {
            tris[t] = t;
        }

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, UVs);
        mesh.triangles = tris;
        // normals are computed here
        mesh.RecalculateNormals();
    }

    void CreateTri(Vector3 a, Vector3 b, Vector3 c, ref int index, Vector3[] vertices, Vector2[] UVs)
    {
        var center = (a + b + c) / 3;
        var uv = new Vector2(center.x, center.z);
        UVs[index] = uv;
        vertices[index++] = a;
        UVs[index] = uv;
        vertices[index++] = b;
        UVs[index] = uv;
        vertices[index++] = c;
    }

}
