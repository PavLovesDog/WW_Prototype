using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // The target the camera will follow
    public float smoothSpeed = 0.125f; // Adjust this value to change how quickly the camera follows the target
    public Vector3 offset; // Offset from the target's position

    void Update()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Optionally, if you want the camera to always look at the target, uncomment the line below
        // transform.LookAt(target);
    }
}
