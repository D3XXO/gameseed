using UnityEngine;

public class IsometricCameraFollow : MonoBehaviour
{
    [Header("Target & Speed")]
    public Transform playerTarget;
    public float smoothSpeed = 0.125f;

    [Header("Offset from Player")]
    public Vector3 offset;

    void Start()
    {
        if (playerTarget == null)
        {
            BoatController playerBoat = FindObjectOfType<BoatController>();
            if (playerBoat != null)
            {
                playerTarget = playerBoat.transform;
            }
            else
            {
                Debug.LogError("Player (BoatController) not found for camera follow. Please assign it in the Inspector.");
            }
        }

        if (offset == Vector3.zero && playerTarget != null)
        {
            offset = transform.position - playerTarget.position;
            Debug.Log("Auto-calculated camera offset: " + offset);
        }
        else if (offset == Vector3.zero && playerTarget == null)
        {
            Debug.LogWarning("Camera offset is zero and player target is not set. Camera might not follow correctly.");
        }
    }

    void LateUpdate()
    {
        if (playerTarget == null)
        {
            BoatController playerBoat = FindObjectOfType<BoatController>();
            if (playerBoat != null)
            {
                playerTarget = playerBoat.transform;
            }
            else
            {
                return;
            }
        }

        Vector3 desiredPosition = playerTarget.position + offset;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }


}