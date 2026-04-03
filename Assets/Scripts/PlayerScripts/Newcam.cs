using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.WiiU;
using WiiU = UnityEngine.WiiU;
using UnityEngine.UI;

public class Newcam : MonoBehaviour
{
    [Header("Victory")]
    [SerializeField] public bool IsMovable = true;

    [Header("Player")]
    [SerializeField] public PlayerMoving PlayerMoving;
    [Header("Camera")]
    [SerializeField] public bool CanPlayerMove = true;

    [SerializeField] private float Sensitivity = 3.0f;
    [SerializeField] private float SmoothingTime = 0.2f;


    [SerializeField] public Transform Target;

    [SerializeField] private Transform TargetLookTowards;
    [SerializeField] private float SpeedUntilFlipCam;
    [SerializeField] public bool LookTowardReverse;
    [SerializeField] private bool WhenReverseFlip;

    [SerializeField] public float DistanceFromTarget;
    [SerializeField] public float DistanceFromTargetUp;

    [SerializeField] bool IsXInverted;
    [SerializeField] bool IsYInverted;

    // current/applied rotation angles (degrees)
    private float _currentYaw;
    private float _currentPitch;

    // desired rotation angles (degrees)
    private float _targetYaw;
    private float _targetPitch;

    private Vector2 _rotationXMinMax = new Vector2(-40, 40);

    private Vector3 nextRotation;

    float mouseX;
    float mouseY;

    WiiU.GamePad gp = WiiU.GamePad.access;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        TargetLookTowards = Target;

        // sample initial rotation once
        Vector3 e = transform.rotation.eulerAngles;
        _currentYaw = _targetYaw = e.y;
        _currentPitch = _targetPitch = e.x;
    }

    void Update()
    {
        if (!IsMovable) { VictoryCam(); return; }

        ReadInputAndSetTargets();
    }

    void LateUpdate()
    {
        if (Mathf.Abs(_currentYaw - _targetYaw) < 0.001f &&
            Mathf.Abs(_currentPitch - _targetPitch) < 0.001f)
            return;

        // simple float lerp for yaw/pitch (no Quaternion.Slerp)

        float t = SmoothingTime * Time.deltaTime;
        _currentYaw = Mathf.LerpAngle(_currentYaw, _targetYaw, t);
        _currentPitch = Mathf.Lerp(_currentPitch, _targetPitch, t);

        // compute final rotation quaternion once from cached angles
        Quaternion rot = Quaternion.Euler(_currentPitch, _currentYaw, 0f);

        // compute position using rot without reading transform.eulerAngles
        Vector3 forward = Target.forward;
        Vector3 up =  Target.up;
        Vector3 camPos = Target.position - forward * DistanceFromTarget + up * DistanceFromTargetUp;

        // apply rotation and position (two writes, unavoidable)
        transform.rotation = rot;
        transform.position = camPos;
    }

    private void VictoryCam()
    {
        // simple fixed offset behind current forward (use existing transform.forward once)
        // compute desired rot from current cached angles to keep behavior consistent
        Quaternion rot = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        Vector3 forward = rot * Vector3.forward;

       // Vector3 camPos = Target.position - forward;
       // transform.rotation = rot;
       // transform.position = camPos;


        transform.rotation = rot;

        Vector3 camPos =
            Target.position
            - transform.forward * DistanceFromTarget
            + transform.up * DistanceFromTargetUp;

        transform.position = camPos;

    }

    private void ReadInputAndSetTargets()
    {
        // Read controller / mouse input
        WiiU.GamePadState state = gp.state;

        if (state.gamePadErr == WiiU.GamePadError.None)
        {
            mouseX = state.rStick.x * Sensitivity * Time.deltaTime;
            mouseY = state.rStick.y * Sensitivity * Time.deltaTime;
            if (state.IsTriggered(WiiU.GamePadButton.Up))
            {
                Sensitivity++;
            }
            if (state.IsTriggered(WiiU.GamePadButton.Down))
            {
                Sensitivity--;
            }
        }

#if UNITY_EDITOR
        mouseX = Input.GetAxis("Mouse X") * Sensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * Sensitivity * Time.deltaTime;
#endif

        if (CanPlayerMove)
        {
            // update target yaw/pitch using simple float math; avoid reading eulerAngles
            float xSign = IsXInverted ? -1f : 1f;
            float ySign = IsYInverted ? -1f : 1f;

            _targetYaw += mouseX * 1f * xSign;
            _targetPitch += mouseY * 1f * ySign;

            // clamp pitch
            _targetPitch = Mathf.Clamp(_targetPitch, _rotationXMinMax.x, _rotationXMinMax.y);
        }
        else
        {
            // when camera is controlled by other targets, derive the target angles once per update,
            // using Quaternion.LookRotation (no manual trig).
            if (TargetLookTowards == Target)
            {
                // follow target rotation directly
                Quaternion tRot = TargetLookTowards.rotation;
                Vector3 te = tRot.eulerAngles; // single euler read when needed
                _targetYaw = te.y;
                _targetPitch = te.x;
            }
            else
            {
                if (WhenReverseFlip) CheckAndFlipCamera();

                Vector3 forward = LookTowardReverse ? TargetLookTowards.forward : -TargetLookTowards.forward;
                // construct quaternion facing that direction and sample angles once
                Quaternion look = Quaternion.LookRotation(forward, TargetLookTowards.up);
                Vector3 le = look.eulerAngles;
                _targetYaw = le.y;
                _targetPitch = Mathf.Clamp(le.x, _rotationXMinMax.x, _rotationXMinMax.y);
            }
        }
    }


    private void CheckAndFlipCamera()
    {

        return;



        if (PlayerMoving.currentSpeed < SpeedUntilFlipCam) return;

        Vector3 playerDir = Target.forward;
        Vector3 lookDir = TargetLookTowards.forward;

        //float dot = Vector3.Dot(playerDir.normalized, lookDir.normalized);

        float dot = Vector3.Dot(playerDir, lookDir);
        LookTowardReverse = dot > 0f;
    }

    //Switch modes from other scripts
    public void SetCamToFollow(bool Follow)
    {

        return;




        // sample current applied rotation once
        Vector3 e = transform.rotation.eulerAngles;
        _currentYaw = _targetYaw = e.y;
        _currentPitch = _targetPitch = e.x;

        CanPlayerMove = Follow;
        TargetLookTowards = Target;
    }
    public void SetCamToTowards(Transform Targetlook)
    {
        return;




        // sample target rotation once and store as targets
        Vector3 te = Target.rotation.eulerAngles;
        _targetYaw = te.y;
        _targetPitch = te.x;
        CanPlayerMove = false;

        TargetLookTowards = Targetlook;
    }
}

