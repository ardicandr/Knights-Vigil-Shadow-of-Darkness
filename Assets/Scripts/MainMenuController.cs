using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] public string firstLevelName = "Shadowshire Village";
    
    [Header("Transition Settings")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1f;

    [Header("UI Panels")]
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject quitConfirmPanel;

    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Tutup semua panel saat awal
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (quitConfirmPanel != null) quitConfirmPanel.SetActive(false);

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

    public void OpenSettings() {
        settingsPanel.SetActive(true);
    }
    public void CloseSettings() {
        settingsPanel.SetActive(false);
    }

    public void OpenCredits() {
        creditsPanel.SetActive(true);
    }
    public void CloseCredits() {
        creditsPanel.SetActive(false);
    }

    public void ShowQuitConfirmation() {
        quitConfirmPanel.SetActive(true);
    }

    public void HideQuitConfirmation() {
        quitConfirmPanel.SetActive(false);
    }

    public void FinalQuit() {
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