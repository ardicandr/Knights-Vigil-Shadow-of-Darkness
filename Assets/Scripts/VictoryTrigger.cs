using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VictoryTrigger : MonoBehaviour
{
    [Header("UI Fading Screen (Opsional)")]
    [Tooltip("Panel UI hitam yang digunakan untuk efek transisi Fade to Black")]
    public CanvasGroup blackFadePanel;
    public float fadeDuration = 1.0f;

    [Header("Hidden Objects (Kastil)")]
    [Tooltip("Masukkan parent GameObject dari semua NPC/Objek yang disembunyikan di dalam kastil")]
    public GameObject hiddenNPCsParent;

    [Header("Teleport Settings")]
    [Tooltip("Titik teleportasi di dalam kastil untuk player")]
    public Transform insideCastleSpawnPoint;

    [Header("UI Settings")]
    [Tooltip("Panel/UI Kemenangan yang muncul di akhir")]
    public GameObject victoryUI;
    [Tooltip("Berapa lama (detik) UI kemenangan akan tampil sebelum menghilang?")]
    public float victoryUIDisplayTime = 3.5f;

    private bool isTriggered = false;

    void Start()
    {
        if (hiddenNPCsParent != null) hiddenNPCsParent.SetActive(false);
        if (victoryUI != null) victoryUI.SetActive(false);
        
        if (blackFadePanel != null)
        {
            blackFadePanel.alpha = 0f;
            blackFadePanel.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player"))
        {
            isTriggered = true;

            // Matikan collider agar tidak ter-trigger dua kali
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Sembunyikan efek visual portal/particle (semua objek di dalam portal ini)
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
            
            // Matikan ParticleSystem jika ada di parent
            ParticleSystem ps = GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop();

            StartCoroutine(VictorySequence(other.gameObject));
        }
    }

    IEnumerator VictorySequence(GameObject player)
    {
        // 1. Reset input mobile agar tidak nyangkut saat lari
        if (MobileInputManager.Instance != null)
        {
            MobileInputManager.Instance.moveInput = Vector2.zero;
            MobileInputManager.Instance.lookInput = Vector2.zero;
        }

        // 2. Hentikan gerakan dan paksa animasi ke Idle
        PlayerController pc = player.GetComponent<PlayerController>();
        CharacterController cc = player.GetComponent<CharacterController>();
        Animator anim = player.GetComponentInChildren<Animator>();

        if (cc != null) cc.Move(Vector3.zero);
        if (anim != null)
        {
            anim.SetFloat("InputX", 0f);
            anim.SetFloat("InputZ", 0f);
            anim.SetFloat("isCrouchingFloat", 0f);
        }
        
        // Disable kontrol agar tidak bisa gerak selama cutscene
        if (pc != null) pc.enabled = false;

        // 3. FADE TO BLACK
        if (blackFadePanel != null)
        {
            blackFadePanel.gameObject.SetActive(true);
            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                blackFadePanel.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }
            blackFadePanel.alpha = 1f;
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        // --- DI BALIK LAYAR HITAM ---

        // 4. Munculkan NPC Paladin
        if (hiddenNPCsParent != null)
        {
            hiddenNPCsParent.SetActive(true);
        }

        // 5. Pindahkan posisi player (Teleport)
        if (insideCastleSpawnPoint != null)
        {
            if (cc != null) cc.enabled = false;
            
            player.transform.position = insideCastleSpawnPoint.position;
            player.transform.rotation = insideCastleSpawnPoint.rotation;
            
            if (cc != null) cc.enabled = true;
        }

        yield return new WaitForSeconds(0.5f);

        // 6. FADE IN
        if (blackFadePanel != null)
        {
            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                blackFadePanel.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                yield return null;
            }
            blackFadePanel.alpha = 0f;
            blackFadePanel.gameObject.SetActive(false);
        }

        // 7. Tampilkan UI Kemenangan dengan efek Fade
        if (victoryUI != null)
        {
            victoryUI.SetActive(true);
            
            // Coba ambil CanvasGroup, jika belum ada, otomatis tambahkan
            CanvasGroup cg = victoryUI.GetComponent<CanvasGroup>();
            if (cg == null) cg = victoryUI.AddComponent<CanvasGroup>();
            
            float fadeTime = 0.5f;
            float t = 0;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                cg.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
                yield return null;
            }
            cg.alpha = 1f;
        }

        // 8. Tunggu selama beberapa detik sesuai permintaan
        yield return new WaitForSeconds(victoryUIDisplayTime);

        // 9. Sembunyikan UI Kemenangan (Fade Out) dan KEMBALIKAN KONTROL
        if (victoryUI != null)
        {
            CanvasGroup cg = victoryUI.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                float fadeTime = 0.5f;
                float t = 0;
                while (t < fadeTime)
                {
                    t += Time.deltaTime;
                    cg.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
                    yield return null;
                }
                cg.alpha = 0f;
            }
            victoryUI.SetActive(false);
        }
        
        if (pc != null) pc.enabled = true; // Player bisa bergerak bebas lagi di dalam kastil
    }
}
