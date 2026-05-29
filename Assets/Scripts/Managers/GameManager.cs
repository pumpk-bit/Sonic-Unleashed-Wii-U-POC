using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WiiU = UnityEngine.WiiU;

public class GameManager : MonoBehaviour
{
    WiiU.GamePad gp = WiiU.GamePad.access;

    [Header("Game Data")]
    [Tooltip("Version. Naming: 01 02 03 D/P. 01 = release, 02 = Bug fix/update, 03 = small bug fix. D/P = developer or playable")]
    [SerializeField] public string GameVersion;

    public List<LevelData> levels = new List<LevelData>();

    [System.Serializable]
    [SerializeField]
    public class LevelData
    {
        public string levelName;
        public float SceneBestTime;
        public float SceneBestScore;
        public float SceneBestSpeed;
        public float SceneBestRings;

        public int SunMedalsMax;
        public int SunMedalsCol;

        public int MoonMedalsMax;
        public int MoonMedalsCol;
    }

    public static GameManager instance;
    public enum CurrentSceneType
    {
        GameLevel,
        Hubworld,
        MainMenu,
    }

    [Header("Current Data")]
    /// ----
    [SerializeField] public int PlayerLives;
    [SerializeField] public int PlayerRings;
    [SerializeField] public int MoonMedals;
    [SerializeField] public int SunMedals;
    [SerializeField] public int XP;

    [Header("Scenedata")]
    [SerializeField] public float SceneTime;
    [SerializeField] public float ScenScore;
    [SerializeField] public float SceneSpeed;
    [SerializeField] public float SceneRings; //1 ring = 100
     
    [SerializeField] public float SceneSunMedals;
    [SerializeField] public float SceneMoonMedals;

  

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void SceneHasChanged()
    {
    }


    //Scene Change Detection
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);

        SceneHasChanged();
    }



    //Temp
    void Update()
    {
        WiiU.GamePadState state = gp.state;

        if (state.gamePadErr == WiiU.GamePadError.None)
        {
            //moveX = state.lStick.x;
            //moveY = state.lStick.y;

            if (state.IsTriggered(WiiU.GamePadButton.Plus) && state.IsTriggered(WiiU.GamePadButton.Minus))
                {
                LoadScn();
            }

        }

        //0 = A, 1 = B, 2 = X, 3 = 4 JoystickButton4 --  LB JoystickButton5 -- RB
        if (Input.GetKeyDown(KeyCode.JoystickButton4) && Input.GetKeyDown(KeyCode.JoystickButton5))
        {
            LoadScn();
        }
    }

    private void LoadScn()
    {
        SceneManager.LoadScene(0);
    }
}
