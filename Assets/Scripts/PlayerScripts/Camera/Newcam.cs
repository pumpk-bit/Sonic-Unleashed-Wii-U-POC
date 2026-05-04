using System.Collections.Generic;
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

    private float _rotationY;
    private float _rotationX;
    private Vector2 _rotationXMinMax = new Vector2(-40, 40);

    private Vector3 nextRotation;

    float mouseX;
    float mouseY;

    [Header("3D")]
    [SerializeField] public bool ifIs3d;
    [SerializeField] public float CamDistance = 0.5f;
    [SerializeField] public Slider ApartSlider;
    [SerializeField] public Text Slidertext;

    [SerializeField] GameObject RightCam;
    [SerializeField] GameObject LeftCam;



    WiiU.GamePad gp = WiiU.GamePad.access;




    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        TargetLookTowards = Target;
    }
    void Update()
    {

        if (!IsMovable) { VictoryCam(); return; }
        PlayerInput();
        DampeningAndRotation();
    }
    void LateUpdate()
    {
        if (ifIs3d) Cam3D();
        else NormalCamera();
    }
    private void VictoryCam()
    {
        transform.position = Target.position - transform.forward;

        if (ifIs3d)
        {
            CamDistance = ApartSlider.value;
            Slidertext.text = CamDistance.ToString();

            RightCam.transform.position = new Vector3(transform.position.x + CamDistance, transform.position.y, transform.position.z);

            LeftCam.transform.position = new Vector3(transform.position.x - CamDistance, transform.position.y, transform.position.z);

        }
    }


    private Quaternion nexRotation;
    private Quaternion targetRotation;
    private void NormalCamera()
    {

        //Positioning behind the target
        transform.position = Target.position - transform.forward * DistanceFromTarget + transform.up * DistanceFromTargetUp;

    }

    private void PlayerInput()
    {
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

        _rotationY += mouseX;
        _rotationX += mouseY;

        //clamping x rotation 
        _rotationX = Mathf.Clamp(_rotationX, _rotationXMinMax.x, _rotationXMinMax.y);
        if (IsXInverted)
        {
            nextRotation = new Vector3(_rotationX, -_rotationY, Target.rotation.eulerAngles.z);
        }
        if (IsYInverted)
        {
            nextRotation = new Vector3(-_rotationX, _rotationY, Target.rotation.eulerAngles.z);
        }
        else
        {
            nextRotation = new Vector3(_rotationX, _rotationY, Target.rotation.eulerAngles.z);
        }

    }

    private void DampeningAndRotation()
    {
        //Dampening and the rotation
        if (CanPlayerMove == true)
        {
            nexRotation = Quaternion.Euler(nextRotation);
            transform.rotation = Quaternion.Slerp(transform.rotation, nexRotation, SmoothingTime * Time.deltaTime);
        }
        else
        {

            _rotationY = transform.rotation.eulerAngles.y;
            _rotationX = transform.rotation.eulerAngles.x;

            if (TargetLookTowards == Target)
            {
                Quaternion targetRotation = TargetLookTowards.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, SmoothingTime * Time.deltaTime);
            }
            else
            {

                if (LookTowardReverse == true) targetRotation = Quaternion.LookRotation(TargetLookTowards.forward, TargetLookTowards.up);
                else targetRotation = Quaternion.LookRotation(-TargetLookTowards.forward, TargetLookTowards.up);

                if (WhenReverseFlip) CheckAndFlipCamera();
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, SmoothingTime * Time.deltaTime);
            }

        }

    }

    private void Cam3D()
    {
        //Positioning behind the target
        transform.position = Target.position - transform.forward * DistanceFromTarget + transform.up * DistanceFromTargetUp;

        CamDistance = ApartSlider.value;
        Slidertext.text = CamDistance.ToString();

        RightCam.transform.position = new Vector3(transform.position.x + CamDistance, transform.position.y, transform.position.z);
        LeftCam.transform.position = new Vector3(transform.position.x - CamDistance, transform.position.y, transform.position.z);

    }

    private void CheckAndFlipCamera()
    {
        if (PlayerMoving.currentSpeed < SpeedUntilFlipCam) return;
        Vector3 playerDir = Target.forward.normalized;
        Vector3 lookDir = TargetLookTowards.forward.normalized;

        float dot = Vector3.Dot(playerDir, lookDir);
        LookTowardReverse = dot > 0f;
    }

    //Switch modes from other scripts
    public void SetCamToFollow(bool Follow)
    {
        _rotationY = transform.rotation.eulerAngles.y;
        _rotationX = transform.rotation.eulerAngles.x;
        CanPlayerMove = Follow;
        TargetLookTowards = Target;

    }
    public void SetCamToTowards(Transform Targetlook)
    {
        nexRotation = Target.rotation;
        nextRotation = Target.rotation.eulerAngles;
        CanPlayerMove = false;

        TargetLookTowards = Targetlook;
    }

}
