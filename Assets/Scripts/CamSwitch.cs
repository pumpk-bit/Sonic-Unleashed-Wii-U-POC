using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CamSwitch : MonoBehaviour {

    [SerializeField] bool ChangeToNoFollow;

    [SerializeField] bool CameraLooksTowardsSth;
    [SerializeField] Transform LookTowardsTransfom;


    GameObject player;
    PlayerMoving PlayerMovingScript;
    Newcam NewcamScript;

    void Start()
    {
        CheckIfAllIsAssigned();
    }

    private void CheckIfAllIsAssigned()
    {
        // Things that can be auto-fixed:
        if (LookTowardsTransfom == null)
            Debug.LogError("LookTowardsTransfom not assigned in CamSwitch. This is used for the direction where the cam points at. Cannot fix.");
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            PlayerMovingScript = player.gameObject.GetComponentInParent<PlayerMoving>();
            NewcamScript = PlayerMovingScript.CameraGameObject.GetComponent<Newcam>();

            if (ChangeToNoFollow == true && !CameraLooksTowardsSth)
            {
                ToggleOn();
            }
            if (ChangeToNoFollow == false &&!CameraLooksTowardsSth)
            {
                ToggleOff();
            }
            if (CameraLooksTowardsSth == true)
            {
                LookCamSth();
            }
            
        }

    }

    private void ToggleOn()
    {
        NewcamScript.SetCamToFollow(true);

        //Cam3D.SetCamToFollow(true);
       

    }
    private void ToggleOff()
    {
        NewcamScript.SetCamToFollow(false);

        //Cam3D.SetCamToFollow(false);
        

    }
    private void LookCamSth()
    {
        NewcamScript.SetCamToTowards(LookTowardsTransfom);
        //Cam3D.SetCamToTowards(LookTowardsTransfom);
    }
}
