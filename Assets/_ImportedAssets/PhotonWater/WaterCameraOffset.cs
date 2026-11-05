using System;
using UnityEngine;

public class WaterCameraOffset : MonoBehaviour
{
    public float LookDistance = 10;
    private Transform _cameraTransform;
    private WaterMesh _waterMesh;
    private void Start()
    {
        _cameraTransform = Camera.main.transform;
        _waterMesh = GetComponent<WaterMesh>();
    }

    private void Update()
    {
        var position = _cameraTransform.TransformPoint(Vector3.forward * LookDistance);
        position.y = 0;
        _waterMesh.SetPosition(position);
    }
}
