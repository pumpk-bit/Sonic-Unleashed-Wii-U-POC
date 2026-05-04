
using UnityEngine;

public class CameraHolderScript : MonoBehaviour {

    public Transform cameraPosition;
    public bool CanMove;

    float lateUpdateTime;

    void Start()
    {
        CanMove = true;
    }

    private void CheckIfAllIsAssigned()
    {
           if (cameraPosition == null)
            Debug.LogError("CameraPosition not assigned in CameraHolderScript. Cannot Fix. CamHolder should be the parrent of cam.");
    }


    void Update()
    {

        if (CanMove == true)
        {
            transform.position = cameraPosition.position;


        }


    }

}
