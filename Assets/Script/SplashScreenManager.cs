using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // o System.Collections.Generic

public class SplashScreenManager : MonoBehaviour
{
    public string mainSceneName = "Menu";
    public float minSplashTime = 3f;

    void Start()
    {
        StartCoroutine(LoadAppAsync());
    }

    IEnumerator LoadAppAsync()
    {

        AsyncOperation operation = SceneManager.LoadSceneAsync(mainSceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;
        while (timer < minSplashTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        operation.allowSceneActivation = true;
    }
}
