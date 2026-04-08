using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {


    [SerializeField] private Transform Target;

    [SerializeField] private float DistanceFromTarget;
    [SerializeField] private float DistanceFromTargetUp;

    float lateUpdateTime;

    void Start () {
		
	}

    private void CheckIfAllIsAssigned()
    {
        if (Target == null)
            Debug.LogError("Target not assigned in CameraScript. Fixing for now."); Target = FindObjectOfType<PlayerMoving>().transform;
    }

    void LateUpdate () {

        float start = Time.realtimeSinceStartup; //Debug


        Vector3 forward =  Target.forward;
        // construct quaternion facing that direction and sample angles once
        Quaternion look = Quaternion.LookRotation(forward, Target.up);

        // compute position using rot without reading transform.eulerAngles
        Vector3 camPos = Target.position - forward * DistanceFromTarget + Target.up * DistanceFromTargetUp;

        // apply rotation and position (two writes, unavoidable) - Why are there comments here, why am I using AI to describe simple things? 
        transform.rotation = look;
        transform.position = camPos;

        lateUpdateTime = (Time.realtimeSinceStartup - start) * 1000f;
    }

    private float _uiTimerAcc;
    private const float UI_UPDATE_INTERVAL = 0.05f;

    private string _cachedLateUpdateLabel;
    void OnGUI()
    {
        _uiTimerAcc += Time.deltaTime;
        if (_uiTimerAcc >= UI_UPDATE_INTERVAL)
        {
            _cachedLateUpdateLabel = string.Format("CameraS:LateUpdate: {0:F2} ms", lateUpdateTime);
            _uiTimerAcc = 0f;
        }

        GUI.Label(new Rect(10, 90, 300, 20), _cachedLateUpdateLabel);
    }
}
