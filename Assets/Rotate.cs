using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Vector3 rotationAxis; // Select in inspector
    public float speed = 1f; // Speed of rotation
    public float targetAngle = 45f; // Target angle to rotate

    private float totalRotation = 0f; // Total rotation that has been applied
    private bool isRotating = true; // Flag to control rotation

    void Update()
    {
        // Check if the object should still rotate
        if (isRotating)
        {
            float angleToRotate = speed * Time.deltaTime;

            // Check if this rotation will exceed the target angle
            if (totalRotation + angleToRotate > targetAngle)
            {
                // Adjust the angleToRotate so we don't exceed the targetAngle
                angleToRotate = targetAngle - totalRotation;
                // stop rotating after reaching the target angle
                isRotating = false;
            }

            // Apply the rotation
            transform.Rotate(rotationAxis * angleToRotate);
            // Update the total rotation
            totalRotation += angleToRotate;
        }
    }
}
