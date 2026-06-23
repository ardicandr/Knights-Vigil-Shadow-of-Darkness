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
    public float panelFadeDuration = 0.6f;

    [Header("UI Panels")]
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject quitConfirmPanel;

    private CanvasGroup settingsCG;
    private CanvasGroup creditsCG;
    private CanvasGroup quitCG;

    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (settingsPanel != null) settingsCG = settingsPanel.GetComponent<CanvasGroup>();
        if (creditsPanel != null) creditsCG = creditsPanel.GetComponent<CanvasGroup>();
        if (quitConfirmPanel != null) quitCG = quitConfirmPanel.GetComponent<CanvasGroup>();

        SetupPanelAtStart(settingsPanel, settingsCG);
        SetupPanelAtStart(creditsPanel, creditsCG);
        SetupPanelAtStart(quitConfirmPanel, quitCG);

        if (fadeGroup != null) StartCoroutine(Fade(0));
    }

    void SetupPanelAtStart(GameObject panel, CanvasGroup cg)
    {
        if (panel != null)
        {
            panel.SetActive(false);
            if (cg != null) cg.alpha = 0;
        }
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
        StopAllCoroutines();
        StartCoroutine(FadePanelIn(settingsPanel, settingsCG));
    }
    public void CloseSettings() {
        StartCoroutine(FadePanelOut(settingsPanel, settingsCG));
    }

    public void OpenCredits() {
        StopAllCoroutines();
        StartCoroutine(FadePanelIn(creditsPanel, creditsCG));
    }
    public void CloseCredits() {
        StartCoroutine(FadePanelOut(creditsPanel, creditsCG));
    }

    public void ShowQuitConfirmation() {
        StopAllCoroutines();
        StartCoroutine(FadePanelIn(quitConfirmPanel, quitCG));
    }

    public void HideQuitConfirmation() {
        StartCoroutine(FadePanelOut(quitConfirmPanel, quitCG));
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

    IEnumerator FadePanelIn(GameObject panel, CanvasGroup cg)
    {
        if (panel == null || cg == null) yield break;
        panel.SetActive(true);
        float timer = 0;
        while (timer < panelFadeDuration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0, 1, timer / panelFadeDuration);
            yield return null;
        }
        cg.alpha = 1;
    }

    // Coroutine baru untuk menyembunyikan panel
    IEnumerator FadePanelOut(GameObject panel, CanvasGroup cg)
    {
        if (panel == null || cg == null) yield break;
        float timer = 0;
        while (timer < panelFadeDuration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1, 0, timer / panelFadeDuration);
            yield return null;
        }
        cg.alpha = 0;
        panel.SetActive(false);
    }
}