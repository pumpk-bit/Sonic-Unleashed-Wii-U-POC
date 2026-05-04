using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
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
}
