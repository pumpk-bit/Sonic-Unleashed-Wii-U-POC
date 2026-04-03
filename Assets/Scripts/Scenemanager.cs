using System.Diagnostics;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using UnityEngine.PostProcessing;
using System.Collections;

[System.Serializable]

public class Scenemanager : MonoBehaviour {

    public static Scenemanager instance;

    [Header("Scenes")]
    [SerializeField] private string SceneFirst;
    [SerializeField] private Scene Second;
    [SerializeField] private Scene Third;


    [Header("Audio")]

    [SerializeField] private string currentSongString;

    [SerializeField] private MusicManager MusicManager;
    [SerializeField] private AudioManager AudioManager;



    [Header("Respawn")]
    [SerializeField] private PlayerRespawnManager PlayerRespawnManager;
    [SerializeField] private GameObject GameStartPlace;

    [SerializeField] private GameObject LoadingScreen;


    [Header("Other")]
    [SerializeField] private VictoryManager VictoryManager;
    [SerializeField] private Transform SetAllObjectsMain;

    [Header("Scripts")]
    [SerializeField] private PlayerMoving PlayerMoving;
    [SerializeField] private Newcam Newcam;
    [SerializeField] private CameraHolderScript CameraHolderScript;
    [SerializeField] private GameObject PlayerObj;
    [SerializeField] private GameObject CameraObj;
    [SerializeField] private GameObject Music;
    [SerializeField] private GameObject Audio;

    void Awake()
    {
        //if (instance != null)
        //{
        //   Destroy(gameObject);
        // return;
        //}
        //else
        //{
        //   instance = this;
        //  DontDestroyOnLoad(gameObject);
        //}
    }
    void Start()
    {
        CheckIfAllIsAssigned();
        //DebugMesseagesLoader();

        Time.timeScale = 1f;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        LoadingScreen.SetActive(false);

        StartCoroutine(UnloadUnused());
        LoadSceneFirst();

        PlayerRespawnHandler();

        MusicManager.Play(currentSongString);
    }

    private void DebugMesseagesLoader()
    {
        Debug.Log("Add GamePad use for Wii U");
    }

    private void CheckIfAllIsAssigned()
    {
        if (MusicManager == null)
        {
            Debug.LogError("MusicManager not assigned in Scenemanager. Fixing for now.");
            MusicManager = FindObjectOfType<MusicManager>();
        }
        if (AudioManager == null)
        {
            Debug.LogError("AudioManager not assigned in Scenemanager. Fixing for now.");
            AudioManager = FindObjectOfType<AudioManager>();
        }

        if (PlayerRespawnManager == null)
            Debug.LogError("PlayerRespawnManager not assigned in Scenemanager. Fixing for now."); PlayerRespawnManager = GetComponent<PlayerRespawnManager>();

        if (GameStartPlace == null)
            Debug.LogError("GameStartPlace not assigned in Scenemanager. Cannot fix.");

        if (LoadingScreen == null)
            Debug.LogError("LoadingScreen not assigned in Scenemanager. Cannot fix.");

        if (SetAllObjectsMain == null)
            Debug.LogError("SetAllObjectsMain (Rings and other) not assigned in Scenemanager. Cannot fix.");


    }


    private void PlayerRespawnHandler()
    {
        PlayerRespawnManager.SetNewRespawnPosition(GameStartPlace.transform);

        PlayerMoving[] players = FindObjectsOfType<PlayerMoving>();

        foreach (PlayerMoving p in players)
        {        
            PlayerRespawnManager.RespawnAtPosition(p.gameObject);
        }

    }

    IEnumerator SetAllObjectsThatNeedToBeOnOn(GameObject player)
    {
        AudioManager.Play("DeathSfx");

        //yield return new WaitForSeconds(1f);
        player.GetComponentInParent<PlayerMoving>().SetVelToZero();

        LoadingScreen.SetActive(true);

        ActivateMany.SetHierarchyActive(SetAllObjectsMain, true);

        // Wait one frame so all Start() calls finish
        yield return null;

        PlayerRespawnManager.RespawnAtPosition(player);
        LoadingScreen.SetActive(false);
    }


    #region SceneEndingAndRestarting
    public void LevelEnd()
    {
        MusicManager.StopPlaying(currentSongString);
        VictoryManager.VictoryScreenSetUp();
    }

    public void RestartLevel()
    {
        FindObjectOfType<ResetLevel>().DestroyDontDestroyOnLoadObjects();
    }

    public void EndPlayer(GameObject player)
    {
        StartCoroutine(SetAllObjectsThatNeedToBeOnOn(player));
    }
    #endregion
    #region Scene Loading and Unloading
    private void LoadSceneFirst()
    {
        if (SceneFirst == "")
        {
            Debug.Log("SceneFirst is not set in the inspector! or you didn't put any.");
            return;
        }
        PlayerMoving = FindObjectOfType<PlayerMoving>();
        StartCoroutine(LoadSceneAsync(SceneFirst, true)); // Load the first scene additively, and pass true to indicate that the player should be frozen during loading.

    }

    IEnumerator LoadSceneAsync(string GameScene, bool PlayerIsFrozen)
    {
        if (PlayerIsFrozen)
            PlayerMoving.CanScriptRun = false; LoadingScreen.SetActive(true); // Make sure the player can't move while the scene is loading.

        AsyncOperation op = SceneManager.LoadSceneAsync(GameScene, LoadSceneMode.Additive);
        while (!op.isDone)
        {
            yield return null;
        }
        if (PlayerIsFrozen)
            PlayerMoving.CanScriptRun = true; LoadingScreen.SetActive(false); // Make sure the player can move again after the scene has finished loading.

    }

    IEnumerator UnloadSceneAsync(string GameScene, bool PlayerIsFrozen)
    {
        if (PlayerIsFrozen)
            PlayerMoving.CanScriptRun = false; // Make sure the player can't move while the scene is unloading.
        AsyncOperation op = SceneManager.UnloadSceneAsync(GameScene);
        while (!op.isDone)
        {
            yield return null;
        }
        if (PlayerIsFrozen)
            PlayerMoving.CanScriptRun = true; // Make sure the player can move again after the scene has finished unloading.
        StartCoroutine(UnloadUnused());
    }
    IEnumerator UnloadUnused()
    {
        AsyncOperation op = Resources.UnloadUnusedAssets();
        yield return op; // waits until the cleanup is done
    }
    #endregion


    public void TurnoffScript(int Number)
    {
        if (Number == 0) PlayerMoving.enabled = false;
        if (Number == 1) Newcam.enabled = false;
        if (Number == 2) CameraHolderScript.enabled = false;
        if (Number == 3) PlayerObj.SetActive(false);
        if (Number == 4) CameraObj.SetActive(false);
        if (Number == 5) Music.SetActive(false);
        if (Number == 6) Audio.SetActive(false);

    }
    public void TurnonScript(int Number)
    {
        if (Number == 0) PlayerMoving.enabled = true;
        if (Number == 1) Newcam.enabled = true;
        if (Number == 2) CameraHolderScript.enabled = true;
        if (Number == 3) PlayerObj.SetActive(true);
        if (Number == 4) CameraObj.SetActive(true);
        if (Number == 5) Music.SetActive(true);
        if (Number == 6) Audio.SetActive(true);

    }
}
