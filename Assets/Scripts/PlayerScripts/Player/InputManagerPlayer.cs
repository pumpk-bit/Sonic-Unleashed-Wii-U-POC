using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WiiU = UnityEngine.WiiU;

public class InputManagerPlayer : MonoBehaviour {

    WiiU.GamePad gp = WiiU.GamePad.access;

    [SerializeField] PlayerMoving PlayerMoving;
    [SerializeField] Scenemanager cachedGameManager;

    [SerializeField] private bool ControllsPC;
    [SerializeField] private bool ControllsWiiU;

    [SerializeField] private bool isWiiUWiiMote;
    [SerializeField] private bool isWiiUGamepad;

    [SerializeField] private bool isaprefab;


    void Start()
    {
        CheckIfAllIsAssigned();
    }
    private void CheckIfAllIsAssigned()
    {
        if (PlayerMoving == null)
            Debug.LogError("playerMovingScript not assigned in InputManagerPlayer. Fixing for now."); PlayerMoving = GetComponent<PlayerMoving>();
        if (cachedGameManager == null)
            Debug.LogError("cachedGameManager not assigned in InputManagerPlayer. Fixing for now."); cachedGameManager = FindObjectOfType<Scenemanager>();

    }
    public void ControllsManager()
    {
        if (isaprefab) return;
        if (ControllsPC) PlayerControllsPC();
        if (ControllsWiiU) PlayerControllsWiiU();
    }

    private float moveX;
    private float moveY;
    public void PlayerControllsPC()
    {
        //Pc
        if ((Input.GetKeyDown("space")))
        {
            PlayerMoving.Jump();
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

            if (state.IsTriggered(WiiU.GamePadButton.A))
            {
                PlayerMoving.Jump();
            }

            if (state.IsPressed(WiiU.GamePadButton.X))
            {
                //Core.homeMenuEnabled = false;
                PlayerMoving.Boost();
            }
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



    private void UpdateTheOtherSide()
    {
        PlayerMoving.moveX = moveX;
        PlayerMoving.moveY = moveY;
    }


}

