using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointScript : MonoBehaviour {

	[SerializeField] bool HasBeenUsed = false;
	[SerializeField] string CheckPointSfx;
    [SerializeField] GameObject Ribbon;

    [SerializeField] GameObject Arm1;
    [SerializeField] GameObject Arm2;

    void Start()
    {
        CheckIfAllIsAssigned();

        Ribbon.SetActive(true);

        Arm1.SetActive(false);
        Arm2.SetActive(false);
    }

    private void CheckIfAllIsAssigned()
    {
        // Things that can be auto-fixed:
        if (Arm1 == null)
            Debug.LogError("Arm1 not assigned in CheckPointScript. Cannot fix.");
        if (Arm2 == null)
            Debug.LogError("Arm2 not assigned in CheckPointScript. Cannot fix.");
        if (Ribbon == null)
            Debug.LogError("Ribbon not assigned in CheckPointScript. Cannot fix.");
    }


        void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
			if (HasBeenUsed == true) return;
            SetCheckPointToUsed();
            FindObjectOfType<PlayerRespawnManager>().SetNewRespawnPosition(transform);
			FindObjectOfType<AudioManager>().Play(CheckPointSfx);


        }

    }

    private void SetCheckPointToUsed()
    {
        HasBeenUsed = true;
        Ribbon.SetActive(false);
        Arm1.SetActive(true);
        Arm2.SetActive(true);
    }
}
