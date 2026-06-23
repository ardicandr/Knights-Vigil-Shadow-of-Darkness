using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PauseMenuController : MonoBehaviour
{
    public GameObject pausePanel;    
    [Tooltip("Masukkan CanvasGroup yang menempel pada Pause Panel")]
    public CanvasGroup pauseCanvasGroup;
    [Tooltip("Durasi transisi menu pause (rekomendasi 0.2 detik agar instan)")]
    public float fadeDuration = 0.2f;

    [Header("Pause Audio Settings (Dark Fantasy)")]
    [Tooltip("SFX saat menu pause dibuka (cth: dentang lonceng sunyi, suara pedang ditarik perlahan)")]
    [SerializeField] private AudioClip pauseOpenSFX;
    [Tooltip("SFX saat menu pause ditutup (cth: suara klik batu maut/mekanisme kuno)")]
    [SerializeField] private AudioClip pauseCloseSFX;

    [Header("UI Panels (Settings)")]
    public GameObject settingsPanel;
    public CanvasGroup settingsCanvasGroup;

    public static bool isPaused = false;
    private PlayerController playerScript;
    private Coroutine pauseCoroutine;
    private RectTransform panelRectTransform;
    private AudioSource audioSource;

    void Start()
    {
        playerScript = FindObjectOfType<PlayerController>();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (pausePanel != null)
        {
            panelRectTransform = pausePanel.GetComponent<RectTransform>();
            
            if (pauseCanvasGroup != null) pauseCanvasGroup.alpha = 0f;
            pausePanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            if (settingsCanvasGroup != null) settingsCanvasGroup.alpha = 0f;
            settingsPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        
        if (pauseCoroutine != null) StopCoroutine(pauseCoroutine);
        pauseCoroutine = StartCoroutine(AnimatePauseMenu(0f, 1f, true));

        if (audioSource != null && pauseOpenSFX != null)
        {
            audioSource.PlayOneShot(pauseOpenSFX);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        isPaused = false;

        if (pauseCoroutine != null) StopCoroutine(pauseCoroutine);
        pauseCoroutine = StartCoroutine(AnimatePauseMenu(1f, 0f, false));

        if (audioSource != null && pauseCloseSFX != null)
        {
            audioSource.PlayOneShot(pauseCloseSFX);
        }

        if (playerScript != null)
        {
            if (playerScript.hideCursorOnStart)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        if (MobileInputManager.Instance != null)
        {
            MobileInputManager.Instance.moveInput = Vector2.zero;
            MobileInputManager.Instance.lookInput = Vector2.zero;
        }
    }

    private IEnumerator AnimatePauseMenu(float startAlpha, float endAlpha, bool isOpening)
    {
        if (isOpening)
        {
            pausePanel.SetActive(true);
            if (panelRectTransform != null) panelRectTransform.localScale = new Vector3(0.7f, 1f, 1f);
        }

        float elapsedTime = 0f;
        if (pauseCanvasGroup != null) pauseCanvasGroup.alpha = startAlpha;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; 
            float progress = Mathf.Clamp01(elapsedTime / fadeDuration);
            
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            if (pauseCanvasGroup != null)
            {
                pauseCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, smoothProgress);
            }

            if (panelRectTransform != null)
            {
                if (isOpening)
                    panelRectTransform.localScale = new Vector3(Mathf.Lerp(0.7f, 1f, smoothProgress), 1f, 1f);
                else
                    panelRectTransform.localScale = new Vector3(Mathf.Lerp(1f, 0.7f, smoothProgress), 1f, 1f);
            }

            yield return null;
        }

        if (pauseCanvasGroup != null) pauseCanvasGroup.alpha = endAlpha;

        if (isOpening)
        {
            if (panelRectTransform != null) panelRectTransform.localScale = Vector3.one;
            Time.timeScale = 0f;
        }
        else
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        LevelData.NextLevelName = "MainMenu";
        SceneManager.LoadScene("LoadingScene");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        isPaused = false;
        LevelData.NextLevelName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("LoadingScene");
    }

    private Coroutine settingsCoroutine;

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            if (settingsCanvasGroup != null)
            {
                if (settingsCoroutine != null) StopCoroutine(settingsCoroutine);
                settingsCoroutine = StartCoroutine(FadeSettingsPanel(0f, 1f, true));
            }
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            if (settingsCanvasGroup != null)
            {
                if (settingsCoroutine != null) StopCoroutine(settingsCoroutine);
                settingsCoroutine = StartCoroutine(FadeSettingsPanel(1f, 0f, false));
            }
            else
            {
                settingsPanel.SetActive(false);
            }
        }
    }

    private IEnumerator FadeSettingsPanel(float startAlpha, float endAlpha, bool isOpening)
    {
        float elapsedTime = 0f;
        settingsCanvasGroup.alpha = startAlpha;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; 
            settingsCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        settingsCanvasGroup.alpha = endAlpha;

        if (!isOpening)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}