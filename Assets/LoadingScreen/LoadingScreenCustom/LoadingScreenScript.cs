using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreenScript : MonoBehaviour
{
    [Header("UI")]
    public GameObject LoadingScreen;
    public GameObject LoadingButton;
    public Slider LoadingBarSlider;

    [Header("Audio")]
    public MusicManager MusicManager;
    [SerializeField] string LoadingMusic;
    [SerializeField] float LoadingMusicVolume = 1f;

    void Start()
    {
        // Find AudioManager safely
        if (MusicManager == null)
            MusicManager = FindObjectOfType<MusicManager>();

        LoadingScreen.SetActive(false);
        LoadingButton.SetActive(true);
    }

    public void LoadScene(int sceneId)
    {
        StartCoroutine(LoadSceneAsync(sceneId));
    }

    IEnumerator LoadSceneAsync(int sceneId)
    {
        // Start loading
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
        LoadingScreen.SetActive(true);
        LoadingButton.SetActive(false);

        // Play loading music
        if (MusicManager != null)
            MusicManager.PlayVolumeOfChoice(LoadingMusic, LoadingMusicVolume);

        // Update progress bar
        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);
            LoadingBarSlider.value = progressValue;
            yield return null;
        }
    }
}
