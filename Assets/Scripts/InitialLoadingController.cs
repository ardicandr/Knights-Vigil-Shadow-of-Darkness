using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class InitialLoadingController : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider progressBar;
    public Text percentText;
    public CanvasGroup faderGroup;

    [Header("Settings")]
    public string targetScene = "MainMenu";
    public float minLoadingTime = 3f;
    public float fadeDuration = 1f;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        StartCoroutine(LoadProcess());
    }

    IEnumerator LoadProcess()
    {
        // 1. FADE IN (Hitam ke Terang)
        yield return StartCoroutine(Fade(0));

        float startTime = Time.time;
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        operation.allowSceneActivation = false;

        float visualProgress = 0f;

        while (!operation.isDone)
        {
            // Ambil progress asli (0 - 0.9)
            float actualProgress = Mathf.Clamp01(operation.progress / 0.9f);
            
            // Gerakkan progress bar secara halus
            visualProgress = Mathf.MoveTowards(visualProgress, actualProgress, Time.deltaTime * 0.5f);

            if (progressBar != null) progressBar.value = visualProgress;
            if (percentText != null) percentText.text = (visualProgress * 100f).ToString("F0") + "%";

            float elapsedTime = Time.time - startTime;

            // Jika loading internal selesai & waktu minimal tercapai & bar penuh
            if (operation.progress >= 0.9f && elapsedTime >= minLoadingTime && visualProgress >= 0.99f)
            {
                // 2. FADE OUT (Terang ke Hitam)
                yield return StartCoroutine(Fade(1));
                
                // Pindah ke MainMenu
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    // Fungsi Fade Mandiri (Hanya untuk scene ini)
    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = faderGroup.alpha;
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            faderGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }
        faderGroup.alpha = targetAlpha;
    }
}