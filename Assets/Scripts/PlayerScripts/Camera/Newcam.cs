using UnityEngine;
using WiiU = UnityEngine.WiiU;

public class Newcam : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMoving playerMoving;
    [SerializeField] private Transform target;

    public enum CameraMode
    {
        FollowingTarget,
        PlayerControll,
        FreeCam
    }
    public CameraMode currentMode;

    [Header("Camera Settings")]
    [SerializeField] private float sensitivity = 3f;
    [SerializeField] private float smoothingSpeed = 10f;

    [Header("Distance")]
    [SerializeField] private float distanceFromTarget = 6f;
    [SerializeField] private float heightOffset = 2f;

    [Header("Rotation")]
    [SerializeField] private bool invertX;
    [SerializeField] private bool invertY;

    [SerializeField] private Vector2 pitchLimits = new Vector2(-40f, 40f);

    [Header("Look Toward")]
    [SerializeField] private Transform targetLookTowards;
    [SerializeField] private bool reverseLookDirection;
    [SerializeField] private bool autoFlipReverse;
    [SerializeField] private float speedUntilFlipCam = 20f;

   // private WiiU.GamePad gamePad = WiiU.GamePad.access;

    private float yaw;
    private float pitch;

    private float inputX;
    private float inputY;

    private Vector3 targetEuler;
    private Quaternion targetRotation;
    private CameraHolderScript CameraHolderScript;
    private bool CanCameraHolderMove;


    private void Start()
    {
        CameraHolderScript = FindObjectOfType<CameraHolderScript>();
        CameraHolderScript.CanMove = true;
        Cursor.lockState = CursorLockMode.Locked;

        if (targetLookTowards == null)
        {
            targetLookTowards = target;
        }

        Vector3 startRotation = transform.rotation.eulerAngles;

        yaw = startRotation.y;
        pitch = startRotation.x;
    }

    private void Update()
    {
        if (currentMode == CameraMode.FreeCam) return;
        else
        {
            if (CanCameraHolderMove == false)
            {
                CameraHolderScript.CanMove = true;
                CanCameraHolderMove = CameraHolderScript.CanMove;
            }

            ReadInput();
            UpdateRotation();
        }

    }

    private void LateUpdate()
    {
        if (currentMode == CameraMode.FreeCam)
        {
            if (CanCameraHolderMove == true)
            {
                CameraHolderScript.CanMove = false;
                CanCameraHolderMove = CameraHolderScript.CanMove;
            }

            FreeCam();
        }


        else UpdatePosition();
    }


    private void UpdatePosition()
    {
        transform.position =target.position- transform.forward * distanceFromTarget+ Vector3.up * heightOffset;
    }

    private void ReadInput()
    {
        inputX = 0f;
        inputY = 0f;

        inputX *= sensitivity * Time.deltaTime;
        inputY *= sensitivity * Time.deltaTime;

        if (invertX)
            inputX *= -1f;

        if (invertY)
            inputY *= -1f;

        yaw += inputX;
        pitch += inputY;

        pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);

        targetEuler = new Vector3(pitch, yaw, 0f);
    }

    private void UpdateRotation()
    {
        if (currentMode == CameraMode.PlayerControll)
        {
            targetRotation = Quaternion.Euler(targetEuler);
        }
        else
        {
            HandleLookTowardRotation();
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothingSpeed * Time.deltaTime
        );
    }

    private void HandleLookTowardRotation()
    {
        if (targetLookTowards == target)
        {
            targetRotation = target.rotation;
            return;
        }

        Vector3 direction =reverseLookDirection? targetLookTowards.forward: -targetLookTowards.forward;

        targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        if (autoFlipReverse)
        {
            CheckAndFlipCamera();
        }

        yaw = transform.rotation.eulerAngles.y;
        pitch = transform.rotation.eulerAngles.x;
    }

    private void CheckAndFlipCamera()
    {
        if (playerMoving.currentSpeed < speedUntilFlipCam)
            return;

        Vector3 playerDirection = target.forward.normalized;
        Vector3 lookDirection = targetLookTowards.forward.normalized;

        float dot = Vector3.Dot(playerDirection, lookDirection);

        reverseLookDirection = dot > 0f;
    }

    public void SetCamToFollow(bool followPlayerInput)
    {
        if (followPlayerInput == true) currentMode = CameraMode.PlayerControll;
        else currentMode = CameraMode.FollowingTarget;
        targetLookTowards = target;

        yaw = transform.rotation.eulerAngles.y;
        pitch = transform.rotation.eulerAngles.x;
    }

    public void SetCamToTowards(Transform newTarget)
    {
        currentMode = CameraMode.FollowingTarget;

        targetLookTowards = newTarget;

        targetRotation = target.rotation;
        targetEuler = target.rotation.eulerAngles;
    }


    public float movementSpeed = 2f;
    public float fastMovementSpeed = 5f;
    public float freeLookSensitivity = 2f;
    public float zoomSensitivity = 2f;
    public float fastZoomSensitivity = 5f;

    private bool looking = true;

    WiiU.GamePad gp = WiiU.GamePad.access;
    float moveX;
    float moveY;

    void FreeCam()
    {
        WiiU.GamePadState state = gp.state;

        if (state.gamePadErr == WiiU.GamePadError.None)
        {
            // 1. Gather Joystick Inputs
            moveX = state.lStick.x;
            moveY = state.lStick.y;

            // Determine speed modifier
            var fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var movementSpeed = fastMode ? this.fastMovementSpeed : this.movementSpeed;

            // 2. Apply GamePad Left Stick Movement
            // Horizontal movement (Left/Right)
            if (Mathf.Abs(moveX) > 0.1f) // 0.1f acts as a small deadzone
            {
                transform.position += transform.right * moveX * movementSpeed * Time.deltaTime;
            }
            // Vertical movement (Forward/Backward)
            if (Mathf.Abs(moveY) > 0.1f)
            {
                transform.position += transform.forward * moveY * movementSpeed * Time.deltaTime;
            }

            // 3. Keyboard Movement Overrides/Additions
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                transform.position += -transform.right * movementSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                transform.position += transform.right * movementSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                transform.position += transform.forward * movementSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                transform.position += -transform.forward * movementSpeed * Time.deltaTime;

            // Vertical Up/Down Controls (Keyboard)
            if (Input.GetKey(KeyCode.Q))
                transform.position += transform.up * movementSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.E))
                transform.position += -transform.up * movementSpeed * Time.deltaTime;

            // 4. Camera Rotation (Mouse Look OR GamePad Right Stick Look)
            float gamepadLookX = state.rStick.x;
            float gamepadLookY = state.rStick.y;
            bool isGamepadLooking = Mathf.Abs(gamepadLookX) > 0.1f || Mathf.Abs(gamepadLookY) > 0.1f;

            if (looking || isGamepadLooking)
            {
                // Combine Mouse and GamePad inputs if both are active, otherwise it uses whichever is moving
                float inputX = Input.GetAxis("Mouse X") + (gamepadLookX * 2f); // Multiplied by 2f to adjust sensitivity if needed
                float inputY = Input.GetAxis("Mouse Y") + (gamepadLookY * 2f);

                float newRotationX = transform.localEulerAngles.y + inputX * freeLookSensitivity;
                float newRotationY = transform.localEulerAngles.x - inputY * freeLookSensitivity;
                transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
            }

            // 5. Zoom / Scroll Wheel
            float axis = Input.GetAxis("Mouse ScrollWheel");
            if (axis != 0)
            {
                var zoomSensitivity = fastMode ? this.fastZoomSensitivity : this.zoomSensitivity;
                transform.position += transform.forward * axis * zoomSensitivity;
            }

            // Mouse Look Toggle
            if (Input.GetKeyDown(KeyCode.Mouse1))
                StartLooking();
            else if (Input.GetKeyUp(KeyCode.Mouse1))
                StopLooking();
        }
    }

    void OnDisable()
    {
        StopLooking();
    }

    public void StartLooking()
    {
        looking = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void StopLooking()
    {
        looking = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

}
