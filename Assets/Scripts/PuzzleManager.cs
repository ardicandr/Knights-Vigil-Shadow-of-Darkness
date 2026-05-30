using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PuzzleManager : MonoBehaviour
{
    [Header("Urutan Kunci Jawaban (Sesuai ID Pilar)")]
    public List<int> kunciJawaban = new List<int>(); 

    [Header("Pengaturan Jeda Reset")]
    [Tooltip("Waktu tunggu (detik) agar pilar terakhir sempat menyala sebelum semuanya mati")]
    public float jedaResetWaktu = 1.0f;

    [Header("Puzzle Audio Settings")]
    [SerializeField] private AudioClip successSound;

    private List<int> urutanInputPemain = new List<int>();
    private bool puzzleSelesai = false;
    private bool sedangMengulang = false; 
    private LevelManager levelManager;
    private AudioSource audioSource;

    void Start()
    {
        levelManager = FindObjectOfType<LevelManager>();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void PilarDipukul(int idPilar)
    {
        if (puzzleSelesai || sedangMengulang) return;

        urutanInputPemain.Add(idPilar);
        Debug.Log("Pemain mengaktifkan pilar ID: " + idPilar);

        if (urutanInputPemain.Count == kunciJawaban.Count)
        {
            PeriksaJawaban();
        }
    }

    public bool IsSedangMengulang()
    {
        return sedangMengulang || puzzleSelesai;
    }

    void PeriksaJawaban()
    {
        for (int i = 0; i < kunciJawaban.Count; i++)
        {
            if (urutanInputPemain[i] != kunciJawaban[i])
            {
                StartCoroutine(ProsesJawabanSalahDenganJeda());
                return;
            }
        }

        JawabanBenar();
    }

    void JawabanBenar()
    {
        puzzleSelesai = true;
        Debug.Log("<color=cyan>Puzzle Sukses! Melapor ke Level Manager.</color>");
        
        // Putar SFX Sukses
        if (audioSource != null && successSound != null)
        {
            audioSource.PlayOneShot(successSound);
        }

        if (levelManager != null)
        {
            levelManager.PuzzleCompleted();
        }
    }

    IEnumerator ProcessJawabanSalahDenganJeda()
    {
        yield return ProsesJawabanSalahDenganJeda();
    }

    IEnumerator ProsesJawabanSalahDenganJeda()
    {
        sedangMengulang = true; 
        Debug.Log("Urutan Salah! Menunggu pilar terakhir menyala...");

        yield return new WaitForSeconds(jedaResetWaktu);

        Debug.Log("Mereset Semua Pilar Sekarang!");
        urutanInputPemain.Clear();

        PilarPuzzle[] semuaPilar = FindObjectsOfType<PilarPuzzle>();
        foreach (PilarPuzzle pilar in semuaPilar)
        {
            pilar.ResetPilar();
        }

        sedangMengulang = false; 
    }
}