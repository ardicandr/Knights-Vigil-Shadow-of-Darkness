using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class InteractableInfoBoard : MonoBehaviour, IInteractable
{
    [Header("UI Reference")]
    [SerializeField] private GameObject infoBoardPanel; 
    [SerializeField] private CanvasGroup panelCanvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("Audio Settings (Dark Fantasy SFX)")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    
    private AudioSource audioSource;
    public bool isOpen = false;
    public static bool isAnyBoardOpen = false;
    private Coroutine fadeCoroutine;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        audioSource.playOnAwake = false;

        if (infoBoardPanel != null && !isOpen)
        {
            if (panelCanvasGroup != null) panelCanvasGroup.alpha = 0f;
            infoBoardPanel.SetActive(false);
        }
    }

    public void Interact()
    {
        if (infoBoardPanel == null || panelCanvasGroup == null) {
            Debug.LogError("Info Board Panel atau Canvas Group belum diisi di Inspector!");
            return;
        }

        if (!isOpen) OpenPanel();
        else ClosePanel();
    }

    public void OpenPanel()
    {
        isOpen = true;
        isAnyBoardOpen = true;
        
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        
        fadeCoroutine = StartCoroutine(FadeUI(0f, 1f, true));

        PlaySFX(openSound);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ClosePanel()
    {
        isOpen = false;
        isAnyBoardOpen = false;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeUI(1f, 0f, false));

        // MAINKAN SUARA TUTUP
        PlaySFX(closeSound);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private IEnumerator FadeUI(float startAlpha, float endAlpha, bool isOpening)
    {
        if (isOpening)
        {
            infoBoardPanel.SetActive(true);
            infoBoardPanel.transform.localScale = Vector3.one * 0.95f; 
        }

        float elapsedTime = 0f;
        panelCanvasGroup.alpha = startAlpha;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; 
            float progress = elapsedTime / fadeDuration;

            panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);

            if (isOpening)
            {
                infoBoardPanel.transform.localScale = Vector3.Lerp(Vector3.one * 0.95f, Vector3.one, progress);
            }
            else
            {
                infoBoardPanel.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.95f, progress);
            }

            yield return null;
        }

        panelCanvasGroup.alpha = endAlpha;

        if (isOpening)
        {
            infoBoardPanel.transform.localScale = Vector3.one;
            Time.timeScale = 0f;
        }
        else
        {
            infoBoardPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}