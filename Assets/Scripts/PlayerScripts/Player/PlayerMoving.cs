using System.Collections;
using UnityEngine;
using WiiU = UnityEngine.WiiU;

public class PlayerMoving : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerTransform;
    [SerializeField] public Rigidbody playerRigidbody;
    [SerializeField] public GameObject Camera;
    [SerializeField] public Newcam CameraScriptForPlayer;//Keep

    [Header("Scripts")]
    [SerializeField] private InputManagerPlayer inputManager;
    [SerializeField] private HomingAttack homingAttackManager;
    [SerializeField] private PlayerAnimatorScript animatorScript;
    [SerializeField] private PlayerRotation playerRotation;
    [SerializeField] private PlayerAbilities playerAbilities;
    [SerializeField] private PlayerGrounded playerGrounded;
    [SerializeField] public PlayerRailGrinding playerRailGrinding;

    [Header("Health and Others")]
    [SerializeField] public int lives;

    [Header("Movement")]
    [SerializeField] public float maxSpeed;
    [SerializeField] public float walkSpeed;
    [SerializeField] public float crawlSpeed;
    [SerializeField] public float deadZoneForWalk;

    [SerializeField] public float airSpeed;
    [SerializeField] public float acceleration;
    [SerializeField] private float walkDeceleration;
    [SerializeField] private float runDeceleration;

    [Header("Drag")]
    [SerializeField] public float groundDrag;
    [SerializeField] public float airDrag;

    [Header("States")]
    [SerializeField] public bool canMove = true;
    [SerializeField] public bool grinding = false;
    [SerializeField] public bool cangrind = false;
    public bool AnimatorBoolJumping;

    public float moveX;
    public float moveY;

    public bool isPlayerABall;
    public bool grounded;
    public Vector3 moveDirection;
    public float currentSpeed;
    public bool canBoost;
    public bool isBoosting;
    public Vector3 normal;
    public float lastJumpTime;
    public bool canCheckIfGrounded;
    public Vector3 desiredForward = Vector3.forward;
    private WiiU.GamePad gp = WiiU.GamePad.access;
    private Coroutine lateFixedLoopCoroutine;

    private void Start()
    {
        CheckIfAllIsAssigned();

        canCheckIfGrounded = true;
        cangrind = true;
        playerRailGrinding.cangrind = cangrind;
        gp.StopMotor();

        if (lateFixedLoopCoroutine == null)
        {
            lateFixedLoopCoroutine = StartCoroutine(LateFixedLoop());
        }
    }

    private void CheckIfAllIsAssigned()
    {
        // Add null checks here later if you want warnings in the Console.
    }

    private void Update()
    {
        if (!canMove)
            return;

        AbilitesManager();

        inputManager.ControllsManager();

        AbilitesManager();

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            UpdateRotation();
            MovePlayer();
        }
        AbilitesManager();

        UpdateGroundAndGravity();

        AbilitesManager();


        HandleGroundState();
        

        float velUp = Vector3.Dot(playerRigidbody.velocity, playerRigidbody.transform.up);
        if (grounded && velUp > 0f)
        {
            playerRigidbody.velocity -= playerRigidbody.transform.up * velUp;
        }
    }

    private IEnumerator LateFixedLoop()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
            LateFixedUpdate();
        }
    }

    private void LateFixedUpdate()
    {
        if (!grounded)
            return;

        float velAlongNormal = Vector3.Dot(playerRigidbody.velocity, normal);
        if (velAlongNormal > 0f)
        {
            playerRigidbody.velocity -= normal * velAlongNormal;
        }
    }

    private void UpdateGroundAndGravity()
    {
        playerGrounded.GroundedAndGravityManager();
    }

    private void HandleGroundState()
    {
        if (grounded)
        {
            homingAttackManager.SetSphereNonActive();
            isPlayerABall = false;
            AnimatorBoolJumping = false;

        }
        else
        {
            homingAttackManager.HomingAttackManagerSphere();
        }
    }

    private void UpdateRotation()
    {
        playerRotation.RotationManager();
    }

    private void MovePlayer()
    {
        if (isBoosting) return;
        if (grinding) return;
        UpdateSpeed();
        WallSliding();

        if (grounded)
        {
            playerRigidbody.drag = groundDrag;

            Vector3 direction;

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                direction = finalDirection.normalized;
            }
            else
            {
                // Preserve momentum direction
                Vector3 flatVelocity =Vector3.ProjectOnPlane(playerRigidbody.velocity, normal);

                direction = flatVelocity.sqrMagnitude > 0.001f? flatVelocity.normalized: transform.forward;
            }

            Vector3 targetVelocity = direction * currentSpeed;

            float dot = Vector3.Dot(targetVelocity, normal);

            playerRigidbody.velocity = targetVelocity - normal * dot;
        }
        else
        {
            playerRigidbody.drag = airDrag;

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                playerRigidbody.AddForce(moveDirection.normalized * airSpeed, ForceMode.Force);
            }
        }
    }



    private bool isWalkingInput;
    private bool isAccell;
    private void UpdateSpeed()
    {
         isWalkingInput = Mathf.Abs(moveX) <= deadZoneForWalk && Mathf.Abs(moveY) <= deadZoneForWalk;
        if (moveX == 0 && moveY == 0)
        {
            Decelerate();
            isAccell = false;
            return;
        }

        if (grounded)
        {
            Accelerate();
            isAccell = true;

        }
        else
        {
            Decelerate();
            isAccell = false;
        }
    }

    private void Decelerate()
    {
        if (grounded)
        {
            float threshold = walkSpeed + 0.1f;

            float decel = currentSpeed <= threshold
                ? walkDeceleration
                : runDeceleration;

            currentSpeed = Mathf.Lerp(currentSpeed, 0f, decel * Time.fixedDeltaTime);
        }
        if (!grounded)
        {
            Vector3 flatVelocity = new Vector3(playerRigidbody.velocity.x,0,playerRigidbody.velocity.z);
            currentSpeed = flatVelocity.magnitude;
        }
    }

    private void Accelerate()
    {
        float targetSpeed = isWalkingInput ? walkSpeed : maxSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
    }

    Vector3 finalDirection;
    [SerializeField]float wallCheckDistance = 2.5f;
     bool LastBool;

    private void WallSliding()
    {
        finalDirection = moveDirection;

        RaycastHit hit;

        //Debug
        Debug.DrawRay(playerRigidbody.centerOfMass, moveDirection * wallCheckDistance,Color.red);

        bool isTouchingAwall = false;

        if (Physics.Raycast(playerRigidbody.centerOfMass, moveDirection,out hit,wallCheckDistance))
        {
            if (Vector3.Angle(hit.normal, Vector3.up) > 5f)
            {
                finalDirection = Vector3.ProjectOnPlane(moveDirection, hit.normal).normalized;
                isTouchingAwall = true;

            }
        }
        if (LastBool != isTouchingAwall)
        {
           Debug.Log(isTouchingAwall);
           LastBool = isTouchingAwall;

        }
    }
    private void UpdateAnimator()
    {
        animatorScript.AnimatorManager(grounded, currentSpeed, isBoosting, moveX, moveY, isAccell, grinding);
    }

    public void StartScenAnimEnd(bool isBoost)
    {
        if (isBoost)
        {
            StartSceneWithBoost(maxSpeed);
        }

        canMove = true;
    }

    private void StartSceneWithBoost(float boostSpeed)
    {
        Vector3 forward = playerTransform.forward.normalized;

        playerRigidbody.AddForce(forward * boostSpeed, ForceMode.Impulse);
        playerRigidbody.velocity = forward * (boostSpeed - 10f);
        currentSpeed = boostSpeed - 10f;
    }

    private void AbilitesManager()
    {
        playerAbilities.AbilitiesManager();
    }

    public void GrindingManager(bool isgrinding, bool cangrinds = true)
    {

        grinding = isgrinding;
        playerRailGrinding.grinding = isgrinding;

        if (cangrind != cangrinds)
        {
            cangrind = cangrinds;
            playerRailGrinding.cangrind = cangrind;

            if (cangrind == false)
            {
                playerRailGrinding.StopGrinding();
                StartCoroutine(WaitAmmountTimeUnitlGrind(1f));
            }
        }

    }
    public void Jump()
    {
        AnimatorBoolJumping = true;
        playerAbilities.Jump();
    }

    public void Boost()
    {
        playerAbilities.Boost();
    }

    public void Spring(float springJumpHeight, string soundSfx, Vector3 direction, bool removesSpeed)
    {
        playerAbilities.Spring(springJumpHeight, soundSfx, direction, removesSpeed);
    }

    public void SetVelToZero()
    {
        playerRigidbody.velocity = Vector3.zero;
        playerRigidbody.angularVelocity = Vector3.zero;
    }

    private IEnumerator WaitAmmountTimeUnitlGrind(float delay)
    {
        yield return new WaitForSeconds(delay);
        cangrind = true;
        playerRailGrinding.cangrind = cangrind;

    }
}
