using UnityEngine;

[AddComponentMenu("Player/MouseLook")]
public class MouseLook : MonoBehaviour
{
    [Header("Mouse Look")]
    public Transform cameraTransform;         // assign the Camera (child of player)
    public float mouseSensitivity = 500f;     // horizontal & vertical sensitivity multiplier
    public float pitchMin = -85f;             // look down limit
    public float pitchMax = 85f;              // look up limit
    public bool lockCursor = true;

    float pitch = 0f; // rotation around X (camera up/down)

    void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        // read mouse delta
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // yaw: rotate the player object around Y
        transform.Rotate(Vector3.up, mouseX);

        // pitch: rotate the camera around local X (clamped)
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        if (cameraTransform != null)
            cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }
}
