using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.WiiU;
using WiiU = UnityEngine.WiiU;
using UnityEngine.SceneManagement;

public class DebugMenuScript : MonoBehaviour {


    bool CanScriptRun = true;
    [SerializeField] Slider LoadingBarSlider;
    [SerializeField] GameObject SliderObj;


    [SerializeField] GameObject BackPannel;
    [SerializeField] Text Text1B;
    [SerializeField] Text Text2B;
    [SerializeField] Text Text3B;


    [SerializeField] GameObject FrontPannel;
    [SerializeField] Text Text1F;
    [SerializeField] Text Text2F;

    WiiU.GamePad gp = WiiU.GamePad.access;
    private int CurrentScneneIndex;
    private int selectedSceneIndex;
    public enum Currenttype
    {
        PopUp,
        LoadingAScene,
        SelectingAScene,
    }
    public Currenttype CurrentAction;
    int sceneCount;
    // Use this for initialization
    void Start ()
    {
        selectedSceneIndex = SceneManager.GetActiveScene().buildIndex;
        CurrentAction = Currenttype.SelectingAScene;

        CanScriptRun = true;
        FrontPannel.SetActive(false);
        BackPannel.SetActive(true);

        sceneCount = SceneManager.sceneCountInBuildSettings;

        SliderObj.SetActive(false);
        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            Debug.Log(sceneName);
        }
        var GameManagerScript = FindObjectOfType<GameManager>();
        Text3B.text = "Press: \"Start\" and \"Select\" to always return. Game Version: " + GameManagerScript.GameVersion;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!CanScriptRun) return;
        Inputs();

        Text1B.text = "Scene Count: " + sceneCount+ "|" + " Time:  " + System.DateTime.Now.ToString("hh:mm:ss tt") + "|" + " Account: " + Core.accountName + "|";
        Text2B.text = "Current Scene: " + GetSceneName(selectedSceneIndex);
    }

    private void Inputs()
    {
        if (!CanScriptRun) return;

        WiiU.GamePadState state = gp.state;

        if (state.gamePadErr == WiiU.GamePadError.None)
        {
            //moveX = state.lStick.x;
            //moveY = state.lStick.y;

            if (state.IsTriggered(WiiU.GamePadButton.B)) BAcction();

            if (state.IsTriggered(WiiU.GamePadButton.A)) AAcction();

            if (state.IsTriggered(WiiU.GamePadButton.Right)) ChangeSceneAdd(1);
            if (state.IsTriggered(WiiU.GamePadButton.Left)) ChangeSceneAdd(-1);
        }

        //0 = A, 1 = B, 2 = X, 3 = 4 JoystickButton4 --  LB JoystickButton5 -- RB
        if (Input.GetKeyDown(KeyCode.JoystickButton0)) AAcction();
        if (Input.GetKeyDown(KeyCode.JoystickButton1)) BAcction(); //

        if (Input.GetKeyDown(KeyCode.JoystickButton2)) ChangeSceneAdd(-1); //
        if (Input.GetKeyDown(KeyCode.JoystickButton3)) ChangeSceneAdd(1); //
    }

    private void AAcction()
    {
        if (!CanScriptRun) return;

        switch (CurrentAction)
        {
            case Currenttype.PopUp:
                PopUp("", true);
                break;
            case Currenttype.SelectingAScene:
                SceneSelect();
                break;
            case Currenttype.LoadingAScene:
                LoadSelectedScene();
                break;

        }
        


    }
    private void BAcction()
    {
        if (!CanScriptRun) return;

        switch (CurrentAction)
        {
            case Currenttype.PopUp:
                PopUp("", true);
                break;
            case Currenttype.LoadingAScene:
                SceneSelect(true);
                break;

        }
    }

    private void ChangeSceneAdd(int direction)
    {
        if (!CanScriptRun) return;

        if (CurrentAction != Currenttype.SelectingAScene) return;

        selectedSceneIndex += direction;

        if (selectedSceneIndex >= sceneCount)
            selectedSceneIndex = 0;

        if (selectedSceneIndex < 0)
            selectedSceneIndex = sceneCount - 1;
    }

    private void PopUp(string astring = "", bool closing = false)
    {
        if (!CanScriptRun) return;

        CurrentAction = Currenttype.PopUp;

        if (closing)
        {
            FrontPannel.SetActive(false);
            CurrentAction = Currenttype.SelectingAScene;
            //BackPannel.SetActive(true);
            return;
        }
        FrontPannel.SetActive(true);
        //BackPannel.SetActive(false);

        Text1F.text = astring;
        Text2F.text = "Press 'A' to close.";
    }

    private void SceneSelect(bool close = false)
    {
        if (!CanScriptRun) return;

        if (GetSceneName(selectedSceneIndex) == SceneManager.GetActiveScene().name)
        {
            PopUp("You are already in the selected scene.");
            return;
        }

        if (close)
        {
            FrontPannel.SetActive(false);
            CurrentAction = Currenttype.SelectingAScene;

            return;
        }
        CurrentAction = Currenttype.LoadingAScene;

        FrontPannel.SetActive(true);
        Text1F.text = "Scene Select";
        string sceneName = GetSceneName(selectedSceneIndex);

        if (sceneName.Contains("Test") ||
            sceneName.Contains("Demo") ||
            sceneName.Contains("Beta") ||
            sceneName.Contains("Temp") ||
            sceneName.Contains("Debug") ||
            sceneName.Contains("Tmp"))
        {
            Text2F.text = "Warning. Scne: " + GetSceneName(selectedSceneIndex) + " is not a public scene and can crash. Press 'A' to load the selected scene.";

        }
        else
        {
            Text2F.text = "You chose Scene: " + GetSceneName(selectedSceneIndex) + " Press 'A' to load the selected scene.";
        }
    }

    private void LoadSelectedScene()
    {
        if (GetSceneName(selectedSceneIndex) == SceneManager.GetActiveScene().name)
        {
            PopUp("Illegal scene noy loaded. Return...");
            return;
        }

        SliderObj.SetActive(true);

        CanScriptRun = false;
        CurrentScneneIndex = selectedSceneIndex;
        SceneManager.LoadScene(selectedSceneIndex);
        //StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        AsyncOperation operation =
            SceneManager.LoadSceneAsync(selectedSceneIndex);

        while (!operation.isDone)
        {
            float progressValue =
                Mathf.Clamp01(operation.progress / 0.9f);

            LoadingBarSlider.value = progressValue;

            yield return null;
        }
    }
    private string GetSceneName(int index)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(index);
        return System.IO.Path.GetFileNameWithoutExtension(path);
    }
}
