using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerMoving playerMoving;
    [SerializeField] private Transform cam;

    [Header("Rotation")]
    [SerializeField] private float snapSpeed = 25f;
    [SerializeField] private float turnSpeed = 8f;

    private Vector3 normal;
    private float moveX;
    private float moveY;

    private Vector3 moveDirection;
    private Vector3 desiredForward = Vector3.forward;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (playerMoving == null) playerMoving = GetComponent<PlayerMoving>();
    }

    public void RotationManager()
    {
        ReadPlayerData();
        CalculateMoveDirectionFromCamera();

        Vector3 groundNormal = GetGroundNormal();

        //
        // SURFACE SNAP
        //

        Quaternion snappedRotation =
            Quaternion.FromToRotation(rb.transform.up, groundNormal) * rb.rotation;

        float snapT = 1f - Mathf.Exp(-snapSpeed * Time.fixedDeltaTime);

        Quaternion groundAlignedRotation =
            Quaternion.Slerp(rb.rotation, snappedRotation, snapT);

        //
        // MOVEMENT TURN
        //

        Quaternion finalRotation = groundAlignedRotation;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            desiredForward =
                Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized;

            Quaternion moveRotation =
                Quaternion.LookRotation(desiredForward, groundNormal);

            float turnT = 1f - Mathf.Exp(-turnSpeed * Time.fixedDeltaTime);

            finalRotation =
                Quaternion.Slerp(groundAlignedRotation, moveRotation, turnT);
        }

        rb.MoveRotation(finalRotation);

        playerMoving.moveDirection = moveDirection;
        playerMoving.desiredForward = desiredForward;
    }

    private void ReadPlayerData()
    {
        normal = playerMoving.normal;
        moveX = playerMoving.moveX;
        moveY = playerMoving.moveY;
    }

    private Vector3 GetGroundNormal()
    {
        return normal.sqrMagnitude > 0.001f ? normal : Vector3.up;
    }

    private void CalculateMoveDirectionFromCamera()
    {
        Vector3 groundNormal = GetGroundNormal();

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
}
