using System;
using System.Collections;
using System.Collections.Generic;
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


    float updateTime;
    void Update()
    {
        float start = Time.realtimeSinceStartup;

        if (CanMove == true)
        {
            transform.position = cameraPosition.position;


        }


        // LateUpdate logic

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
            _cachedLateUpdateLabel = string.Format("CamHolder:LateUpdate: {0:F2} ms", lateUpdateTime);
            _uiTimerAcc = 0f;
        }

        GUI.Label(new Rect(10, 70, 300, 20), _cachedLateUpdateLabel);
    }


}
