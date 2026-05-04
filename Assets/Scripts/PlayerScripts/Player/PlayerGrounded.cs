using UnityEngine;

public class PlayerGrounded : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerMoving playerMoving;

    [Header("Performance")]
    [SerializeField] private float groundCheckDelay = 0.2f;

    [Header("State")]
    [SerializeField] private Vector3 normal = Vector3.up;
    [SerializeField] private bool canCheckIfGrounded = true;
    [SerializeField] private bool grounded;

    [Header("Tuning")]
    [SerializeField] private float airGravity = 35f;
    [SerializeField] private float groundedGravity = 40f;
    [SerializeField] private float downhillSlopeAngle = 30f;
    [SerializeField] private float uphillSlopeAngle = 40f;
    [SerializeField] private float groundDistance = 0.9f;
    [SerializeField] private LayerMask groundMask;

    private float lastJumpTime;
    private Vector3 desiredForward = Vector3.forward;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (playerMoving == null) playerMoving = FindObjectOfType<PlayerMoving>();
    }

    public void GroundedAndGravityManager()
    {
        ReadPlayerState();
        CheckGround();
        ApplyGravity();
        WriteBackToPlayer();
    }

    private void ReadPlayerState()
    {
        canCheckIfGrounded = playerMoving.canCheckIfGrounded;
        grounded = playerMoving.grounded;
        lastJumpTime = playerMoving.lastJumpTime;
        desiredForward = playerMoving.desiredForward;
    }

    private void WriteBackToPlayer()
    {
        playerMoving.grounded = grounded;
        playerMoving.normal = normal;
    }

    private void CheckGround()
    {
        if (!canCheckIfGrounded)
            return;

        if (Time.time - lastJumpTime < groundCheckDelay)
            return;

        RaycastHit hit;
        float speed = rb.velocity.magnitude;
        float extraDistance = speed * Time.fixedDeltaTime;
        float castDistance = groundDistance + 0.05f;

        AdjustCastDistanceForSlope(ref castDistance, extraDistance);

        //bool hitGround = Physics.SphereCast(rb.worldCenterOfMass,groundDistance,Vector3.down,out hit,castDistance,groundMask,QueryTriggerInteraction.Ignore);
        bool hitGround = Physics.SphereCast(rb.worldCenterOfMass,groundDistance,-rb.transform.up,out hit, castDistance,groundMask,QueryTriggerInteraction.Ignore);

        grounded = hitGround;
        normal = grounded ? hit.normal.normalized : Vector3.up;

        if (!grounded)
            return;

        float distanceToGround = hit.distance;
        SnapToGround(distanceToGround);
        CancelVelocityIntoGround();

        float stickForce = Mathf.Max(10f, speed);
        rb.AddForce(-normal * stickForce, ForceMode.Acceleration);
    }

    private void AdjustCastDistanceForSlope(ref float castDistance, float extraDistance)
    {
        if (desiredForward.sqrMagnitude <= 0.001f)
            return;

        Vector3 moveOnSlope = Vector3.ProjectOnPlane(desiredForward, normal);
        float slopeAngle = Vector3.Angle(normal, Vector3.up);
        float uphillDot = Vector3.Dot(moveOnSlope, Vector3.up);

        bool goingUphill = uphillDot > 0.01f;
        bool goingDownhill = uphillDot < -0.01f;

        if (goingDownhill && slopeAngle >= downhillSlopeAngle)
        {
            castDistance += extraDistance;
        }

        if (goingUphill && slopeAngle >= uphillSlopeAngle)
        {
            castDistance -= 0.02f;
        }
    }

    private void SnapToGround(float distanceToGround)
    {
        float snapAmount = distanceToGround - groundDistance;

        if (snapAmount > 0.001f)
        {
            rb.position -= normal * snapAmount;
        }
    }

    private void CancelVelocityIntoGround()
    {
        float velAlongNormal = Vector3.Dot(rb.velocity, normal);

        if (velAlongNormal > 0f)
        {
            rb.velocity -= normal * velAlongNormal;
        }
    }

    private void ApplyGravity()
    {
        if (!grounded)
        {
            rb.velocity -= Vector3.up * airGravity * Time.fixedDeltaTime;
        }
        else
        {
            rb.velocity -= normal * groundedGravity * Time.fixedDeltaTime;
        }
    }
}
