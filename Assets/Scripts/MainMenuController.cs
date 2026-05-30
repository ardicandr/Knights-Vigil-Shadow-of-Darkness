using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] public string firstLevelName = "Shadowshire Village";
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1f;

    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (fadeGroup != null) StartCoroutine(Fade(0));
    }

    public void PlayGame()
    {
        StartCoroutine(StartGameSequence());
    }

    IEnumerator StartGameSequence()
    {
        if (fadeGroup != null) yield return StartCoroutine(Fade(1));
        LevelData.StoredHealth = -1f;

        LevelData.NextLevelName = firstLevelName;
        SceneManager.LoadScene("LoadingScene");
    }

    public void QuitGame()
    {
        Debug.Log("Game ditutup.");
        Application.Quit();
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