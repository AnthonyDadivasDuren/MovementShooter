using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    public Transform playerCam;     // Reference to the player's camera
    public Transform orientation;   // Reference to orientation (direction player is facing)

    [Header("Look Settings")]
    public float sensitivity = 50f;     // Mouse sensitivity
    public float sensMultiplier = 1f;   // Sensitivity multiplier

    private float _xRotation;      // Current vertical rotation of the camera
    private float _desiredX;       // Desired horizontal rotation

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Look();
    }

    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        // Add to the horizontal rotation (yaw)
        _desiredX += mouseX;

        // Subtract from vertical rotation (pitch), clamp to prevent over-rotation
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        // Apply rotation to camera and orientation
        playerCam.localRotation = Quaternion.Euler(_xRotation, _desiredX, 0);
        orientation.localRotation = Quaternion.Euler(0, _desiredX, 0);
    }
}
