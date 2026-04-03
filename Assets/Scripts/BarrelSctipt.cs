using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BarrelSctipt : MonoBehaviour {

    [Header("AudioManager")]
    [SerializeField] public AudioManager AudioManager;
    [SerializeField] string BarrelBreakSFX;
    [Header("PlayerScript")]
    [SerializeField] public PlayerMoving PlayerMoving;

    void Start()
    {
        AudioManager = FindObjectOfType<AudioManager>();
        if (PlayerMoving == null) PlayerMoving = FindObjectOfType<PlayerMoving>();

    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && PlayerMoving.BallorNot == "Ball")
        {
            Break();
        }

    }

    private void Break()
    {
        AudioManager.Play(BarrelBreakSFX);
        Destroy(gameObject);
    }
}
