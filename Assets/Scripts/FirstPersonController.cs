using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("Player/FirstPersonController")]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float strafeSpeed = 5f;   // usually same as walkSpeed
    public float acceleration = 20f; // higher = snappier
    public bool useGravity = true;
    public float gravity = -9.81f;

    [Header("References")]
    public Transform cameraTransform; // assign the camera (child)

    CharacterController cc;
    Vector3 currentVelocity;
    Vector3 verticalVelocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // Read input
        float inputX = Input.GetAxisRaw("Horizontal"); // A/D
        float inputZ = Input.GetAxisRaw("Vertical");   // W/S

        // Build movement direction relative to camera/player yaw.
        // We want forward to follow the camera's facing direction.
        Vector3 forward;
        Vector3 right;

        if (cameraTransform != null)
        {
            // Use camera's forward but keep it horizontal (y = 0)
            forward = cameraTransform.forward;
            forward.y = 0;
            forward.Normalize();

            right = cameraTransform.right;
            right.y = 0;
            right.Normalize();
        }
        else
        {
            forward = transform.forward;
            right = transform.right;
        }

        Vector3 desiredMove = forward * inputZ * walkSpeed + right * inputX * strafeSpeed;

        // Limit diagonal speed so moving both doesn't exceed max speed
        if (desiredMove.magnitude > walkSpeed)
            desiredMove = desiredMove.normalized * walkSpeed;

        // Smooth acceleration
        currentVelocity = Vector3.MoveTowards(currentVelocity, desiredMove, acceleration * Time.deltaTime);

        // simple gravity so the character stays grounded on slopes
        if (useGravity)
        {
            if (cc.isGrounded && verticalVelocity.y < 0f)
                verticalVelocity.y = -2f; // small downward force to keep grounded
            verticalVelocity.y += gravity * Time.deltaTime;
        }
        else
        {
            verticalVelocity = Vector3.zero;
        }

        Vector3 total = currentVelocity + new Vector3(0, verticalVelocity.y, 0);

        cc.Move(total * Time.deltaTime);
    }
}
