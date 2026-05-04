using System.Collections;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private HomingAttack homingAttackManager;
    [SerializeField] private PlayerMoving playerMoving;

    [Header("Boost")]
    [SerializeField] private float boostSpeed = 60f;
    [SerializeField] private float boostSpeedAir = 50f;
    [SerializeField] private bool boostReturnsOnGround = true;
    [SerializeField] private bool boostReturnsOnBounce = true;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 3.5f;
    [SerializeField] private bool jumpFromZero = false;

    [Header("Audio")]
    [SerializeField] private string jumpSFX;
    [SerializeField] private string jumpSFX2;
    [SerializeField] private string boostSFX;
    [SerializeField] private string boostAirSFX;

    // Cached state (mirrors PlayerMoving)
    private bool isBoosting;
    private bool canBoost;
    private bool canCheckGround;
    private bool grounded;
    private bool canMove = true;

    private float lastJumpTime;
    private float currentSpeed;
    private Vector3 normal;

    public void AbilitiesManager()
    {
        ReadPlayerState();
        WriteBackToPlayer();
    }

    private void ReadPlayerState()
    {
        canBoost = playerMoving.canBoost;
        canCheckGround = playerMoving.canCheckIfGrounded;
        lastJumpTime = playerMoving.lastJumpTime;
        grounded = playerMoving.grounded;
        currentSpeed = playerMoving.currentSpeed;
        normal = playerMoving.normal;
        isBoosting = playerMoving.isBoosting;

        if (boostReturnsOnGround && grounded)
            canBoost = true;
    }

    private void WriteBackToPlayer()
    {
        playerMoving.canBoost = canBoost;
        playerMoving.canCheckIfGrounded = canCheckGround;
        playerMoving.lastJumpTime = lastJumpTime;
        playerMoving.currentSpeed = currentSpeed;
        playerMoving.isBoosting = isBoosting;
    }

    public void Jump()
    {
        if (!canMove)
            return;

        if (!grounded)
        {
            homingAttackManager.HomingAttackManagerJump();
            return;
        }


        EnterBallState();
        playerMoving.canCheckIfGrounded = false;
        grounded = false;
        playerMoving.grounded = false;
        playerMoving.normal = Vector3.up;

        if (jumpFromZero)
        {
            float velAlongNormal = Vector3.Dot(rb.velocity, normal);
            rb.velocity -= normal * velAlongNormal;
        }

        rb.AddForce(normal * jumpForce * 10f, ForceMode.Impulse);

        PlaySound(jumpSFX);
        PlaySound(jumpSFX2);

        grounded = false;
        lastJumpTime = Time.time;

        StartCoroutine(ReenableGroundCheck(1f));
    }

    public void Boost()
    {
        if (!canMove || !canBoost)
            return;

        Vector3 direction = playerTransform.forward.normalized;

        if (grounded)
        {
            rb.AddForce(direction * boostSpeed, ForceMode.Impulse);
            isBoosting = true;
            PlaySound(boostSFX);
        }
        else
        {
            rb.AddForce(direction * boostSpeedAir, ForceMode.Impulse);
            PlaySound(boostAirSFX);
            canBoost = false;
        }

        LimitBoostSpeed(direction);
    }

    private void LimitBoostSpeed(Vector3 direction)
    {
        float forwardSpeed = Vector3.Dot(rb.velocity, playerTransform.forward);

        if (forwardSpeed >= boostSpeed && canBoost)
        {
            rb.velocity = direction * boostSpeed;
            currentSpeed = boostSpeed;
        }
    }

    public void Spring(float force, string sound, Vector3 direction, bool resetVelocity)
    {
        canCheckGround = false;

        if (homingAttackManager.IsHoming)
            homingAttackManager.StopHomingCoroutine();

        if (resetVelocity)
            SetVelocityToZero();

        rb.AddForce(direction.normalized * force * 10f, ForceMode.Impulse);
        grounded = false;

        PlaySound(sound);

        if (boostReturnsOnBounce)
            canBoost = true;

        StartCoroutine(ReenableGroundCheck(1f));
    }

    private IEnumerator ReenableGroundCheck(float delay)
    {
        yield return new WaitForSeconds(delay);
        playerMoving.canCheckIfGrounded = true;
    }

    private void EnterBallState()
    {
        playerMoving.isPlayerABall = true;
    }

    private void SetVelocityToZero()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void PlaySound(string sound)
    {
        if (!string.IsNullOrEmpty(sound) && sound != "None" && audioManager != null)
        {
            audioManager.Play(sound);
        }
    }
}
