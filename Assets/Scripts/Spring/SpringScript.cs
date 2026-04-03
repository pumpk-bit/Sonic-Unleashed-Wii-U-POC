using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringScript : MonoBehaviour {
    [Header("Variables")]

    [SerializeField] bool RemovesSpeed = true;


    [SerializeField] float SpringJumpHeight;
    [SerializeField] string SoundSFXSpring;

    [SerializeField] bool IsBalloon;
    [SerializeField] bool CanRespawn;
    [SerializeField] float BalloonJumpHeight;
    [SerializeField] string SoundSFXBalloon;

    [SerializeField] bool IsLiftPad;
    [SerializeField] float BounceHeight;


    GameObject player;
    PlayerMoving PlayerMovingScript;

        void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            PlayerMovingScript = player.gameObject.GetComponentInParent<PlayerMoving>();


            if (!IsBalloon && !IsLiftPad)
            {
                // This takes two inputs. The spring height and the soundeffect.
                PlayerMovingScript.Spring(SpringJumpHeight, SoundSFXSpring, transform.up, RemovesSpeed);
            }
            if (IsBalloon)
            {
                PlayerMovingScript.Spring(BalloonJumpHeight, SoundSFXBalloon, transform.up, RemovesSpeed);
                if(!CanRespawn) DeleteCurrentObj();
                if (CanRespawn) Debug.Log("not added");

            }
            if (IsLiftPad)
            {
                PlayerMovingScript.Spring(BounceHeight, "None", transform.up, RemovesSpeed);
            }
        }

    }

    void DeleteCurrentObj()
    {
        gameObject.SetActive(false);
    }
}
