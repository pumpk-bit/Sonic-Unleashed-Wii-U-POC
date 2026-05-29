using UnityEngine;

public class PlayerGrounded : MonoBehaviour
{
    public enum GroundType
    {
        OldGrounded,
        NewGrounded
    }

    [Header("Mode")]
    [SerializeField] private GroundType groundType = GroundType.NewGrounded;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerMoving playerMoving;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundMask;

    [SerializeField] private float groundProbeRadius = 0.35f;
    [SerializeField] private float groundProbeDistance = 1.2f;
    [SerializeField] private float velocityProbeMultiplier = 0.05f;
    [SerializeField] private float maxGroundAngle = 50f;

    [Header("Gravity")]
    [SerializeField] private float groundedGravity = 40f;
    [SerializeField] private float airGravity = 35f;

    [Header("Ground Stick")]
    [SerializeField] private float minimumStickForce = 10f;
    [SerializeField] private float snapThreshold = 0.001f;

    [Header("Jump Protection")]
    [SerializeField] private float groundCheckDelay = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool grounded;
    [SerializeField] private Vector3 normal = Vector3.up;

    private bool canCheckIfGrounded = true;
    private float lastJumpTime;

    private void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (playerMoving == null)
            playerMoving = GetComponent<PlayerMoving>();
    }

    public void GroundedAndGravityManager()
    {
        ReadPlayerState();

        switch (groundType)
        {
            case GroundType.OldGrounded:
                OldGroundCheck();
                break;

            case GroundType.NewGrounded:
                NewGroundCheck();
                break;
        }

        ApplyGravity();

        WriteBackToPlayer();
    }

    private void ReadPlayerState()
    {
        canCheckIfGrounded = playerMoving.canCheckIfGrounded;
        lastJumpTime = playerMoving.lastJumpTime;
    }

    private void WriteBackToPlayer()
    {
        playerMoving.grounded = grounded;
        playerMoving.normal = normal;
    }

    // OLD GROUNDING

    private void OldGroundCheck()
    {
        if (!canCheckIfGrounded)
        {
            grounded = false;
            normal = Vector3.up;
            return;
        }

        if (Time.time - lastJumpTime < groundCheckDelay)
        {
            grounded = false;
            normal = Vector3.up;
            return;
        }

        RaycastHit hit;

        float speed = rb.velocity.magnitude;

        float extraDistance =speed * Time.fixedDeltaTime;

        float castDistance =groundProbeDistance + 0.05f;

        castDistance += extraDistance;

        bool hitGround = Physics.SphereCast(rb.worldCenterOfMass,groundProbeRadius,-rb.transform.up,out hit,castDistance,groundMask,QueryTriggerInteraction.Ignore);

        grounded = hitGround;

        normal = grounded ?hit.normal.normalized :Vector3.up;

        if (!grounded)
            return;

        float distanceToGround = hit.distance;

        SnapToGround(distanceToGround);

        CancelVelocityIntoGround();

        float stickForce =Mathf.Max(minimumStickForce, speed);

        rb.AddForce(-normal * stickForce,ForceMode.Acceleration);
    }

    // NEW GROUNDING

    private void NewGroundCheck()
    {
        if (!canCheckIfGrounded)
        {
            grounded = false;
            normal = Vector3.up;
            return;
        }

        if (Time.time - lastJumpTime < groundCheckDelay)
        {
            grounded = false;
            normal = Vector3.up;
            return;
        }

        float speed = rb.velocity.magnitude;

        float probeDistance =groundProbeDistance +(speed * velocityProbeMultiplier);

        RaycastHit[] hits = Physics.SphereCastAll(rb.worldCenterOfMass,groundProbeRadius,Vector3.down,probeDistance,groundMask,QueryTriggerInteraction.Ignore);

        if (hits.Length == 0)
        {
            grounded = false;
            normal = Vector3.up;
            return;
        }

        bool foundGround = false;

        RaycastHit bestHit =default(RaycastHit);

        float bestDistance =float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            if (hit.collider.attachedRigidbody == rb)
                continue;

            float slopeAngle =Vector3.Angle(hit.normal, Vector3.up);


            if (slopeAngle > maxGroundAngle)
                continue;

            if (hit.distance < bestDistance)
            {
                bestDistance = hit.distance;
                bestHit = hit;
                foundGround = true;
            }
        }

        if (!foundGround)
        {
            grounded = false;
            normal = Vector3.up;
            return;
        }

        grounded = true;

        normal =bestHit.normal.normalized;

        SnapToGround(bestHit.distance);

        CancelVelocityIntoGround();

        ApplyGroundStickForce(speed);
    }

 // SHARED FUNCTIONS

    private void SnapToGround(float distanceToGround)
    {
        float desiredDistance =groundProbeRadius;

        float snapAmount =distanceToGround - desiredDistance;

        if (snapAmount > snapThreshold)
        {
            rb.position -= normal * snapAmount;
        }
    }

    private void CancelVelocityIntoGround()
    {
        float velocityAlongNormal =Vector3.Dot(rb.velocity, normal);

        if (velocityAlongNormal > 0f)
        {
            rb.velocity -= normal * velocityAlongNormal;
        }
    }

    private void ApplyGroundStickForce(float speed)
    {
        float stickForce =Mathf.Max(minimumStickForce, speed);

        rb.AddForce(-normal * stickForce,ForceMode.Acceleration);
    }

    private void ApplyGravity()
    {
        if (grounded)
        {
            rb.velocity -=normal * groundedGravity *Time.fixedDeltaTime;
        }
        else
        {
            rb.velocity -=Vector3.up *airGravity *Time.fixedDeltaTime;
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if (rb == null)
            return;

        Gizmos.color =grounded ?Color.green :Color.red;

        Vector3 origin =rb.worldCenterOfMass;

        Vector3 end =origin +Vector3.down *groundProbeDistance;

        Gizmos.DrawWireSphere(origin,groundProbeRadius);

        Gizmos.DrawWireSphere( end,groundProbeRadius);

        Gizmos.DrawLine(origin, end);

        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(rb.worldCenterOfMass,rb.worldCenterOfMass +normal * 2f);
    }

#endif
}
