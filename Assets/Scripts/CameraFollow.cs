using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
        public Transform target;    // Object to follow
        public Vector3 offset = new(0, 0, -10f);

    [Header("Smoothing")]
        public float smoothTime = 0.25f;
        private Vector3 currentVelocity = Vector3.zero;

    void LateUpdate() {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref currentVelocity, 
            smoothTime
        );
    }
}