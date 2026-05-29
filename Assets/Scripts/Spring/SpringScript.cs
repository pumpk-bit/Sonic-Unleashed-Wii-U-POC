using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringScript : MonoBehaviour {
    [Header("Variables")]

    [SerializeField] bool RemovesSpeed = true;

    [Header("Spring")]
    [SerializeField] float SpringJumpHeight;
    [SerializeField] string SoundSFXSpring;
    [SerializeField] Animator Animator;

    [Header("Balloon")]
    [SerializeField] bool IsBalloon;
    [SerializeField] bool CanRespawn;
    [SerializeField] float BalloonJumpHeight;
    [SerializeField] string SoundSFXBalloon;

    [Header("LiftPad")]
    [SerializeField] bool IsLiftPad;
    [SerializeField] float BounceHeight;


    GameObject player;
    PlayerMoving PlayerMovingScript;

        void OnTriggerEnter(Collider other)
        {
        if (other.CompareTag("Player"))
        {
            var rb = other.GetComponentInParent<Rigidbody>();
            if (rb == null) return;

            var player = rb.GetComponentInChildren<PlayerMoving>();
            if (player == null) return;

            PlayerMovingScript = player;


            if (!IsBalloon && !IsLiftPad) Spring();
            if (IsBalloon) Balloon();
            if (IsLiftPad) LiftPad();
        }

    }

    private void Spring()
    {
        // This takes two inputs. The spring height and the soundeffect.
        PlayerMovingScript.Spring(SpringJumpHeight, SoundSFXSpring, transform.up, RemovesSpeed);
        Animator.Play("SpringAnim", 0, 0f);
    }
    private void Balloon()
    {
        PlayerMovingScript.Spring(BalloonJumpHeight, SoundSFXBalloon, transform.up, RemovesSpeed);
        if (!CanRespawn) DeleteCurrentObj();
        if (CanRespawn) Debug.Log("not added");
    }
    private void LiftPad()
    {
        PlayerMovingScript.Spring(BounceHeight, "None", transform.up, RemovesSpeed);
    }

    void DeleteCurrentObj()
    {
        gameObject.SetActive(false);
    }
}
