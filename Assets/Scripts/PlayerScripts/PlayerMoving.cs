using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using WiiU = UnityEngine.WiiU;
public class PlayerMoving : MonoBehaviour
{

    [SerializeField] public bool CanScriptRun = true;

    [Header("Scripts")]
    [SerializeField] private InputManagerPlayer InputManager;
    [SerializeField] private HomingAttack HomingAttackManger;
    [SerializeField] private Scenemanager Scenemanager;


    [Header("Main Controlls")]
    [SerializeField] private float MaxSpeed;
    [SerializeField] private float WalkSpeed;
    [SerializeField] private float deadzoneforwalk;
    [SerializeField] private float CrawlSpeed;


    [SerializeField] private float airspeed;
    [SerializeField] private float rotationSpeed;

    [SerializeField] private float Acceleration;
    [SerializeField] private float Deacceleration;

    [SerializeField] private float jumpForce;
    [SerializeField] private float airMulitplier;


    [Header("Boost")]
    [SerializeField] private float BoosSpeed;
    [SerializeField] private float BoosSpeedAir;
    [SerializeField] private bool CanBoost;

    [SerializeField] public bool BoostingCurrently;

    [SerializeField] private bool BoostReturnsWhenGrounded;
    [SerializeField] private bool BoostReturnsWhenBouncesFromObject;
    [SerializeField] private string BoostSFX;
    [SerializeField] private string BoostAirSFX;


    [Header("Controlls+")]
    [SerializeField] public bool CanMove;

    [SerializeField] private bool JumpsFrom0;

    [Header("Slopes")]
    [SerializeField] private float MaxSlopeAngle;
    [Header("UpHill")]

    [SerializeField] private float SlopeAngleWhereLooseSpeed;
    [SerializeField] private float SlopeAngleSpeedLoss;

    [Header("DownHill")]
    [SerializeField] private float SlopeAngleWhereGainSpeed;
    [SerializeField] private float SlopeAngleSpeedGain;
    [SerializeField] private float SpeedGainMax;


    [Header("Gravity")]
    [SerializeField] private float gravity;
    [SerializeField] private float Groundedgravity;
    [SerializeField] Vector3 normal;


    [Header("Variables")]
    [SerializeField] bool Invincebility;

    [SerializeField] private float InvincebilityTimer;

    [SerializeField] public string BallorNot;

    [Header("Tweak")]

    [SerializeField] private float groundDrag;
    [SerializeField] private float AirDrag;
    [SerializeField] private float groundCheckDelay = 0.1f;
    private float lastJumpTime;

    [Header("Gravity and Stuff")]
    [SerializeField] public bool cancheckifgrounded;

    [SerializeField] private Transform groundCheckObj;
    [SerializeField] private float groundDistance;

    [SerializeField] public bool grounded;

    [SerializeField] LayerMask LayerMaskGround;

    [Header("Animtions")]
    [SerializeField] Animator Animator;


    [Header("Main Objects")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform PlayerTransform;

    [SerializeField] public GameObject CameraGameObject;
    [SerializeField] private Rigidbody Rigidbody;


    [Header("AudioManager")]
    [SerializeField] private AudioManager AudioManager;
    [SerializeField] private string JumpSFX;
    [SerializeField] private string JumpSFX2;

    Vector3 moveDirection;

    WiiU.GamePad gp = WiiU.GamePad.access;

    // --- CACHED / REDUCED-ALLOCATION FIELDS ---
    private Coroutine _lateFixedLoopCoroutine;
    private bool _prevGrounded;
    private bool _prevJumping;
    private bool _prevBoosting;
    private float _prevSpeedNormalized = -1f;
    private const float AnimatorFloatEpsilon = 1e-3f;

    private float _uiTimerAcc;
    private const float UI_UPDATE_INTERVAL = 0.05f;

    private Transform cam;
    void Start()
    {
        CheckIfAllIsAssigned();

        cancheckifgrounded = true;
        EnableControlls();
        gp.StopMotor();

        // Start a single repeating coroutine instead of starting one each FixedUpdate (avoids string lookup & allocation).
        if (_lateFixedLoopCoroutine == null)
            _lateFixedLoopCoroutine = StartCoroutine(LateFixedLoop());

        cam = CameraGameObject.transform;
    }

    private void CheckIfAllIsAssigned()
    {
        // Things that can be auto-fixed:
        if (InputManager == null)
            Debug.LogError("InputManager not assigned in PlayerMoving. Fixing for now."); InputManager = GetComponent<InputManagerPlayer>();

        if (HomingAttackManger == null)
            Debug.LogError("HomingAttackManger not assigned in PlayerMoving. Fixing for now."); HomingAttackManger = GetComponent<HomingAttack>();

        if (Scenemanager == null)
            Debug.LogError("Scenemanager not assigned in PlayerMoving. Fixing for now."); Scenemanager = FindObjectOfType<Scenemanager>();

        if (Rigidbody == null)
            Debug.LogError("Rigidbody not assigned in PlayerMoving. Fixing for now."); Rigidbody = GetComponent<Rigidbody>();

        if (PlayerTransform == null)
            Debug.LogError("PlayerTransform not assigned in PlayerMoving. Fixing for now."); PlayerTransform = transform;

        if (AudioManager == null)
            Debug.LogError("AudioManager not assigned in PlayerMoving. Fixing for now."); AudioManager = FindObjectOfType<AudioManager>();


        // Things that can't be auto-fixed:

        //if (Animator == null)
        //Debug.LogError("Animator not assigned in PlayerMoving."); 

        if (orientation == null)
            Debug.LogError("Orientation not assigned in PlayerMoving. Cannot Fix.");

        if (CameraGameObject == null)
            Debug.LogError("CameraGameObject not assigned in PlayerMoving. Cannot Fix.");

        if (groundCheckObj == null)
            Debug.LogError("GroundCheck not assigned in PlayerMoving. Cannot Fix."); 
    }

    void Update()
    {
        if (CanScriptRun == false)
        {
            return;
        }

        InputManager.ControllsManager();

        AnimatorManager();
    }

    void FixedUpdate()
    {
        if (CanScriptRun == false)
        {
            return;
        }

        if (CanMove)
        {
            CalculateMovementBasedOnCam();
            CalculateRotation();
            MovePlayer();
        }

        Gravity();

        if (grounded && Vector3.Dot(Rigidbody.velocity, Rigidbody.transform.up) < Rigidbody.sleepThreshold)
        {
            Rigidbody.velocity = Vector3.ProjectOnPlane(Rigidbody.velocity, Rigidbody.transform.up);
        }
    }

    // Single coroutine running once per physics frame for late physics processing.
    IEnumerator LateFixedLoop()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
            LateFixedUpdate();
        }
    }

    void LateFixedUpdate()
    {
        GroundChecker();
        if (grounded)
        {
            Rigidbody.velocity = Vector3.ProjectOnPlane(Rigidbody.velocity, Rigidbody.transform.up);
        }
    }

    // verticallSpeed Vector3.Dot(Rigidbody.velocity, Rigidbody.transform.up);
    // verticalVelocity Vector3.Project(Rigidbody.velocity, Rigidbody.transform.up);
    // horizontalVelocity Vector3.ProjectOnPlane(Rigidbody.velocity, Rigidbody.transform.up);

    #region Grounded and Gravity
    private void GroundChecker()
    {
        if (Time.time - lastJumpTime < groundCheckDelay)
            return;

        RaycastHit hit;

        float speed = Rigidbody.velocity.magnitude;
        float extraDistance = speed * Time.fixedDeltaTime;

        // SphereCast is MUCH more stable than Raycast
        if (cancheckifgrounded == true)
        {
            bool hitGround = Physics.SphereCast(Rigidbody.worldCenterOfMass, groundDistance, -Rigidbody.transform.up, out hit, groundDistance + extraDistance, LayerMaskGround, QueryTriggerInteraction.Ignore);
            grounded = hitGround;
            normal = grounded ? hit.normal.normalized : Vector3.up;
        }

        if (grounded)
        {
            // Remove velocity INTO the surface (not away from it)
            float normalVel = Vector3.Dot(Rigidbody.velocity, normal);
            if (normalVel < 0f)
            {
                Rigidbody.velocity -= normal * normalVel;
            }

            // Adhesion force to keep player glued on loops
            float stickForce = Mathf.Max(10f, speed);
            Rigidbody.AddForce(-normal * stickForce, ForceMode.Acceleration);

            WhenGrounded();
        }
        else
        {
            WhenNotGrounded();
        }
    }

    private void WhenGrounded()
    {
        HomingAttackManger.SetSphereNonActive();
        BallorNot = "Not";

        if (BoostReturnsWhenGrounded)
        {
            CanBoost = true;
        }
    }

    private void WhenNotGrounded()
    {
        HomingAttackManger.HomingAttackManagerSphere();

    }

    private void Gravity()
    {
        if (grounded == false)
        {
            Rigidbody.velocity -= Vector3.up * gravity * Time.deltaTime;
        }
        else
        {
            Rigidbody.velocity -= normal * Groundedgravity * Time.deltaTime;
        }
    }

    #endregion

    public float moveX;
    public float moveY;

    #region Movment and other

    //Don't touch the Rotation stuff, it's really hard to get right and it works prob fine as it is.
    private void CalculateMovementBasedOnCam()
    {

        // raw camera directions
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        // Project camera axes onto the ground plane so movement is tangent to surface
        Vector3 n = (normal.sqrMagnitude > 0.001f) ? normal : Vector3.up; // normal is already normalized in GroundChecker
        // inline ProjectOnPlane: v' = v - n * dot(v, n)
        float df = Vector3.Dot(camForward, n);
        camForward -= n * df;
        float dr = Vector3.Dot(camRight, n);
        camRight -= n * dr;

        if (camForward.sqrMagnitude < 0.001f) camForward = Vector3.ProjectOnPlane(Vector3.forward, n);
        if (camRight.sqrMagnitude < 0.001f) camRight = Vector3.ProjectOnPlane(Vector3.right, n);

        camForward.Normalize();
        camRight.Normalize();

        // Movement input relative to camera, already tangent to surface
        Vector3 raw = camForward * moveY + camRight * moveX;
        moveDirection = raw.sqrMagnitude > 0.001f ? raw.normalized : Vector3.zero;
    }

    private Quaternion snapRot;
    private void CalculateRotation()
    {
        if (moveDirection.sqrMagnitude <= 0.001f) return;

        // make the move direction tangent to the surface to compute desired forward
        Vector3 desiredForward = Vector3.ProjectOnPlane(moveDirection, normal);

        if (desiredForward.sqrMagnitude <= 0.001f) return;

        // 1. SNAP
        // Cache the projected move direction to avoid repeating ProjectOnPlane calls.
        Vector3 moveOnSlope = desiredForward; // already projected above
        bool goingUphill = Vector3.Dot(moveOnSlope, Vector3.up) > 0f;
        bool goingDownhill = Vector3.Dot(moveOnSlope, Vector3.down) > 0f;

        float slopeAngle = Vector3.Angle(normal, Vector3.up);

        if (slopeAngle <= MaxSlopeAngle)
        {
            snapRot = Quaternion.FromToRotation(Rigidbody.transform.up, normal) * Rigidbody.rotation;
        }
        if (goingUphill && slopeAngle >= SlopeAngleWhereLooseSpeed)
        {
            currentSpeed -= SlopeAngleSpeedLoss * Time.fixedDeltaTime;
        }

        if (goingDownhill && slopeAngle >= SlopeAngleWhereGainSpeed)
        {
            currentSpeed += SlopeAngleSpeedGain * Time.fixedDeltaTime;
            // Fix: original Clamp did nothing. Limit speed to SpeedGainMax (minimum 0).
            currentSpeed = Mathf.Clamp(currentSpeed, 0f, SpeedGainMax);
        }

        // 2. TURN
        Quaternion target = Quaternion.LookRotation(desiredForward.normalized, normal);


        // 3. SMOOTH
        float t = 1f - Mathf.Exp(-rotationSpeed * Time.fixedDeltaTime);
        Rigidbody.MoveRotation(Quaternion.Slerp(snapRot, target, t));
    }
    //End for Rotation, start for movement.


    public float currentSpeed;
    private void MovePlayer()
    {
        AccelDeAccelManager();

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Vector3 dir = moveDirection.normalized; // normalized once

            if (grounded)
            {
                Rigidbody.drag = groundDrag;
                Vector3 v = dir * currentSpeed;
                // inline ProjectOnPlane using normalized normal
                float d = Vector3.Dot(v, normal);
                Rigidbody.velocity = v - normal * d;
            }
            if (!grounded)
            {
                Rigidbody.drag = AirDrag;
                Rigidbody.AddForce(dir * airspeed, ForceMode.Force);
            }
        }
    }

    private void AccelDeAccelManager()
    {
        if (moveX == 0 && moveY == 0)
        {
            DeaccelCalc();
        }
        else
        {
            if (grounded)
            {
                AccelCalc();
            }
            else
            {
                DeaccelCalc();
            }

        }
    }

    private void DeaccelCalc()
    {
        currentSpeed = Mathf.Lerp(currentSpeed, 0, Deacceleration * Time.deltaTime);
    }
    private void AccelCalc()
    {
        if (Mathf.Abs(moveX) <= deadzoneforwalk && Mathf.Abs(moveY) <= deadzoneforwalk)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, WalkSpeed, Acceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, MaxSpeed, Acceleration * Time.deltaTime);
        }
    }

    #endregion

    #region Apilytis
    public void Jump()
    {
        if (!CanMove) return;
        if (!grounded)
        {
            HomingAttackManger.HomingAttackManagerJump();
            return;
        }
        SwitchToBall();

        if (JumpsFrom0) Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, 0f, Rigidbody.velocity.z);

        Rigidbody.AddForce(transform.up * jumpForce * 10f, ForceMode.Impulse);

        if (AudioManager != null)
        {
            AudioManager.Play(JumpSFX);
            AudioManager.Play(JumpSFX2);
        }

        grounded = false;
        lastJumpTime = Time.time; // record jump time
    }

    public void Boost()
    {
        if (!CanMove) return;

        Vector3 boostDirection = PlayerTransform.forward; // Use the player's forward direction

        if (grounded && CanBoost)
        {
            Rigidbody.AddForce(boostDirection.normalized * BoosSpeed, ForceMode.Impulse);
            BoostingCurrently = true;
            if (AudioManager != null) AudioManager.Play(BoostSFX);
        }
        if (!grounded && CanBoost)
        {
            Rigidbody.AddForce(boostDirection.normalized * BoosSpeedAir, ForceMode.Impulse);
            if (AudioManager != null) AudioManager.Play(BoostAirSFX);
            CanBoost = false;
        }
        if (Vector3.Dot(Rigidbody.velocity, PlayerTransform.forward) >= BoosSpeed)
        {
            Rigidbody.velocity = boostDirection.normalized * BoosSpeed;
        }
    }

    #endregion

    // This here makes the player jump up and play a sound
    public void Spring(float SpringJumpHeight, string SoundSFX, Vector3 direction, bool RemovesSpeed)
    {
        cancheckifgrounded = false;
        if (HomingAttackManger.IsHoming) HomingAttackManger.StopHomingCoroutine();
        if (RemovesSpeed == true) SetVelToZero();

        Rigidbody.AddForce(direction.normalized * SpringJumpHeight * 10f, ForceMode.Impulse); // Times ten for more height
        grounded = false;

        if (!string.IsNullOrEmpty(SoundSFX) && SoundSFX != "None" && AudioManager != null)
            AudioManager.Play(SoundSFX);

        if (BoostReturnsWhenBouncesFromObject)
        {
            CanBoost = true;
        }

        // Avoid allocating a lambda each call; use a dedicated coroutine.
        StartCoroutine(WaitAndEnableGroundCheck(1f));
    }


    public void VictoryMode()
    {
        //TimerRuns = false;


        SetVelToZero();
        CanMove = false;
        SetVelToZero();

        // Animator.SetBool("Victory", true);
        //int I;
       // if (timer <= 200) I = 5;
       // else if (timer <= 250) I = 4;
       // else if (timer <= 300) I = 3;
       /// else if (timer <= 450) I = 2;
      ///  else I = 1;
        // Animator.SetInteger("Rank", I);
        SetVelToZero();

    } // Not used for now.

    private void AnimatorManager()
    {
        return;
        // Only update animator parameters when they change to reduce internal overhead.
        if (_prevGrounded != grounded)
        {
            Animator.SetBool("Grounded", grounded);
            _prevGrounded = grounded;
        }

        // Jumping state
        bool jumping = !grounded;
        if (_prevJumping != jumping)
        {
            Animator.SetBool("Jumping", jumping);
            _prevJumping = jumping;
        }

        // Current speed normalized
        float speedNorm = (MaxSpeed > 0f) ? (currentSpeed / MaxSpeed) : 0f;
        if (Mathf.Abs(_prevSpeedNormalized - speedNorm) > AnimatorFloatEpsilon)
        {
            Animator.SetFloat("CurrentSpeed", speedNorm);
            _prevSpeedNormalized = speedNorm;
        }

        if (_prevBoosting != BoostingCurrently)
        {
            Animator.SetBool("Boosting", BoostingCurrently);
            _prevBoosting = BoostingCurrently;
        }
    } // Not used for now.



    //No move and move again.
    public void DisableControlls()
    {
        CanMove = false;
    }
    public void EnableControlls()
    {
        CanMove = true;
    }

    public void PLayerHasBeenHit() // Not used for now
    {
        if (!Invincebility)
        {
            Invincebility = true;
            // Avoid using string-based Invoke (reflection). Use coroutine to clear invincibility.
            StartCoroutine(InvincibilityTimerCoroutine());
        }

    }
    IEnumerator InvincibilityTimerCoroutine()
    {
        yield return new WaitForSeconds(InvincebilityTimer);
        Invincebility = false;
    }

    public void SwitchToBall()
    {
        BallorNot = "Ball";
    }

    public void SwitchToStanding()
    {
        BallorNot = "Not";
    }
    public void SetVelToZero()
    {
        Rigidbody.velocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
    }

    IEnumerator WaitAndEnableGroundCheck(float time)
    {
        yield return new WaitForSeconds(time);
        cancheckifgrounded = true;
    }

    void OnDrawGizmosSelected()
    {
       // Gizmos.color = Color.yellow;
     //   if (groundCheck != null)
      //      Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
//
      //  if (moveDirection.sqrMagnitude > 0.001f)
     //   {
     //       Vector3 flatMoveDir = new Vector3(moveDirection.x, 0f, moveDirection.z);
     //       Gizmos.color = Color.blue;
     //       Gizmos.DrawRay(transform.position, flatMoveDir * 3f);
//
     //       RaycastHit hit;
    //        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 2f, LayerMaskGround))
    //        {
     //           Gizmos.color = Color.green;
     //           //Gizmos.DrawRay (hit.normal);
     //       }
    //    }
    } //Debug
}

