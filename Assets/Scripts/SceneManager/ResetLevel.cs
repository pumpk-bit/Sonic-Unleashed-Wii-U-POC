using System.Collections;
using UnityEngine.SceneManagement;

using UnityEngine;
using UnityEngine.UI;

public class ResetLevel : MonoBehaviour {

    [SerializeField] MusicManager MusicManager;
    [SerializeField] string LoadingMusic;
    [SerializeField] GameObject LoadingScreen;

    private void Start()
    {
        CheckIfAllIsAssigned();
    }
    private void CheckIfAllIsAssigned()
    {
        if (MusicManager == null)
            Debug.LogError("MusicManager not assigned in ResetLevel. Fixing for now."); MusicManager = FindObjectOfType<MusicManager>();
        if (LoadingScreen == null)
            Debug.LogError("LoadingScreen not assigned in ResetLevel. Cannot fix.");
    }

    public void DestroyDontDestroyOnLoadObjects() 
    {
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator Reload()
    {
        yield return null; // wait one frame
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    IEnumerator LoadSceneAsync()
    {
        // Start loading
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        LoadingScreen.SetActive(true);

        MusicManager.Play(LoadingMusic);

        // Update progress bar
        while (!operation.isDone)
        {
            yield return null;
        }
    }
}
