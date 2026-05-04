using System.Collections;
using UnityEngine;
using WiiU = UnityEngine.WiiU;

public class PlayerMoving : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] public GameObject Camera;

    [Header("Scripts")]
    [SerializeField] private InputManagerPlayer inputManager;
    [SerializeField] private HomingAttack homingAttackManager;
    [SerializeField] private AnimatorScriptS animatorScript;
    [SerializeField] private PlayerRotation playerRotation;
    [SerializeField] private PlayerAbilities playerAbilities;
    [SerializeField] private PlayerGrounded playerGrounded;

    [Header("Health and Others")]
    [SerializeField] public int lives;

    [Header("Movement")]
    [SerializeField] public float maxSpeed;
    [SerializeField] public float walkSpeed;
    [SerializeField] public float crawlSpeed;
    [SerializeField] public float deadZoneForWalk;

    [SerializeField] public float airSpeed;
    [SerializeField] public float rotationSpeed;
    [SerializeField] public float acceleration;
    [SerializeField] private float walkDeceleration;
    [SerializeField] private float runDeceleration;

    [Header("Drag")]
    [SerializeField] public float groundDrag;
    [SerializeField] public float airDrag;

    [Header("States")]
    [SerializeField] public bool canMove = true;

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
        UpdateSpeed();

        if (grounded)
        {
            playerRigidbody.drag = groundDrag;

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                Vector3 direction = moveDirection.normalized;
                Vector3 targetVelocity = direction * currentSpeed;

                float dot = Vector3.Dot(targetVelocity, normal);
                playerRigidbody.velocity = targetVelocity - normal * dot;
            }
            else
            {
                //playerRigidbody.velocity = Vector3.ProjectOnPlane(playerRigidbody.velocity, normal);
                Vector3 direction = transform.forward.normalized;
                Vector3 targetVelocity = direction * currentSpeed;

                float dot = Vector3.Dot(targetVelocity, normal);
                playerRigidbody.velocity = targetVelocity - normal * dot;
            }
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
        float threshold = walkSpeed + 0.1f;

        float decel = currentSpeed <= threshold
            ? walkDeceleration
            : runDeceleration;

        currentSpeed = Mathf.Lerp(currentSpeed, 0f, decel * Time.fixedDeltaTime);
    }

    private void Accelerate()
    {


        float targetSpeed = isWalkingInput ? walkSpeed : maxSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
    }

    private void UpdateAnimator()
    {
        animatorScript.AnimatorManager(grounded, currentSpeed, isBoosting, moveX, moveY, isAccell);
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
    public void Jump()
    {
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
}
