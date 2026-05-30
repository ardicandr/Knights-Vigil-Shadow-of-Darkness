using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingController : MonoBehaviour
{
    public Slider progressBar;
    public Text progressText;
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1f;
    
    [Header("Settings")]
    public float minLoadingTime = 3f;

    void Start()
    {
  
        if (fadeGroup != null) StartCoroutine(Fade(0));
        StartCoroutine(LoadLevelAsync());
    }

    IEnumerator LoadLevelAsync()
    {
        string targetLevel = LevelData.NextLevelName;
        if (string.IsNullOrEmpty(targetLevel)) {
            Debug.LogError("Nama level tujuan kosong!");
            yield break;
        }

        float startTime = Time.time;
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetLevel);
        operation.allowSceneActivation = false;

        float visualProgress = 0f;
        while (!operation.isDone)
        {
            float actualProgress = Mathf.Clamp01(operation.progress / 0.9f);
            visualProgress = Mathf.MoveTowards(visualProgress, actualProgress, Time.deltaTime * 0.5f);

            if (progressBar != null) progressBar.value = visualProgress;
            if (progressText != null) progressText.text = (visualProgress * 100f).ToString("F0") + "%";

            float elapsedTime = Time.time - startTime;
            if (operation.progress >= 0.9f && elapsedTime >= minLoadingTime && visualProgress >= 0.99f)
            {
       
                if (fadeGroup != null) yield return StartCoroutine(Fade(1));
                operation.allowSceneActivation = true;
            }
            yield return null;
        }
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeGroup.alpha;
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }
        fadeGroup.alpha = targetAlpha;
    }
}