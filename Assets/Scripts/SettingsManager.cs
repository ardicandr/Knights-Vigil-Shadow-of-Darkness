using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro; // 1. WAJIB DISEDIAKAN UNTUK TMPro

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Mixer & Routing")]
    [Tooltip("Masukkan MainAudioMixer yang telah dibuat di sini")]
    public AudioMixer mainAudioMixer;
    
    [Tooltip("Pilih grup Music dari Audio Mixer Anda")]
    public AudioMixerGroup musicGroup;
    
    [Tooltip("Pilih grup SFX dari Audio Mixer Anda")]
    public AudioMixerGroup sfxGroup;

    [Header("UI Sliders")]
    [Tooltip("Slider dengan Min Value 0.1 dan Max Value 5 (atau sesuai selera)")]
    public Slider sensitivitySlider;
    
    [Tooltip("Slider dengan Min Value 0.0001 dan Max Value 1")]
    public Slider musicSlider;
    
    [Tooltip("Slider dengan Min Value 0.0001 dan Max Value 1")]
    public Slider sfxSlider;

    [Header("UI Toggles & Dropdowns")]
    public Toggle invertYToggle;
    public TMP_Dropdown graphicsDropdown; // 2. DIUBAH DARI Dropdown MENJADI TMP_Dropdown

    [Header("Default Values")]
    public float defaultSensitivity = 1f;
    public float defaultMusicVolume = 1f; // Linear (0.0001 - 1)
    public float defaultSFXVolume = 1f; // Linear (0.0001 - 1)
    public bool defaultInvertY = false;
    public int defaultGraphicsQuality = 2; // Asumsi: 2 = High (tergantung QualitySettings Unity Anda)

    private float routeTimer = 0f;

    private void Start()
    {
        LoadSettings();
        AutoRouteAudioSources();
        
        // Add listeners to sliders automatically
        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        }
        
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (invertYToggle != null)
        {
            invertYToggle.onValueChanged.AddListener(SetInvertY);
        }

        if (graphicsDropdown != null)
        {
            graphicsDropdown.onValueChanged.AddListener(SetGraphicsQuality);
        }
    }

    private void LoadSettings()
    {
        // 1. Load Sensitivity
        float savedSensitivity = PlayerPrefs.GetFloat("Sensitivity", defaultSensitivity);
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSensitivity;
        }
        // Save back initial to ensure we have a value
        PlayerPrefs.SetFloat("Sensitivity", savedSensitivity);

        // 2. Load Music Volume
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
        if (musicSlider != null)
        {
            musicSlider.value = savedMusic;
        }
        SetMusicVolume(savedMusic); // Apply immediately to mixer

        // 3. Load SFX Volume
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", defaultSFXVolume);
        if (sfxSlider != null)
        {
            sfxSlider.value = savedSFX;
        }
        SetSFXVolume(savedSFX); // Apply immediately to mixer

        // 4. Load Invert Y
        int savedInvert = PlayerPrefs.GetInt("InvertY", defaultInvertY ? 1 : 0);
        if (invertYToggle != null)
        {
            invertYToggle.isOn = (savedInvert == 1);
        }
        SetInvertY(savedInvert == 1);

        // 5. Load Graphics Quality
        int savedGraphics = PlayerPrefs.GetInt("GraphicsQuality", defaultGraphicsQuality);
        if (graphicsDropdown != null)
        {
            graphicsDropdown.value = savedGraphics;
        }
        SetGraphicsQuality(savedGraphics);
    }

    public void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat("Sensitivity", value);
        
        // Jika ada PlayerController di scene (saat in-game pause menu), langsung update nilainya
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.mobileLookSensitivity = value;
        }
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        
        if (mainAudioMixer != null)
        {
            // Mengkonversi nilai linear (0.0001 ke 1) menjadi logaritmik (dB) untuk AudioMixer
            float dbVolume = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
            mainAudioMixer.SetFloat("MusicVolume", dbVolume);
        }
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        
        if (mainAudioMixer != null)
        {
            // Mengkonversi nilai linear (0.0001 ke 1) menjadi logaritmik (dB) untuk AudioMixer
            float dbVolume = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
            mainAudioMixer.SetFloat("SFXVolume", dbVolume);
        }
    }

    public void SetInvertY(bool isON)
    {
        PlayerPrefs.SetInt("InvertY", isON ? 1 : 0);
        
        // Update in real-time jika ada player di scene
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.invertYAxis = isON;
        }
    }

    public void SetGraphicsQuality(int qualityIndex)
    {
        PlayerPrefs.SetInt("GraphicsQuality", qualityIndex);
        QualitySettings.SetQualityLevel(qualityIndex);
    }
    
    [Header("Panel Reference (Untuk Close)")]
    [Tooltip("Opsional: Masukkan GameObject Panel Setting ini sendiri. Berguna jika skrip tidak dipasang langsung di panel.")]
    public GameObject settingsPanelObject;

    public void ClosePanel()
    {
        // Jika settingsPanelObject diisi, matikan objek tersebut
        if (settingsPanelObject != null)
        {
            settingsPanelObject.SetActive(false);
        }
        else 
        {
            // Jika kosong, matikan objek tempat skrip ini menempel
            gameObject.SetActive(false);
        }
    }

    private void AutoRouteAudioSources()
    {
        if (musicGroup == null || sfxGroup == null) return;

        // Cari semua komponen AudioSource di scene (hanya dieksekusi sekali saat Start untuk menghindari lag)
        AudioSource[] allSources = FindObjectsOfType<AudioSource>(true);
        foreach (AudioSource source in allSources)
        {
            // Hanya sambungkan jika output belum disetel sama sekali (None)
            if (source.outputAudioMixerGroup == null)
            {
                string objName = source.gameObject.name.ToLower();
                
                // Heuristik: Jika namanya mengandung music, bgm, atau ambient, jadikan Music
                if (objName.Contains("music") || objName.Contains("bgm") || objName.Contains("ambient"))
                {
                    source.outputAudioMixerGroup = musicGroup;
                }
                else
                {
                    // Sisanya otomatis jadi SFX
                    source.outputAudioMixerGroup = sfxGroup;
                }
            }
        }
    }
    
    private void OnDisable()
    {
        // Menyimpan nilai ke memori hanya saat panel ditutup/dinonaktifkan
        PlayerPrefs.Save();
    }
    
    private void OnDestroy()
    {
        PlayerPrefs.Save();
        
        // Bersihkan listener untuk mencegah memory leak
        if (sensitivitySlider != null) sensitivitySlider.onValueChanged.RemoveListener(SetSensitivity);
        if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);
        if (invertYToggle != null) invertYToggle.onValueChanged.RemoveListener(SetInvertY);
        if (graphicsDropdown != null) graphicsDropdown.onValueChanged.RemoveListener(SetGraphicsQuality);
    }
}