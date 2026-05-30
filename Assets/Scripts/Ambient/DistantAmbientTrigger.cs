using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Dibutuhkan untuk menggunakan List

[RequireComponent(typeof(AudioSource))]
public class DistantAmbientTrigger : MonoBehaviour
{
    [Header("Audio Clips Array")]
    [Tooltip("Masukkan beberapa variasi suara serigala di sini")]
    [SerializeField] private AudioClip[] wolfSounds;

    [Header("Timer Settings (Dalam Detik)")]
    [Tooltip("Waktu minimal untuk jeda antar suara")]
    [SerializeField] private float minInterval = 30f;
    [Tooltip("Waktu maksimal untuk jeda antar suara")]
    [SerializeField] private float maxInterval = 60f;

    [Header("Audio Tuning")]
    [Range(0f, 1f)] [SerializeField] private float volume = 0.7f;
    [Tooltip("Sedikit mengacak nada agar suara tidak monoton")]
    [Range(0f, 0.2f)] [SerializeField] private float pitchRandomness = 0.05f;

    private AudioSource audioSource;
    private int lastPlayedIndex = -1; // Menyimpan index suara terakhir agar tidak terulang dua kali berturut-turut

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false; // Harus false karena kita mengontrol putarannya lewat script

        // Pastikan ada file audio di dalam array sebelum memulai
        if (wolfSounds != null && wolfSounds.Length > 0)
        {
            StartCoroutine(AmbientSoundRoutine());
        }
        else
        {
            Debug.LogWarning("Array Wolf Sounds masih kosong di GameObject: " + gameObject.name);
        }
    }

    private IEnumerator AmbientSoundRoutine()
    {
        // Berikan jeda acak pertama kali saat game baru mulai agar tidak langsung berbunyi mendadak
        float initialDelay = Random.Range(minInterval, maxInterval);
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            PlayRandomSound();

            // Tentukan durasi tunggu secara acak untuk suara berikutnya
            float randomDelay = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(randomDelay);
        }
    }

    private void PlayRandomSound()
    {
        if (wolfSounds.Length == 0 || audioSource == null) return;

        int randomIndex = 0;

        // Jika isi array lebih dari 1, kita cegah agar suara yang sama tidak berbunyi dua kali berturut-turut
        if (wolfSounds.Length > 1)
        {
            do
            {
                randomIndex = Random.Range(0, wolfSounds.Length);
            } while (randomIndex == lastPlayedIndex);
        }
        else
        {
            randomIndex = 0;
        }

        lastPlayedIndex = randomIndex;
        AudioClip clipToPlay = wolfSounds[randomIndex];

        if (clipToPlay != null)
        {
            // Variasikan sedikit pitch (nada) agar setiap lolongan terasa unik
            audioSource.pitch = 1f + Random.Range(-pitchRandomness, pitchRandomness);
            
            // Putar suara
            audioSource.PlayOneShot(clipToPlay, volume);
        }
    }
}
