using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WiiU = UnityEngine.WiiU;

public class InputManagerPlayer : MonoBehaviour {

    WiiU.GamePad gp = WiiU.GamePad.access;

    [SerializeField] PlayerMoving playerMovingScript;
    [SerializeField] Scenemanager cachedGameManager;

    [SerializeField] private bool ControllsPC;
    [SerializeField] private bool ControllsWiiU;

    [SerializeField] private bool isWiiUWiiMote;
    [SerializeField] private bool isWiiUGamepad;

    void Start()
    {
        CheckIfAllIsAssigned();
    }
    private void CheckIfAllIsAssigned()
    {
        if (playerMovingScript == null)
            Debug.LogError("playerMovingScript not assigned in InputManagerPlayer. Fixing for now."); playerMovingScript = GetComponent<PlayerMoving>();
        if (cachedGameManager == null)
            Debug.LogError("cachedGameManager not assigned in InputManagerPlayer. Fixing for now."); cachedGameManager = FindObjectOfType<Scenemanager>();

    }
    public void ControllsManager()
    {
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
            playerMovingScript.Jump();
        }

        if (Input.GetKey("f"))
        {
            playerMovingScript.Boost();
        }
        if (Input.GetKeyUp("f"))
        {
            playerMovingScript.BoostingCurrently = false;
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
            //cachedGameManager.EndPlayer(true);
            cachedGameManager.RestartLevel();


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
                playerMovingScript.Jump();
            }

            if (state.IsPressed(WiiU.GamePadButton.X))
            {
                //Core.homeMenuEnabled = false;
                playerMovingScript.Boost();
            }
            if (state.IsReleased(WiiU.GamePadButton.X))
            {
                playerMovingScript.BoostingCurrently = false;
            }

            if (state.IsTriggered(WiiU.GamePadButton.Y))
            {
                //Core.homeMenuEnabled = true;
                //cachedGameManager.EndPlayer(true);
            }
            if (state.IsTriggered(WiiU.GamePadButton.ZR))
            {
                Scene current = SceneManager.GetActiveScene();
                int nextScene = current.buildIndex + 1;
                SceneManager.LoadScene(nextScene);
            }
            if (state.IsTriggered(WiiU.GamePadButton.ZL))
            {
                Scene current = SceneManager.GetActiveScene();
                int nextScene = current.buildIndex - 1;
                SceneManager.LoadScene(nextScene);
            }
            if (state.IsTriggered(WiiU.GamePadButton.Plus))
            {
                cachedGameManager.RestartLevel();
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
                playerMovingScript.Jump();
            }


            if (remote.IsTriggered(WiiU.RemoteButton.B))
            {
                playerMovingScript.Boost();
            }
            if (remote.IsReleased(WiiU.RemoteButton.B))
            {
                playerMovingScript.BoostingCurrently = false;
            }
            if (remote.IsPressed(WiiU.RemoteButton.Plus))
            {
                cachedGameManager.RestartLevel();
            }


            UpdateTheOtherSide();

        }
    }



    private void UpdateTheOtherSide()
    {
        playerMovingScript.moveX = moveX;
        playerMovingScript.moveY = moveY;
    }


}

