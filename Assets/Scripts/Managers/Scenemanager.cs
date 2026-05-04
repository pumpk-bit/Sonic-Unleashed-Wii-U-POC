using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

[System.Serializable]

public class Scenemanager : MonoBehaviour {


    [Header("Scenes")]
    [SerializeField] public bool IsMenuScene;
    [SerializeField] private string SceneFirst; //Fix multiple 

    public enum StartingType
    {
        Dashing,
        ThumbsUp,
        None
    }
    public StartingType LevelStartingType;

    [Header("Audio")]
    [SerializeField] private string SceneMusic;
    [SerializeField] public string CurrnetSong;
    [SerializeField] private MusicManager MusicManager;

    [Header("Respawn")]
    [SerializeField] private GameObject LoadingScreen;

    [SerializeField] private GameObject GameStartPlace;
    [SerializeField] float ZOffset;
    [SerializeField] float YOffset;
    [SerializeField] float XOffset;

    [HideInInspector] Transform RespawnPosition;

    [Header("Other")]
    [SerializeField] private Transform SetAllObjectsMain;

    private PlayerMoving PlayerMoving;
    void Awake()
    {
  
       
    }
    void Start()
    {
        if (IsMenuScene)
        {
            PlayStartingSong();
            Debug.LogError("Fix this");
            return;
        }

        CheckIfAllIsAssigned();

        LoadingScreen.SetActive(false);

        StartCoroutine(UnloadUnused());
        LoadSceneFirst();

        PlayerRespawnHandler();


        PlayStartingSong();
    }

    private void DoThingsAfterFirstSceneIsLoaded()
    {
        if (PlayerMoving == null)
            PlayerMoving = FindObjectOfType<PlayerMoving>();
        PlayerMoving.canMove = false; // Make sure the player can't move until we set the starting type. Undo is in playerside

        AnimatorScriptS ScriptAnim = FindObjectOfType<AnimatorScriptS>();

        if (LevelStartingType == StartingType.Dashing)
        {
            ScriptAnim.SetStartType(2);
        }
        else if (LevelStartingType == StartingType.ThumbsUp)
        {
            ScriptAnim.SetStartType(1);
        }
        else if (LevelStartingType == StartingType.None)
        {
            ScriptAnim.SetStartType(3);
            PlayerMoving.canMove = true;
        }

    }
    private void CheckIfAllIsAssigned()
    {
        if (MusicManager == null)
        {
            Debug.LogError("MusicManager not assigned in Scenemanager. Fixing for now.");
            MusicManager = FindObjectOfType<MusicManager>();
        }

        if (GameStartPlace == null)
            Debug.LogError("GameStartPlace not assigned in Scenemanager. Cannot fix.");

        if (LoadingScreen == null)
            Debug.LogError("LoadingScreen not assigned in Scenemanager. Cannot fix.");

    }

    #region Player Death and Respawn
    public void EndPlayer(GameObject player)
    {
        Debug.LogError("Gamemanager.Lives - 1. Look if it's -1... fix this");
    }
    private void PlayerRespawnHandler()
    {
        SetNewRespawnPosition(GameStartPlace.transform);

        PlayerMoving[] players = FindObjectsOfType<PlayerMoving>();

        foreach (PlayerMoving p in players)
        {        
            RespawnAtPosition(p.gameObject);
        }

    }
    private void RespawnAtPosition(GameObject PlayerObject)
    {
        PlayerObject.transform.position = new Vector3(RespawnPosition.position.x + XOffset, RespawnPosition.position.y + YOffset, RespawnPosition.position.z + ZOffset);
        PlayerObject.transform.rotation = new Quaternion(RespawnPosition.rotation.x, RespawnPosition.rotation.y, RespawnPosition.rotation.z, RespawnPosition.rotation.w);
    }
    public void SetNewRespawnPosition(Transform RespawnTransform = null)
    {
        RespawnPosition = RespawnTransform;
    }

    //Reseting:
    IEnumerator SoftResetPlayer(GameObject player)
    {
        //AudioManager.Play("DeathSfx");
        Debug.LogError("Fix this...");
        //yield return new WaitForSeconds(1f);
        player.GetComponentInParent<PlayerMoving>().SetVelToZero();

        LoadingScreen.SetActive(true);

        ActivateMany.SetHierarchyActive(SetAllObjectsMain, true);

        // Wait one frame so all Start() calls finish
        yield return null;

        RespawnAtPosition(player);
        LoadingScreen.SetActive(false);
    }

    public void RestartLevelFully()
    {
        PlayerMoving = FindObjectOfType<PlayerMoving>();

        string currentSceneName = SceneManager.GetActiveScene().name;
        LoadSceneAsync(currentSceneName, true, PlayerMoving);
    }

    public void LevelEnd()
    {
        MusicManager.StopPlaying(SceneMusic);
    }

    #endregion

    #region Scene Loading and Unloading
    private void LoadSceneFirst()
    {
        if (SceneFirst == "")
        {
            Debug.Log("SceneFirst is not set in the inspector! or you didn't put any.");
            DoThingsAfterFirstSceneIsLoaded();
            return;
        }
        PlayerMoving = FindObjectOfType<PlayerMoving>();
        StartCoroutine(LoadSceneAsync(SceneFirst, true, PlayerMoving, true)); // Load the first scene additively, and pass true to indicate that the player should be frozen during loading.

    }

    public void LoadAScene(string LevelString)
    {
        if (LevelString == "")
        {
            return;
        }
        StartCoroutine(LoadSceneAsync(LevelString, false));

    }
    public void UnLoadAScene(string LevelString)
    {
        if (LevelString == "")
        {
            return;
        }
        StartCoroutine(UnloadSceneAsync(LevelString, false));

    }

    IEnumerator LoadSceneAsync(string GameScene, bool PlayerIsFrozen, PlayerMoving playerScript = null, bool IsFirstLoad = false)
    {
        if (PlayerIsFrozen)
        {
            PlayerMoving.canMove = false;
            LoadingScreen.SetActive(true);
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(GameScene, LoadSceneMode.Additive);
        while (!op.isDone)
        {
            yield return null;
        }
        if (PlayerIsFrozen)
        {
            PlayerMoving.canMove = true;
            LoadingScreen.SetActive(false);
        }
        if (IsFirstLoad)
        {
            DoThingsAfterFirstSceneIsLoaded();
        }


    }

    IEnumerator UnloadSceneAsync(string GameScene, bool PlayerIsFrozen)
    {
        if (PlayerIsFrozen)
            PlayerMoving.canMove = false;
        AsyncOperation op = SceneManager.UnloadSceneAsync(GameScene);
        while (!op.isDone)
        {
            yield return null;
        }
        if (PlayerIsFrozen)
            PlayerMoving.canMove = true;
        StartCoroutine(UnloadUnused());
    }
    IEnumerator UnloadUnused()
    {
        AsyncOperation op = Resources.UnloadUnusedAssets();
        yield return op; // waits until the cleanup is done
    }
    #endregion

    #region Music Managment
    private void PlayStartingSong()
    {
        CurrnetSong = SceneMusic;
        MusicManager.Play(SceneMusic);
    }

    public void ChangeMusic(string newSong)
    {
        if (newSong == "")
            return;
        MusicManager.StopPlaying(CurrnetSong);
        CurrnetSong = newSong;
        MusicManager.Play(newSong);
    }
    private string TMPsong;
    public void ChangeMusicWTSpLevelM(string newSong)
    {
        if (newSong == "")
            return;
        MusicManager.ChangeVolume(SceneMusic, 0f);
        CurrnetSong = newSong;
        MusicManager.Play(newSong);
    }
    public void ReturnSceneMusic()
    {
        if (CurrnetSong == "")
            return;
        if (CurrnetSong == SceneMusic)
            return;
        MusicManager.StopPlaying(CurrnetSong);
        CurrnetSong = SceneMusic;
        MusicManager.ChangeVolume(SceneMusic, 100f);
    }   

    #endregion


}
