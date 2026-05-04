using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerMoving playerMoving;
    [SerializeField] private Transform cam;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Slopes")]
    [SerializeField] private float maxGroundChangeAngle = 360f;

    private Vector3 normal;
    private float moveX;
    private float moveY;

    private Vector3 moveDirection;
    private Vector3 desiredForward = Vector3.forward;

    private Quaternion snapRotation;
    private Quaternion turnRotation;
    private Quaternion smoothRotation;

    private Vector3 lastNormal = Vector3.up;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (playerMoving == null) playerMoving = GetComponent<PlayerMoving>();

        smoothRotation = rb.rotation;
        snapRotation = rb.rotation;
        turnRotation = rb.rotation;
    }

    public void RotationManager()
    {
        ReadPlayerData();
        CalculateMoveDirectionFromCamera();
        HandleRotation();
        WriteBackToPlayer();

        if (!playerMoving.grounded)
            ResetRotationInAir();
    }

    private void ReadPlayerData()
    {
        normal = playerMoving.normal;
        moveX = playerMoving.moveX;
        moveY = playerMoving.moveY;
    }

    private void WriteBackToPlayer()
    {
        playerMoving.moveDirection = moveDirection;
        playerMoving.desiredForward = desiredForward;
    }

    private void CalculateMoveDirectionFromCamera()
    {
        Vector3 groundNormal = normal.sqrMagnitude > 0.001f ? normal : Vector3.up;

        Vector3 forward = Vector3.ProjectOnPlane(cam.forward, groundNormal);
        Vector3 right = Vector3.ProjectOnPlane(cam.right, groundNormal);

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.ProjectOnPlane(Vector3.forward, groundNormal);

        if (right.sqrMagnitude < 0.001f)
            right = Vector3.ProjectOnPlane(Vector3.right, groundNormal);

        forward.Normalize();
        right.Normalize();

        Vector3 input = forward * moveY + right * moveX;
        moveDirection = input.sqrMagnitude > 0.001f ? input.normalized : Vector3.zero;
    }

    private void HandleRotation()
    {
        if (moveDirection.sqrMagnitude <= 0.001f)
            return;

        desiredForward = Vector3.ProjectOnPlane(moveDirection, normal);

        if (desiredForward.sqrMagnitude <= 0.001f)
            return;

        AlignToGround();
        RotateTowardsMovement();
        ApplySmoothRotation();

        rb.MoveRotation(smoothRotation);
    }

    private void AlignToGround()
    {
        float angle = Vector3.Angle(lastNormal, normal);

        if (angle <= maxGroundChangeAngle)
        {
            lastNormal = normal;
        }
        else
        {
            normal = lastNormal;
        }

        snapRotation = Quaternion.FromToRotation(rb.transform.up, normal) * rb.rotation;
    }

    private void RotateTowardsMovement()
    {
        turnRotation = Quaternion.LookRotation(desiredForward.normalized, normal);
    }

    private void ApplySmoothRotation()
    {
        float t = 1f - Mathf.Exp(-rotationSpeed * Time.fixedDeltaTime);

        Quaternion target = Quaternion.Slerp(snapRotation, turnRotation, 1f);
        smoothRotation = Quaternion.Slerp(smoothRotation, target, t);
    }

    private void ResetRotationInAir()
    {
        smoothRotation = rb.rotation;
        snapRotation = rb.rotation;
        turnRotation = rb.rotation;

        lastNormal = Vector3.up;
        normal = Vector3.up;

        desiredForward = Vector3.ProjectOnPlane(transform.forward, normal);

        AlignToGround();
        RotateTowardsMovement();
        ApplySmoothRotation();

        rb.MoveRotation(smoothRotation);
    }
}
