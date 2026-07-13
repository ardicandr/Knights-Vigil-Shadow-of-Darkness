using UnityEngine;
using System.Collections;

public class CreditsAnimator : MonoBehaviour
{
    [Header("Urutan Bagian Credits")]
    public CanvasGroup[] creditSections;

    [Header("Timing Settings")]
    public float initialDelay = 1.0f;    // Waktu tunggu agar panel utama selesai fade-in dulu
    public float fadeSpeed = 1.5f;       
    public float delayBetween = 0.4f;   

    [Header("Slide Settings")]
    public float slideOffset = 50f;     // Jarak meluncur (pixel). Positif = dari atas ke bawah.
    
    private Vector2[] originalPositions;

    private void Awake()
    {
        // Simpan posisi asli semua group saat pertama kali game jalan
        originalPositions = new Vector2[creditSections.Length];
        for (int i = 0; i < creditSections.Length; i++)
        {
            originalPositions[i] = creditSections[i].GetComponent<RectTransform>().anchoredPosition;
        }
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        ResetCredits();
        StartCoroutine(AnimateCredits());
    }

    void ResetCredits()
    {
        for (int i = 0; i < creditSections.Length; i++)
        {
            creditSections[i].alpha = 0f;
            // Kembalikan posisi ke posisi asli + offset (di atas posisi seharusnya)
            RectTransform rect = creditSections[i].GetComponent<RectTransform>();
            rect.anchoredPosition = originalPositions[i] + new Vector2(0, slideOffset);
        }
    }

    IEnumerator AnimateCredits()
    {
        // ✅ JEDA AWAL: Menunggu panel credits selesai terbuka
        yield return new WaitForSeconds(initialDelay);

        for (int i = 0; i < creditSections.Length; i++)
        {
            StartCoroutine(FadeAndSlide(creditSections[i], originalPositions[i]));
            
            // Tunggu sebentar sebelum memulai bagian berikutnya (Sequential)
            yield return new WaitForSeconds(delayBetween);
        }
    }

    IEnumerator FadeAndSlide(CanvasGroup group, Vector2 targetPos)
    {
        RectTransform rect = group.GetComponent<RectTransform>();
        Vector2 startPos = targetPos + new Vector2(0, slideOffset);
        
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            
            // Gunakan SmoothStep agar gerakan meluncur terasa elegan (tidak kaku)
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // 1. Update Alpha (Fade In)
            group.alpha = t;

            // 2. Update Position (Slide Down)
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothT);

            yield return null;
        }

        group.alpha = 1f;
        rect.anchoredPosition = targetPos;
    }
}