using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using WiiU = UnityEngine.WiiU;
public class PlayerMoving : MonoBehaviour
{

    [SerializeField] public bool CanScriptRun = true;

    //Too much to tweak. 
    [Header("Scripts")]
    [SerializeField] private InputManagerPlayer InputManager;
    [SerializeField] private HomingAttack HomingAttackManger;

    [Header("Health and Others")]
    [SerializeField] public int Lives; // Not used for now, but will be used in the future for sure. GameManager needed

    [SerializeField] public bool Invincebility;

    [SerializeField] private float InvincebilityTimer;

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

    [Header("Current states")]
    [SerializeField] public float moveX;
    [SerializeField] public float moveY;

    [SerializeField] public string BallorNot;
    [SerializeField] public bool grounded;

    [SerializeField] Vector3 moveDirection;

    [SerializeField] public float currentSpeed;


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

    [SerializeField] private bool UnleashedNoSnap;
    [SerializeField] private bool Snaping;

    [Header("Slopes")]
    [SerializeField] private float MaxSlopeAngle;
    [SerializeField] private float maxGroundChangeAngle;

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

    [Header("Tweak")]

    [SerializeField] private float groundDrag;
    [SerializeField] private float AirDrag;
    [SerializeField] private float groundCheckDelay = 0.1f;
    private float lastJumpTime;

    [Header("Gravity and Stuff")]
    [SerializeField] public bool cancheckifgrounded;
    [SerializeField] private float groundDistance;

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

    WiiU.GamePad gp = WiiU.GamePad.access;
    #region BugDeBug
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

    // BugDe
    float UpdateTime;
    float lateUpdateTime;
    float laterUpdateTime;
    #endregion
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

        //Snapping:
        smoothRotation = Rigidbody.rotation;
        snapRot = Rigidbody.rotation;
        turnRot = Rigidbody.rotation; //Gives stupid errors if you don't do this.
        lastNormal = Vector3.up;
    }

    private void CheckIfAllIsAssigned()
    {
        // Things that can be auto-fixed:
        if (InputManager == null)
            Debug.LogError("InputManager not assigned in PlayerMoving. Fixing for now."); InputManager = GetComponent<InputManagerPlayer>();

        if (HomingAttackManger == null)
            Debug.LogError("HomingAttackManger not assigned in PlayerMoving. Fixing for now."); HomingAttackManger = GetComponent<HomingAttack>();

        if (Rigidbody == null)
            Debug.LogError("Rigidbody not assigned in PlayerMoving. Fixing for now."); Rigidbody = GetComponent<Rigidbody>();

        if (PlayerTransform == null)
            Debug.LogError("PlayerTransform not assigned in PlayerMoving. Fixing for now."); PlayerTransform = transform;

        if (AudioManager == null)
            Debug.LogError("AudioManager not assigned in PlayerMoving. Fixing for now."); AudioManager = FindObjectOfType<AudioManager>();


        // Things that can't be auto-fixed:

        //if (Animator == null)
        //Debug.LogError("Animator not assigned in PlayerMoving."); NO. Wii U + CPU skinned mesh = hell for PPC1.

        if (orientation == null)
            Debug.LogError("Orientation not assigned in PlayerMoving. Cannot Fix.");

        if (CameraGameObject == null)
            Debug.LogError("CameraGameObject not assigned in PlayerMoving. Cannot Fix.");
 
    }

    #region UnityUpdates
    //Unity calls
    void Update()
    {
        float start = Time.realtimeSinceStartup;

        if (CanScriptRun == false)
        {
            return;
        }

        InputManager.ControllsManager();

        AnimatorManager();

        UpdateTime = (Time.realtimeSinceStartup - start) * 1000f;

    }

    void FixedUpdate()
    {
        float start = Time.realtimeSinceStartup;

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
        GroundChecker();


        Gravity();

        float velUp = Vector3.Dot(Rigidbody.velocity, Rigidbody.transform.up);
        if (grounded && velUp > 0f)
        {
            Rigidbody.velocity -= Rigidbody.transform.up * velUp;
        }

        lateUpdateTime = (Time.realtimeSinceStartup - start) * 1000f;

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
        float start = Time.realtimeSinceStartup;

        // GroundChecker();

        if (grounded)
        {            // Project using the surface normal so the velocity is tangent to the ground even if the transform isn't aligned.

            float velAlongNormal = Vector3.Dot(Rigidbody.velocity, normal);
            if (velAlongNormal > 0f)
            {
                Rigidbody.velocity -= normal * velAlongNormal;
            }
        }

        laterUpdateTime = (Time.realtimeSinceStartup - start) * 1000f;
    }

    // verticallSpeed Vector3.Dot(Rigidbody.velocity, Rigidbody.transform.up);
    // verticalVelocity Vector3.Project(Rigidbody.velocity, Rigidbody.transform.up);
    // horizontalVelocity Vector3.ProjectOnPlane(Rigidbody.velocity, Rigidbody.transform.up);
    #endregion


    #region Grounded and Gravity

    private float distanceToGround;
    private void GroundChecker()
    {
        if (Time.time - lastJumpTime < groundCheckDelay)
            return;

        RaycastHit hit;

        float speed = Rigidbody.velocity.magnitude;
        float extraDistance = speed * Time.fixedDeltaTime;

        float castDistance = groundDistance + 0.05f; // base

        // Slope thingy. Bug removed - player flies off for whenever they felt like it.
        if (desiredForward.sqrMagnitude > 0.001f)
        {
            Vector3 moveOnSlope = Vector3.ProjectOnPlane(desiredForward, normal);

            float slopeAngle = Vector3.Angle(normal, Vector3.up);

            float uphillDot = Vector3.Dot(moveOnSlope, Vector3.up);

            bool goingUphill = uphillDot > 0.01f;
            bool goingDownhill = uphillDot < -0.01f; //Why is this a bool?

            if (goingDownhill && slopeAngle >= SlopeAngleWhereGainSpeed)
            {
                // Extend ground detection slightly when going downhill
                castDistance += extraDistance;
            }

            if (goingUphill && slopeAngle >= SlopeAngleWhereLooseSpeed)
            {
                // Optional: make it slightly stricter uphill
                castDistance -= 0.02f; //Magic numbers. Should be in editor but there are already too many things to tweak.
            }
        }

        // GroundCheck - maybe I should seperate them into diffrent private voids?
        if (cancheckifgrounded)
        {
            bool hitGround = Physics.SphereCast(
                Rigidbody.worldCenterOfMass,
                groundDistance,
                -Rigidbody.transform.up,
                out hit,
                castDistance,
                LayerMaskGround,
                QueryTriggerInteraction.Ignore
            );

            grounded = hitGround;
            normal = grounded ? hit.normal.normalized : Vector3.up;

            float distanceToGround = grounded ? hit.distance : 0f;

            if (grounded)
            {
                // Snap to ground (fix: hover)
                float snapAmount = distanceToGround - groundDistance;

                if (snapAmount > 0.001f)
                {
                    Rigidbody.position -= normal * snapAmount;
                }

                float velAlongNormal = Vector3.Dot(Rigidbody.velocity, normal);

                // Only cancel motion leaving the surface
                if (velAlongNormal > 0f)
                {
                    Rigidbody.velocity -= normal * velAlongNormal;
                }

                float stickForce = Mathf.Max(10f, speed);
                Rigidbody.AddForce(-normal * stickForce, ForceMode.Acceleration);

                WhenGrounded();
            }
            else
            {
                WhenNotGrounded();
            }
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

        //Make sure the player isn't tilted.

        smoothRotation = Rigidbody.rotation;
        snapRot = Rigidbody.rotation;
        turnRot = Rigidbody.rotation;
        lastNormal = Vector3.up;
        normal = Vector3.up;

        desiredForward = Vector3.ProjectOnPlane(transform.forward, normal);
        Snap();
        Turn();
        Smooth();
        Rigidbody.MoveRotation(smoothRotation); //Fix: player tilted when jumping from a slope, would get the player stuck in weird positions. Funny sometimes. Remove when you want to make the player suffer.

    }

    private void Gravity()
    {
        if (grounded == false)
        {
            Rigidbody.velocity -= Vector3.up * gravity * Time.fixedDeltaTime;
        }
        else
        {
            Rigidbody.velocity -= normal * Groundedgravity * Time.fixedDeltaTime;
        }
    }

    #endregion


    #region Rotation

    //Don't touch the Rotation stuff, it's really hard to get right and it works prob fine as it is.
    Quaternion snapRot = new Quaternion(0, 0, 0, 0);
    Quaternion turnRot = new Quaternion(0, 0, 0, 0);
    Quaternion smoothRotation = new Quaternion(0, 0, 0, 0);

    Vector3 lastNormal = new Vector3(0, 0, 0);
    Vector3 desiredForward = Vector3.forward; 
    //Make sure everything is zerod out.

    private void CalculateMovementBasedOnCam()
    {
        // raw camera directions
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        // Project camera axes onto the ground plane so movement is tangent to surface
        Vector3 CrNrm = (normal.sqrMagnitude > 0.001f) ? normal : Vector3.up; // normal is already normalized in GroundChecker - I hope.
        // inline ProjectOnPlane: v' = v - n * dot(v, n)
        float df = Vector3.Dot(camForward, CrNrm);
        camForward -= CrNrm * df;
        float DirectionF = Vector3.Dot(camRight, CrNrm);
        camRight -= CrNrm * DirectionF;

        if (camForward.sqrMagnitude < 0.001f) camForward = Vector3.ProjectOnPlane(Vector3.forward, CrNrm);
        if (camRight.sqrMagnitude < 0.001f) camRight = Vector3.ProjectOnPlane(Vector3.right, CrNrm);

        camForward.Normalize();
        camRight.Normalize();

        // Movement input relative to camera, already tangent to surface.
        Vector3 raw = camForward * moveY + camRight * moveX;
        moveDirection = raw.sqrMagnitude > 0.001f ? raw.normalized : Vector3.zero;
    }
    private void CalculateRotation()
    {
        if (moveDirection.sqrMagnitude <= 0.001f) return;

        if (UnleashedNoSnap)
        {
            //Unleashed doen't actually snap to the ground???? Having other mode for slopes and stuff -> Shouldn't be default mode. Need fixing.
            desiredForward = moveDirection;
            if (desiredForward.sqrMagnitude <= 0.01f) return;

            turnRot = Quaternion.LookRotation(desiredForward.normalized, Vector3.up);
            Smooth();

            Rigidbody.MoveRotation(smoothRotation);
        }
        if (Snaping)
        {
            desiredForward = Vector3.ProjectOnPlane(moveDirection, normal);
            if (desiredForward.sqrMagnitude <= 0.001f) return;

            Snap();
            Turn();
            Smooth();

            Rigidbody.MoveRotation(smoothRotation);
        }
    }

    private void Snap()
    {
        float normalAngle = Vector3.Angle(lastNormal, normal);

        if (normalAngle > maxGroundChangeAngle)
        {
            // reject sudden wall normals - doesn't work :(
            normal = lastNormal;
        }
        else
        {
            lastNormal = normal;
        }

        // align up to ground
        snapRot = Quaternion.FromToRotation(Rigidbody.transform.up, normal) * Rigidbody.rotation;
    }

    private void Turn()
    {
        turnRot = Quaternion.LookRotation(desiredForward.normalized, normal);
    }

    private void Smooth()
    {
        float t = 1f - Mathf.Exp(-rotationSpeed * Time.fixedDeltaTime);

        // blend snap + turn into a single target
        Quaternion combinedTarget = Quaternion.Slerp(snapRot, turnRot, 1f);

        // don't reset smoothRotation every frame
        smoothRotation = Quaternion.Slerp(smoothRotation, combinedTarget, t);
    }

    private void SpeedLoss() //??? - should it be in the game?
    {
        if (desiredForward.sqrMagnitude <= 0.001f) return;

        // Cache the projected move direction to avoid repeating ProjectOnPlane calls.
        Vector3 moveOnSlope = desiredForward; // already projected above
        bool goingUphill = Vector3.Dot(moveOnSlope, Vector3.up) > 0f;
        bool goingDownhill = Vector3.Dot(moveOnSlope, Vector3.down) > 0f;

        float slopeAngle = Vector3.Angle(normal, Vector3.up);

        if (slopeAngle <= MaxSlopeAngle)
        {
            snapRot = Quaternion.FromToRotation(Rigidbody.transform.up, normal) * Rigidbody.rotation;
        }
        if (goingUphill && slopeAngle >= SlopeAngleWhereLooseSpeed)  //Pain and suffering here.
        {
            //Up hill thing.
            currentSpeed -= SlopeAngleSpeedLoss * Time.fixedDeltaTime;
        }

        if (goingDownhill && slopeAngle >= SlopeAngleWhereGainSpeed)
        {
            //Donwhill
            currentSpeed += SlopeAngleSpeedGain * Time.fixedDeltaTime;
            // Fix: original Clamp did nothing. Limit speed to SpeedGainMax (minimum 0).
            currentSpeed = Mathf.Clamp(currentSpeed, 0f, SpeedGainMax);
        }
    }

    #endregion


    #region Movement
    private void MovePlayer()
    {
        AccelDeAccelManager();

        // When grounded, always ensure velocity is tangent to the surface so the player "sticks"
        if (grounded)
        {
            Rigidbody.drag = groundDrag;

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                Vector3 dir = moveDirection.normalized; // normalized once
                Vector3 v = dir * currentSpeed;
                // inline ProjectOnPlane using normalized normal
                float d = Vector3.Dot(v, normal);
                Rigidbody.velocity = v - normal * d; //??? doesn't preseve the momentum??? should it? maybe? do I care?


            }
            else
            {
                // No input: preserve existing horizontal motion but remove any component along the normal
                Vector3 horiz = Vector3.ProjectOnPlane(Rigidbody.velocity, normal);
                Rigidbody.velocity = horiz;
            }
        }
        else
        {
            Rigidbody.drag = AirDrag;
            if (moveDirection.sqrMagnitude > 0.001f)
            {
                Rigidbody.AddForce(moveDirection.normalized * airspeed, ForceMode.Force);
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
        currentSpeed = Mathf.Lerp(currentSpeed, 0, Deacceleration * Time.fixedDeltaTime);
    }
    private void AccelCalc()
    {
        if (Mathf.Abs(moveX) <= deadzoneforwalk && Mathf.Abs(moveY) <= deadzoneforwalk)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, WalkSpeed, Acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, MaxSpeed, Acceleration * Time.fixedDeltaTime);
        }
    }

    #endregion


    #region Abilities
    public void Jump()
    {
        if (!CanMove) return;
        if (!grounded)
        {
            HomingAttackManger.HomingAttackManagerJump();
            return;
        }
        SwitchToBall();

        // Remove velocity component along the ground normal instead of global Y so jump is consistent on slopes
        if (JumpsFrom0)
        {
            float velAlongNormal = Vector3.Dot(Rigidbody.velocity, normal);
            Rigidbody.velocity -= normal * velAlongNormal;
        }

        // Apply jump along ground normal — use normal computed by ground check for consistent height on slopes
        Rigidbody.AddForce(normal * jumpForce * 10f, ForceMode.Impulse);

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
        if (Vector3.Dot(Rigidbody.velocity, PlayerTransform.forward) >= BoosSpeed && CanBoost) //Fix: infinte fly thing. added && canboost
        {
            Rigidbody.velocity = boostDirection.normalized * BoosSpeed;
        }
    }

    // This here makes the player jump up and play a sound
    public void Spring(float SpringJumpHeight, string SoundSFX, Vector3 direction, bool RemovesSpeed)
    {
        cancheckifgrounded = false;
        if (HomingAttackManger.IsHoming) HomingAttackManger.StopHomingCoroutine();
        if (RemovesSpeed == true) SetVelToZero();

        Rigidbody.AddForce(direction.normalized * SpringJumpHeight * 10f , ForceMode.Impulse); //Direction is the spring direction, not the player forward. Should be used for jump pads and stuff like that. 10f for better
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

#endregion

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
    } // Not used for now;



    //No move and move again. Code from 2024 ? - why??
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
    } //Why isn't this a bool?

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
        cancheckifgrounded = true; //Fixes bug when using spring and then getting stuck.
    }

    void OnDrawGizmosSelected()
    {
        if (moveDirection.sqrMagnitude > 0.001f)
     {
           Vector3 flatMoveDir = new Vector3(moveDirection.x, 0f, moveDirection.z);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, flatMoveDir * 3f);

            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 2f, LayerMaskGround))
            {
     
            }
        }


    } //Debug

    //Making lag to make game worser. Don't remove. Makes game run worse. :P

    private string _cachedUpdateLabel;
    private string _cachedLateUpdateLabel;
    private string _cachedLaterUpdateLabel;
    private string _cachedRamEst;
    private string _cachedGBEst;
    private string _Renderes;
    void OnGUI()
    {
        _uiTimerAcc += Time.deltaTime;
        if (_uiTimerAcc >= UI_UPDATE_INTERVAL)
        {
            var renderers = FindObjectsOfType<Renderer>();
            int count = renderers.Length;

            long mem = System.GC.GetTotalMemory(false);
            float mb = mem / (1024f * 1024f);

            long GCColl = System.GC.CollectionCount(0);
            //float GarbMB = GCColl / (1024f * 1024f);

            _cachedUpdateLabel = string.Format("PlayerM:Update: {0:F2} ms", UpdateTime);
            _cachedLateUpdateLabel = string.Format("PlayerM:LateUpdate: {0:F2} ms", lateUpdateTime);
            _cachedLaterUpdateLabel = string.Format("PlayerM:LaterUpdate: {0:F2} ms", laterUpdateTime);
            _Renderes = string.Format("MaxRenderes: {0:F2} Nbr", count);
            _cachedRamEst = string.Format("RamEST: {0:N0}", mem);

            _cachedRamEst = string.Format("RamEST~: {0:F2}", mb + "~MB");
            _cachedGBEst = string.Format("GarbageEST~: {0:N0}", GCColl);

            _uiTimerAcc = 0f;
        }

        GUI.Label(new Rect(10, 10, 300, 20), _cachedUpdateLabel);
        GUI.Label(new Rect(10, 30, 300, 20), _cachedLateUpdateLabel);
        GUI.Label(new Rect(10, 50, 300, 20), _cachedLaterUpdateLabel);
        GUI.Label(new Rect(10, 130, 300, 20), _cachedRamEst);
        GUI.Label(new Rect(10, 150, 300, 20), _cachedGBEst);
        GUI.Label(new Rect(10, 170, 300, 20), _Renderes);
    }

}
