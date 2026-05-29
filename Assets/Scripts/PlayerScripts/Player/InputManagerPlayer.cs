using System.Collections.Generic;
using UnityEngine;
using WiiU = UnityEngine.WiiU;

public class InputManagerPlayer : MonoBehaviour {

    WiiU.GamePad gp = WiiU.GamePad.access;

    [SerializeField] PlayerMoving PlayerMoving;
    [SerializeField] private bool ControllsPC;

    //WiiU - main
    [SerializeField] private bool ControllsWiiU;
    //Sub
    [SerializeField] private bool isWiiUWiiMote;
    [SerializeField] private bool isWiiUGamepad;

    //Networking
    [SerializeField] private bool isaprefab;

    //Debug
    [SerializeField] private bool debugXboxOneController;


    void Start()
    {
        CheckIfAllIsAssigned();
    }
    private void CheckIfAllIsAssigned()
    {
        if (PlayerMoving == null)
            Debug.LogError("playerMovingScript not assigned in InputManagerPlayer. Fixing for now."); PlayerMoving = FindObjectOfType<PlayerMoving>();
    }
    public void ControllsManager()
    {
        if (isaprefab) return;
        if (ControllsPC) PlayerControllsPC();
        if (ControllsWiiU) PlayerControllsWiiU();
        if (debugXboxOneController) DebugXboxOneController();
    }


   

    private float moveX;
    private float moveY;
    public void PlayerControllsPC()
    {
        //Pc
        if ((Input.GetKeyDown("space")))
        {
            JumpAction();
        }

        if (Input.GetKey("f"))
        {
            PlayerMoving.Boost();
        }
        if (Input.GetKeyUp("f"))
        {
           PlayerMoving.isBoosting = false;
        }

        if (Input.GetKey("x"))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        WalkingMentPC();

        if (Input.GetKey("s") == false && Input.GetKey("w") == false)
        {
            moveY = 0;
        }
        if (Input.GetKey("a") == false && Input.GetKey("d") == false)
        {
            moveX = 0;
        }

        if (Input.GetKeyDown("r"))
        {

        }


        UpdateTheOtherSide();
    }

    private void WalkingMentPC()
    {
        if (Input.GetKey("d")) moveX = 0.5f;

        if (Input.GetKey("a")) moveX = -0.5f;

        if (Input.GetKey("d") && Input.GetKey(KeyCode.LeftShift)) moveX = 1f;

        if (Input.GetKey("a") && Input.GetKey(KeyCode.LeftShift)) moveX = -1f;


        if (Input.GetKey("w")) moveY = 0.5f;

        if (Input.GetKey("s")) moveY = -0.5f;

        if (Input.GetKey("w") && Input.GetKey(KeyCode.LeftShift)) moveY = 1f;

        if (Input.GetKey("s") && Input.GetKey(KeyCode.LeftShift)) moveY = -1f;

    }


    public void PlayerControllsWiiU()
    {
        if (isWiiUGamepad) WiiUGamepad();
        if (isWiiUWiiMote) WiiUWiiRemote();
    }

    private void WiiUGamepad()
    {
        WiiU.GamePadState state = gp.state;

        if (state.gamePadErr == WiiU.GamePadError.None)
        {
            moveX = state.lStick.x;
            moveY = state.lStick.y;

            if (state.IsTriggered(WiiU.GamePadButton.B)) JumpAction();

            if (state.IsPressed(WiiU.GamePadButton.Y)) BoostAction();

            if (state.IsReleased(WiiU.GamePadButton.X))
            {
                PlayerMoving.isBoosting = false;
            }

            if (state.IsTriggered(WiiU.GamePadButton.Y))
            {
                //Core.homeMenuEnabled = true;
                //cachedGameManager.EndPlayer(true);
            }


            if (state.IsTriggered(WiiU.GamePadButton.ZR))
            {

            }
            if (state.IsTriggered(WiiU.GamePadButton.ZL))
            {

            }


            if (state.IsTriggered(WiiU.GamePadButton.Plus))
            {


            }
            if (state.IsTriggered(WiiU.GamePadButton.Minus))
            {

            }

            if (state.IsTriggered(WiiU.GamePadButton.Up))
            {

            }
            if (state.IsTriggered(WiiU.GamePadButton.Down))
            {

            }

            UpdateTheOtherSide();
        }
    }

    public int channel;
    private void WiiUWiiRemote()
    {
        // Querying the first Wii U Remote, it should have Nunchuk attached.
        WiiU.RemoteState remote = WiiU.Remote.Access(0).state;
        if (remote.devType == WiiU.RemoteDevType.Nunchuk || remote.devType == WiiU.RemoteDevType.MotionPlusNunchuk)
        {
            moveX = remote.nunchuk.stick.x;
            moveY = remote.nunchuk.stick.y;

            if (remote.IsTriggered(WiiU.RemoteButton.A))
            {
                PlayerMoving.Jump();
            }


            if (remote.IsTriggered(WiiU.RemoteButton.B))
            {
                PlayerMoving.Boost();
            }
            if (remote.IsReleased(WiiU.RemoteButton.B))
            {
                PlayerMoving.isBoosting = false;
            }


            UpdateTheOtherSide();

        }
    }

    private void DebugXboxOneController()
    {
         moveX = Input.GetAxis("Horizontal");
         moveY = Input.GetAxis("Vertical");
        //0 = A, 1 = B, 2 = X, 3 = 4 JoystickButton4 --  LB JoystickButton5 -- RB
        if (Input.GetKeyDown(KeyCode.JoystickButton0)) JumpAction();
        if (Input.GetKeyDown(KeyCode.JoystickButton2)) BoostAction(); //


        UpdateTheOtherSide();
    }



    private void JumpAction()
    {
        PlayerMoving.Jump();

    }
    private void BoostAction()
    {
        PlayerMoving.Boost();

    }
    private void UpdateTheOtherSide()
    {
        PlayerMoving.moveX = moveX;
        PlayerMoving.moveY = moveY;
    }


}

