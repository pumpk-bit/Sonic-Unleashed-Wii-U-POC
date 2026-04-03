using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {


    [SerializeField] private Transform Target;

    [SerializeField] private float DistanceFromTarget;
    [SerializeField] private float DistanceFromTargetUp;

    void Start () {
		
	}

    private void CheckIfAllIsAssigned()
    {
        if (Target == null)
            Debug.LogError("Target not assigned in CameraScript. Fixing for now."); Target = FindObjectOfType<PlayerMoving>().transform;
    }

    // Update is called once per frame
    void LateUpdate () {

        Vector3 forward =  Target.forward;
        // construct quaternion facing that direction and sample angles once
        Quaternion look = Quaternion.LookRotation(forward, Target.up);

        // compute position using rot without reading transform.eulerAngles
        Vector3 camPos = Target.position - forward * DistanceFromTarget + Target.up * DistanceFromTargetUp;

        // apply rotation and position (two writes, unavoidable)
        transform.rotation = look;
        transform.position = camPos;

    }
}
