using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EventHanlder : MonoBehaviour
{

    public enum ActionTypeEventHanlder
    {
        CamSwitch,
        KillPlayerInsta,
        LoadingAndUnloading,
        LoadingAndUnloadingScenes,
        Music,
        SwitchGroundType,
        SwitchTo2Dor3D,
    }

    public ActionTypeEventHanlder _actionTypeEventHanlder;

    //CamSwitch options:
    public bool ChangeToNoFollow;
    public bool CameraLooksTowardsSth;
    public Transform LookTowardsTransfom;

    //LoadingAndUnloading options:
    public enum Mode
    {
        Load,
        Unload
    }
    public Mode mode;
    public GameObject[] objects;

    //LoadingAndUnloadingScenes options:

    public enum ModeScene
    {
        Load,
        Unload
    }

    public ModeScene modeScene;
    public string[] LevelNameStrings;
    public Scenemanager Scenemanager;

    //MusicManager

    public enum ModeMusic
    {
        Play,
        ReturnLevelMusic
    }
    public ModeMusic modeMusic;
    public string MusicName;
    public bool OneTimeUseM;
    private bool hasBeenUsedM = false;

    //SwitchGroundType options:
    public PlayerAnimatorScript PlayerAnimatorScript;
    public PlayerAnimatorScript.GroundType Choosetype;

    //3d 2d

    public enum SwitchType2d
    {
        If3D,
        If2D
    }
    public SwitchType2d Is3dor2d;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            switch (_actionTypeEventHanlder)
            {
                case ActionTypeEventHanlder.CamSwitch:
                    CamSwitch(other);
                    break;
                case ActionTypeEventHanlder.KillPlayerInsta:
                    KillPlayer(other);
                    break;
                case ActionTypeEventHanlder.LoadingAndUnloading:
                    LoadingAndUnloading();
                    break;
                case ActionTypeEventHanlder.LoadingAndUnloadingScenes:
                    LoadingAndUnloadingScenes();
                    break;
                case ActionTypeEventHanlder.Music:
                    Music();
                    break;
                case ActionTypeEventHanlder.SwitchGroundType:
                    SwitchGroundType(other);
                    break;
                case ActionTypeEventHanlder.SwitchTo2Dor3D:
                    SwitchTo2Dor3Dhand(other);
                    break;

            }
        }

    }

    PlayerMoving PlayerMovingScript;
    Newcam NewcamScript;
    private void CamSwitch(Collider other)
    {

        var rb = other.GetComponentInParent<Rigidbody>();
        if (rb == null) return;

        var player = rb.GetComponentInChildren<PlayerMoving>();
        PlayerMovingScript = player;

        NewcamScript = PlayerMovingScript.Camera.GetComponent<Newcam>();

        if (ChangeToNoFollow == true && !CameraLooksTowardsSth)
        {
            ToggleOn();
        }
        if (ChangeToNoFollow == false && !CameraLooksTowardsSth)
        {
            ToggleOff();
        }
        if (CameraLooksTowardsSth == true)
        {
            if (LookTowardsTransfom == null) return;
            LookCamSth();
        }
    }
    #region CamSwitchFunctions
    private void ToggleOn()
    {
        NewcamScript.SetCamToFollow(true);
    }
    private void ToggleOff()
    {
        NewcamScript.SetCamToFollow(false);
    }
    private void LookCamSth()
    {
        NewcamScript.SetCamToTowards(LookTowardsTransfom);
    }
    #endregion

    private void KillPlayer(Collider other)
    {
        GameObject player;
        player = other.transform.root.gameObject;
        FindObjectOfType<Scenemanager>().EndPlayer(player);
    }

    private void LoadingAndUnloading()
    {
        bool setActive = (mode == Mode.Load);

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
                objects[i].SetActive(setActive);
        }
    }

    private void LoadingAndUnloadingScenes()
    {
        if (Scenemanager == null) return;
        for (int i = 0; i < LevelNameStrings.Length; i++)
        {
            string sceneName = LevelNameStrings[i];

            if (string.IsNullOrEmpty(sceneName))
                continue;

            if (modeScene == ModeScene.Load)
            {
                Scenemanager.LoadAScene(sceneName);
            }
            else
            {
                Scenemanager.UnLoadAScene(sceneName);
            }
        }
    }

    private void Music()
    {
        if (Scenemanager == null) return;
        if (OneTimeUseM && hasBeenUsedM) return;
        if (OneTimeUseM) hasBeenUsedM = true;

        if (modeMusic == ModeMusic.Play)
        {
            Scenemanager.ChangeMusicWTSpLevelM(MusicName);
        }
        else
        {
            Scenemanager.ReturnSceneMusic();
        }
    }
    private void SwitchGroundType(Collider other)
    {
        var rb = other.GetComponentInParent<Rigidbody>();
        if (rb == null) return;

        var player = rb.GetComponentInChildren<PlayerAnimatorScript>();
        var PlayerAnimatorScript = player;
        if (PlayerAnimatorScript == null) return;
        PlayerAnimatorScript.ChooseGroundType = Choosetype;
        PlayerAnimatorScript.SetGroundType(PlayerAnimatorScript.ChooseGroundType);
    }


    private void SwitchTo2Dor3Dhand(Collider other)
    {
        var rb = other.GetComponentInParent<Rigidbody>();
        if (rb == null) return;

        var player = rb.GetComponentInChildren<PlayerMoving>();

        PlayerMovingScript = player;
        NewcamScript = PlayerMovingScript.Camera.GetComponent<Newcam>();

        if (LookTowardsTransfom == null) return;
        LookCamSth();
        }
    }
