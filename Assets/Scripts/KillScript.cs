using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class KillScript : MonoBehaviour {

    void OnTriggerEnter(Collider other)
    {
        GameObject player;
        if (other.CompareTag("Player"))
        {
            player = other.transform.root.gameObject;
            FindObjectOfType<Scenemanager>().EndPlayer(player);
        }

    }
}
