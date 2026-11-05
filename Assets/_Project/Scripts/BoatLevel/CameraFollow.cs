using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        // Drag your Boat here
    public Vector3 offset = new Vector3(0, 5, -15); // Adjust to match your view
    public float smoothTime = 0.3f; // Lower is snappier, higher is smoother

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate the desired position based on the boat's location + offset
        Vector3 targetPosition = target.position + offset;

        // Smoothly move the camera toward that position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // Optional: Keep looking at the boat
        transform.LookAt(target);
    }
}