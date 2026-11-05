using Fusion;
using UnityEngine;

public class PlayerLook : NetworkBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform cameraPivot;      
    [SerializeField] private Transform cameraTransform;  
    [SerializeField] private float sensitivity = 0.2f;
    [SerializeField] private float maxPitch = 70f;
    [SerializeField] private float minPitch = -30f;

    [Header("Camera Distance")]
    [SerializeField] private float cameraDistance = 5f; 
    [SerializeField] private float cameraSmoothTime = 0.05f;
    [SerializeField] private LayerMask collisionMask;

    private PlayerInputHandler input;
    private float pitch;
    private float yaw;

    private float currentDistance;
    private float distanceVelocity;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        currentDistance = cameraDistance;
    }

    public override void Spawned()
    {
        if (!HasInputAuthority)
        {
            if (cameraPivot != null) cameraPivot.gameObject.SetActive(false);
            if (cameraTransform != null) cameraTransform.gameObject.SetActive(false);
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform == null)
        {
            var childCam = GetComponentInChildren<Camera>();
            if (childCam != null) cameraTransform = childCam.transform;
        }
    }

    private void Update()
    {
        if (!HasInputAuthority) return;

        yaw += input.Look.x * sensitivity;
        pitch -= input.Look.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    public override void Render()
    {
        if (!HasInputAuthority || cameraTransform == null) return;

        Vector3 desiredPosition = cameraPivot.TransformPoint(new Vector3(0, 0, -cameraDistance));
        Vector3 direction = (desiredPosition - cameraPivot.position).normalized;

        float targetDistance = cameraDistance;

        if (Physics.SphereCast(cameraPivot.position, 0.2f, direction, out RaycastHit hit, cameraDistance, collisionMask))
        {
            targetDistance = Mathf.Clamp(hit.distance - 0.1f, 0.5f, cameraDistance);
        }

        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref distanceVelocity, cameraSmoothTime);

        cameraTransform.position = cameraPivot.position + direction * currentDistance;
    }

    public float Yaw => yaw;
}