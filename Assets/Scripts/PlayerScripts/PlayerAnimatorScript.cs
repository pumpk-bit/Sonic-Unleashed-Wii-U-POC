using UnityEngine;

public class PlayerAnimatorScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMoving playerMoving;
    [SerializeField] private Animator animator;
    [SerializeField] private FootStepManagerAudio footStepManagerAudio;

    [Header("Bored Settings")]
    [SerializeField] private float boredMinTime = 12f;
    [SerializeField] private float boredMaxTime = 15f;

    [Header("Footstep Settings")]
    [SerializeField] private GroundType groundTypeAnimator;

    private float maxSpeed;
    private float boredTimer;
    private float nextBoredTime = 12f;

    private bool prevGrounded;
    private bool prevJumping;
    private bool prevBoosting;
    private float prevSpeedNormalized = -1f;
    private const float AnimatorFloatEpsilon = 0.001f;

    private bool savedGrounded = true;

    private int soundLength;
    private string soundStartName;

    private string currentAnimation;

    private static readonly int GroundedHash = Animator.StringToHash("Grounded");
    private static readonly int JumpingHash = Animator.StringToHash("Jumping");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int LeftAndRightHash = Animator.StringToHash("LeftAndRight");
    private static readonly int LeftHash = Animator.StringToHash("Left");
    private static readonly int RightHash = Animator.StringToHash("Right");
    private static readonly int BoostingHash = Animator.StringToHash("Boosting");
    private static readonly int SpeedMultiplierHash = Animator.StringToHash("SpeedMultiplyer");
    private static readonly int IsAccelHash = Animator.StringToHash("IsAccel");
    private static readonly int IsGrindingHash = Animator.StringToHash("Grinding");
    private static readonly int BoredHash = Animator.StringToHash("Bored");
    private static readonly int BoredNumberHash = Animator.StringToHash("BoredNumber");
    private static readonly int StartTypeHash = Animator.StringToHash("StartType");
    public enum GroundType
    {
        Concrete,
        Dirt,
        Grass,
        Metal,
        Snow,
        Water,
        Wood,
    }

   // public GroundType GroundTypeAnimator;
    public GroundType ChooseGroundType;

    public bool PlayerStepRandom = true;

    public float StepSoundVolume;



    private void Start()
    {
        if (playerMoving == null)
            playerMoving = FindObjectOfType<PlayerMoving>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (footStepManagerAudio == null)
            footStepManagerAudio = FindObjectOfType<FootStepManagerAudio>();

        if (playerMoving != null)
            maxSpeed = playerMoving.maxSpeed;

        nextBoredTime = Random.Range(boredMinTime, boredMaxTime);
    }

    public void AnimatorManager(bool grounded, float currentSpeed, bool boostingCurrently, float moveX, float moveY, bool accell, bool grinding)
    {
        UpdateGroundedState(grounded);
        UpdateJumpState(grounded);
        UpdateSpeedState(currentSpeed);
        UpdateHorizontalInput(moveX);
        UpdateBoostState(boostingCurrently);

        animator.SetFloat(SpeedMultiplierHash, currentSpeed);
        animator.SetBool(IsAccelHash, accell);
        animator.SetBool(IsGrindingHash, grinding);

        HandleBoredAnimation(moveX, moveY);
    }

    private void UpdateGroundedState(bool grounded)
    {
        if (prevGrounded == grounded) return;

        animator.SetBool(GroundedHash, grounded);
        prevGrounded = grounded;
    }

    private void UpdateJumpState(bool grounded)
    {
        bool jumping = playerMoving.AnimatorBoolJumping;
        if (prevJumping == jumping) return;

        animator.SetBool(JumpingHash, jumping);
        prevJumping = jumping;
    }

    private void UpdateSpeedState(float currentSpeed)
    {
        float speedNormalized = maxSpeed > 0f ? currentSpeed / maxSpeed : 0f;

        if (Mathf.Abs(prevSpeedNormalized - speedNormalized) <= AnimatorFloatEpsilon)
            return;

        animator.SetFloat(SpeedHash, speedNormalized);
        prevSpeedNormalized = speedNormalized;
    }

    private void UpdateHorizontalInput(float moveX)
    {
        float left = moveX < 0f ? -moveX : 0f;
        float right = moveX > 0f ? moveX : 0f;

        animator.SetFloat(LeftHash, left);
        animator.SetFloat(RightHash, right);


        float result = (moveX + 1f) * 0.5f;
        animator.SetFloat(LeftAndRightHash, result);

    }

    private void UpdateBoostState(bool boostingCurrently)
    {
        if (prevBoosting == boostingCurrently) return;

        animator.SetBool(BoostingHash, boostingCurrently);
        prevBoosting = boostingCurrently;
    }

    private void HandleBoredAnimation(float moveX, float moveY)
    {
        if (moveX == 0f && moveY == 0f)
        {
            boredTimer += Time.deltaTime;

            if (boredTimer >= nextBoredTime)
            {
                nextBoredTime = Random.Range(boredMinTime, boredMaxTime);

                int randomBored = Random.Range(0, 6);
                Bored(true, randomBored);

                boredTimer = 0f;
            }
        }
        else
        {
            boredTimer = 0f;
            Bored(false);
        }
    }

    public void Bored(bool boredBool, int boredNumber = 0)
    {
        animator.SetBool(BoredHash, boredBool);

        if (boredBool)
        {
            animator.SetInteger(BoredNumberHash, boredNumber);
        }
    }

    public void SetStartType(int type)
    {
        animator.SetInteger(StartTypeHash, type);
    }

    public void GameStartBoost()
    {
        playerMoving.StartScenAnimEnd(true);
    }

    public void GameStartNormal()
    {
        playerMoving.StartScenAnimEnd(false);
    }

    private int CurrentFootstepSound = 1;
    private string stepSound;
    public void FootstepAnimSoundPlayer()
    {
        if (!savedGrounded)
            return;

        UpdateFootstepSoundPrefix();

        if (soundLength <= 0 || string.IsNullOrEmpty(soundStartName))
            return;
        if (PlayerStepRandom == true)
        {
            int random = Random.Range(1, soundLength + 1);
            stepSound = soundStartName + random;
        }
        else
        {
            CurrentFootstepSound++;
            if (CurrentFootstepSound > soundLength)
                CurrentFootstepSound = 1;
            stepSound = soundStartName + CurrentFootstepSound;
        }

        footStepManagerAudio.PlayVolumeOfChoice(stepSound, StepSoundVolume);
    }

    private void UpdateFootstepSoundPrefix()
    {
        switch (groundTypeAnimator)
        {
            case GroundType.Concrete:
                soundLength = 6;
                soundStartName = "C";
                break;

            case GroundType.Dirt:
                soundLength = 5;
                soundStartName = "D";
                break;

            case GroundType.Grass:
                soundLength = 4;
                soundStartName = "G";
                break;

            case GroundType.Metal:
                soundLength = 5;
                soundStartName = "M";
                break;
            case GroundType.Snow:
                soundLength = 5;
                soundStartName = "S";
                break;
            case GroundType.Water:
                soundLength = 5;
                soundStartName = "Wa";
                break;
            case GroundType.Wood:
                soundLength = 4;
                soundStartName = "Wo";
                break;
        }
    }

    public void SetGroundType(GroundType newGroundType)
    {
        groundTypeAnimator = newGroundType;
    }

}
