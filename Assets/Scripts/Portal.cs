using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class Portal : MonoBehaviour
{
    public string targetLevelName; 
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1f;

    [Header("Portal Audio Settings")]
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip idleSound;

    private AudioSource audioSource;
    private bool isTransitioning = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }

        if (idleSound != null)
        {
            audioSource.clip = idleSound;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTransitioning)
        {
            StartCoroutine(TransitionSequence());
        }
    }

    IEnumerator TransitionSequence()
    {
        isTransitioning = true;

        StartCoroutine(FadeOutPortalSFX());

        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null) pc.enabled = false;

        PlayerHealth ph = FindObjectOfType<PlayerHealth>();
        if (ph != null)
        {
            LevelData.StoredHealth = ph.currentHealth;
        }

        yield return StartCoroutine(Fade(1));

        LevelData.NextLevelName = targetLevelName;
        yield return new WaitForEndOfFrame();
        
        SceneManager.LoadScene("LoadingScene");
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

    IEnumerator FadeOutPortalSFX()
    {
        if (audioSource == null) yield break;

        float startVolume = audioSource.volume;
        float timer = 0;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            yield return null;
        }
        audioSource.Stop();
    }
}