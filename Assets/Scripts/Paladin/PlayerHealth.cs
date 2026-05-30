using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Game Over UI & Animation")]
    public GameObject gameOverPanel;
    [Tooltip("Masukkan CanvasGroup yang menempel pada Game Over Panel")]
    public CanvasGroup gameOverCanvasGroup;
    [Tooltip("Jeda waktu tunggu (detik) sebelum panel mulai muncul setelah mati")]
    public float delayBeforeShow = 2.5f;
    [Tooltip("Durasi animasi transisi panel")]
    public float animationDuration = 0.8f;
    [Tooltip("Jarak piksel posisi awal panel di atas layar sebelum meluncur turun")]
    public float startTopOffset = 300f;

    [Header("Health Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI References")]
    public Slider healthSlider;
    public float lerpSpeed = 5f;

    [Header("VFX")]
    public Image damageImage;
    public float flashDuration = 0.1f;
    public float fadeSpeed = 2f;

    [Header("References")]
    public Animator anim;
    public PlayerController moveScript;
    public PlayerCombat combatScript;
    public CharacterController charController;

    [Header("Shield & Player Audio Settings")]
    public AudioSource shieldSource;
    public AudioClip shieldImpactSFX;
    [Tooltip("Masukkan beberapa variasi suara erangan sakit saat terkena hit")]
    public AudioClip[] damageSFXArray;
    [Tooltip("SFX suara maut saat player mati")]
    public AudioClip deathSFX;
    [Tooltip("SFX saat panel tulisan Game Over atau You Died mulai muncul di layar")]
    public AudioClip gameOverAppearSFX;

    [HideInInspector] public bool isDead = false;
    private Coroutine flashCoroutine; 
    private Color maxTargetColor;
    private RectTransform panelRectTransform;
    private int lastPlayedHurtIndex = -1;

    void Start()
    {
        currentHealth = maxHealth;

        if (gameOverPanel != null)
        {
            panelRectTransform = gameOverPanel.GetComponent<RectTransform>();
            
            if (gameOverCanvasGroup != null) gameOverCanvasGroup.alpha = 0f;
            gameOverPanel.SetActive(false);
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }

        if (damageImage != null)
        {
            maxTargetColor = damageImage.color;
            Color startColor = maxTargetColor;
            startColor.a = 0f;
            damageImage.color = startColor;
            damageImage.enabled = false; 
        }
        else
        {
            Debug.LogError("<color=orange>⚠️ WARNING: damageImage belum dimasukkan ke Inspector!</color>");
        }

        if (LevelData.StoredHealth > 0) 
        {
            currentHealth = LevelData.StoredHealth;
        }
        else 
        {
            currentHealth = maxHealth;
        }
        
        if (healthSlider != null) {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    void Update()
    {
        if (healthSlider != null && healthSlider.value != currentHealth)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, currentHealth, Time.deltaTime * lerpSpeed);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        if (anim != null && anim.GetBool("isBlocking"))
        {
            anim.SetTrigger("BlockHit"); 
            
            if (shieldSource != null && shieldImpactSFX != null)
            {
                shieldSource.pitch = 1f;
                shieldSource.PlayOneShot(shieldImpactSFX);
            }

            Debug.Log("<color=yellow>🛡️ TERSERAP: Perisai menahan serangan!</color>");
            return; 
        }
        currentHealth -= amount;
        Debug.Log("<color=red>💥 HIT: Darah berkurang. Sisa: " + currentHealth + "</color>");

        if (currentHealth > 0)
        {
            PlayRandomHurtSound();
        }

        if (damageImage != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FadeOutDamageEffect());
        }

        if (anim != null) anim.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void PlayRandomHurtSound()
    {
        if (shieldSource == null || damageSFXArray == null || damageSFXArray.Length == 0) return;

        int randomIndex = 0;

        if (damageSFXArray.Length > 1)
        {
            do
            {
                randomIndex = Random.Range(0, damageSFXArray.Length);
            } while (randomIndex == lastPlayedHurtIndex);
        }

        lastPlayedHurtIndex = randomIndex;
        AudioClip clipToPlay = damageSFXArray[randomIndex];

        if (clipToPlay != null)
        {
            shieldSource.pitch = Random.Range(0.9f, 1.1f);
            shieldSource.PlayOneShot(clipToPlay);
        }
    }

    IEnumerator FadeOutDamageEffect()
    {
        damageImage.enabled = true;
        damageImage.color = maxTargetColor; 
        yield return new WaitForSeconds(flashDuration);
        Color currentColor = damageImage.color;

        while (currentColor.a > 0f)
        {
            currentColor.a = Mathf.MoveTowards(currentColor.a, 0f, fadeSpeed * Time.deltaTime);
            damageImage.color = currentColor;
            yield return null;
        }
        
        currentColor.a = 0f;
        damageImage.color = currentColor;
        damageImage.enabled = false;
    }

    public void RestoreHealth(float amount)
    {
        if (isDead) return;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log("<color=green>❤️ HEAL: Darah bertambah! Sisa: " + currentHealth + "</color>");
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("<color=black>💀 DEAD: Paladin telah gugur.</color>");

        if (shieldSource != null && deathSFX != null)
        {
            shieldSource.pitch = 1f;
            shieldSource.PlayOneShot(deathSFX);
        }
        
        if (combatScript != null) {
            combatScript.ResetAllTriggers();
        }

        if (anim != null)
        {
            anim.SetFloat("InputX", 0f);
            anim.SetFloat("InputZ", 0f);
            anim.SetBool("isDead", true);
        }

        if (moveScript != null) moveScript.enabled = false;
        if (combatScript != null) combatScript.enabled = false; 
        if (charController != null) charController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(AnimateGameOverUI());
    }

    IEnumerator AnimateGameOverUI()
    {
        yield return new WaitForSeconds(delayBeforeShow);

        if (gameOverPanel == null || gameOverCanvasGroup == null || panelRectTransform == null)
        {
            Debug.LogError("Komponen UI Game Over belum lengkap di Inspector!");
            yield break;
        }

        Vector2 targetAnchoredPosition = panelRectTransform.anchoredPosition; 
        Vector2 startAnchoredPosition = targetAnchoredPosition + new Vector2(0f, startTopOffset);
        
        panelRectTransform.anchoredPosition = startAnchoredPosition;
        gameOverCanvasGroup.alpha = 0f;
        gameOverPanel.SetActive(true);

        // MAINKAN SUARA JINGLE KETIKA PANEL GAME OVER MULAI MUNCUL
        if (shieldSource != null && gameOverAppearSFX != null)
        {
            shieldSource.pitch = 1f; // Pastikan pitch normal kembali
            shieldSource.PlayOneShot(gameOverAppearSFX);
        }

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / animationDuration;

            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            gameOverCanvasGroup.alpha = Mathf.Lerp(0f, 1f, smoothProgress);

            panelRectTransform.anchoredPosition = Vector2.Lerp(startAnchoredPosition, targetAnchoredPosition, smoothProgress);

            yield return null;
        }

        gameOverCanvasGroup.alpha = 1f;
        panelRectTransform.anchoredPosition = targetAnchoredPosition;

        Time.timeScale = 0f; 
    }
}