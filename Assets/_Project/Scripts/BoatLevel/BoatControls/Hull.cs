using Fusion;
using System.Xml.Linq;
using UnityEngine;
using static Unity.Collections.Unicode;

public class Hull : NetworkBehaviour
{
    public WaveSample Waves;

    private Buoy[] _buoys;
    private Rigidbody _body;
    private float _time;
    public Transform Keel;
    public float KeelArea = 4;
    public bool UseLocalTime = false;

    void Start()
    {
        _buoys = GetComponentsInChildren<Buoy>();
        _body = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (UseLocalTime)
            _time = Time.fixedTime;
        if (Object != null)
        {
            _time = Runner.SimulationTime;
            UseLocalTime = false;
        }

        if (Object != null && !HasStateAuthority)
            return;

        int applied = 0;
        float totalUp = 0f;
        foreach (var buoy in _buoys)
        {
            if (buoy.ComputeAndApplyForce(_body, Waves, _buoys.Length, _time, out var force))
            {
                applied++;
                totalUp += force.y;
            }
        }

        if (Keel)
            UpdateKeelDrag(_body, _time);
    }

    public void UpdateKeelDrag(Rigidbody body, float time)
    {
        var keelOffset = Keel.localPosition;
        var sternPosition = Keel.position;
        float waveHeight = Waves.GetHeight(sternPosition, time);

        if (waveHeight >= sternPosition.y)
        {
            AddDragForce(transform, body, keelOffset, transform.right, KeelArea);
        }
    }

    public static void AddDragForce(Transform t, Rigidbody b, Vector3 offset, Vector3 direction, float dragArea)
    {
        var offsetPosition = t.TransformPoint(offset);
        var velocity = b.GetRelativePointVelocity(offset);
        var drag = -Vector3.Dot(velocity, direction);
        var force = direction * drag * dragArea;
        b.AddForceAtPosition(force, offsetPosition);
    }
}