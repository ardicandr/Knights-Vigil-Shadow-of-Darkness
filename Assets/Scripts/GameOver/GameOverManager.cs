using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("Fade Transition Settings")]
    [Tooltip("Tarik UI Image Hitam yang menutupi seluruh layar ke sini")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1f;

    private bool isTransitioning = false;

    private void Start()
    {
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }
    }

    public void RestartLevel()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionSequence(SceneManager.GetActiveScene().name));
    }

    public void GoToMainMenu()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionSequence("LoadingScene", true));
    }

    private IEnumerator TransitionSequence(string targetScene, bool useLoadingScene = false)
    {
        isTransitioning = true;

        if (fadeGroup != null)
        {
            fadeGroup.gameObject.SetActive(true);
            fadeGroup.blocksRaycasts = true;
        }

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime; 
            if (fadeGroup != null)
            {
                fadeGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            }
            yield return null;
        }

        if (fadeGroup != null) fadeGroup.alpha = 1f;

        Time.timeScale = 1f;
        LevelData.StoredHealth = -1f; 

        if (useLoadingScene)
        {
            LevelData.NextLevelName = "MainMenu";
            SceneManager.LoadScene("LoadingScene");
        }
        else
        {
            SceneManager.LoadScene(targetScene);
        }
    }
}