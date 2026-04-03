using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WhenTriggeredTurnOn : MonoBehaviour {

    public void TurnOnGameObject(string SceneName)
    {
        StartCoroutine(LoadSceneAsync(SceneName));
    }

    public void TurnOffGameObject(string SceneName)
    {
        StartCoroutine(UnloadSceneAsync(SceneName));
    }
    public void TrurnOffItself(GameObject GB)
    {
        GB.SetActive(false);
    }

    IEnumerator LoadSceneAsync(string GameScene)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(GameScene, LoadSceneMode.Additive);
        while (!op.isDone)
        {
            yield return null;
        }
    }
    IEnumerator UnloadSceneAsync(string GameScene)
    {
      
        AsyncOperation op = SceneManager.UnloadSceneAsync(GameScene);
        while (!op.isDone)
        {
            yield return null;
        }
        StartCoroutine(UnloadUnused());
    }
    IEnumerator UnloadUnused()
    {
        AsyncOperation op = Resources.UnloadUnusedAssets();
        yield return op; // waits until the cleanup is done
    }
}
