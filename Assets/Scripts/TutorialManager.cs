using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("UI Objects")]
    public GameObject tutorialPanel;
    public GameObject[] tutorialPages; // Tarik Group 1, Group 2, Group 3 ke sini
    public TextMeshProUGUI nextButtonText; // Tarik Teks tombol Next ke sini

    [Header("Settings")]
    public string nextLabel = "NEXT";
    public string startLabel = "START";

    public static bool isTutorialActive = false;
    private int currentIndex = 0;

    void Start()
    {
        // Logika muncul satu kali (PlayerPrefs)
        int hasSeen = PlayerPrefs.GetInt("HasSeenTutorial", 0);

        if (hasSeen == 0)
        {
            ShowTutorial();
        }
        else
        {
            tutorialPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void ShowTutorial()
    {
        isTutorialActive = true;
        tutorialPanel.SetActive(true);
        Time.timeScale = 0f; 
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        currentIndex = 0;
        UpdatePageVisibility();
    }

    // Fungsi yang dipanggil saat tombol Next diklik
    public void HandleNextButton()
    {
        currentIndex++;

        if (currentIndex < tutorialPages.Length)
        {
            UpdatePageVisibility();
        }
        else
        {
            // Jika sudah lewat halaman terakhir, tutup tutorial
            CloseTutorial();
        }
    }

    void UpdatePageVisibility()
    {
        // Matikan semua halaman, nyalakan yang sedang aktif
        for (int i = 0; i < tutorialPages.Length; i++)
        {
            tutorialPages[i].SetActive(i == currentIndex);
        }

        // Cek apakah sekarang halaman terakhir
        if (currentIndex == tutorialPages.Length - 1)
        {
            nextButtonText.text = startLabel;
        }
        else
        {
            nextButtonText.text = nextLabel;
        }
    }

    public void CloseTutorial()
    {
        PlayerPrefs.SetInt("HasSeenTutorial", 1);
        PlayerPrefs.Save();

        // Gunakan coroutine agar sentuhan dari tombol Start selesai dulu
        // sebelum kontrol game diaktifkan kembali
        StartCoroutine(CloseTutorialRoutine());
    }

    private IEnumerator CloseTutorialRoutine()
    {
        // LANGKAH 1: Sembunyikan panel tutorial, tapi JANGAN aktifkan kontrol dulu
        // isTutorialActive tetap TRUE agar PlayerController & PlayerCombat tidak proses input
        tutorialPanel.SetActive(false);
        Time.timeScale = 1f;

        // Cursor handling harus sama dengan PauseMenuController.Resume()
        // Pada mobile, CursorLockMode.Locked bisa mengganggu drag event (joystick/touchpad)
        // sehingga harus tetap CursorLockMode.None
        PlayerController playerScript = FindObjectOfType<PlayerController>();
        if (playerScript != null && playerScript.hideCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // LANGKAH 2: Reset semua input mobile segera
        ResetAllMobileInputs();

        // LANGKAH 3: KUNCI PERBAIKAN - Reset InputModule agar pointer tracking
        // dari sentuhan tombol Start dibersihkan total.
        // Tanpa ini, Unity InputModule menganggap ada "pointer aktif" yang stuck
        // sehingga joystick & touchpad tidak bisa menerima sentuhan baru.
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);

            // DeactivateModule + ActivateModule memaksa InputModule
            // membersihkan semua internal pointer data (m_PointerData dictionary)
            var inputModule = EventSystem.current.currentInputModule;
            if (inputModule != null)
            {
                inputModule.DeactivateModule();
                inputModule.ActivateModule();
            }
        }

        // LANGKAH 4: Tunggu 1 frame agar InputModule reset selesai
        yield return null;

        // LANGKAH 5: Reset input sekali lagi setelah InputModule restart
        ResetAllMobileInputs();

        // LANGKAH 6: Reset posisi handle joystick secara visual
        MobileJoystick joystick = FindObjectOfType<MobileJoystick>();
        if (joystick != null && joystick.handle != null)
        {
            joystick.handle.anchoredPosition = Vector2.zero;
        }

        // LANGKAH 7: Pastikan state pause tidak nyangkut
        PauseMenuController.isPaused = false;

        // LANGKAH 8: TERAKHIR baru izinkan kontrol game berjalan
        isTutorialActive = false;
    }

    private void ResetAllMobileInputs()
    {
        if (MobileInputManager.Instance != null)
        {
            MobileInputManager.Instance.moveInput = Vector2.zero;
            MobileInputManager.Instance.lookInput = Vector2.zero;
            MobileInputManager.Instance.jumpPressed = false;
            MobileInputManager.Instance.crouchPressed = false;
            MobileInputManager.Instance.interactPressed = false;
            MobileInputManager.Instance.attack1Pressed = false;
            MobileInputManager.Instance.attack2Pressed = false;
            MobileInputManager.Instance.attack3Pressed = false;
            MobileInputManager.Instance.kickPressed = false;
            MobileInputManager.Instance.blockHeld = false;
        }
    }
}