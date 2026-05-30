using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class InteractableChest : MonoBehaviour, IInteractable
{
    [Header("References")]
    public Transform lidEngsel;
    public float openAngle = -90f;  
    public float openSpeed = 2f;
    public float healAmount = 30f;

    [Header("Audio Settings")]
    [Tooltip("Masukkan SFX suara peti kayu kuno terbuka berat")]
    [SerializeField] private AudioClip openChestSFX;

    public bool IsOpened { get; private set; } = false;
    
    private Quaternion targetRotation;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        IsOpened = false;
    }

    public void Interact()
    {
        if (IsOpened) return;
        IsOpened = true;

        targetRotation = lidEngsel.localRotation * Quaternion.Euler(0, 0, openAngle);
        
        if (audioSource != null && openChestSFX != null)
        {
            audioSource.PlayOneShot(openChestSFX);
        }

        PlayerHealth player = FindObjectOfType<PlayerHealth>();
        if (player != null) {
            player.RestoreHealth(healAmount);
        }

        Debug.Log("Peti Terbuka ke Atas menggunakan Sumbu Z!");
    }

    void Update()
    {
        if (IsOpened)
        {
            lidEngsel.localRotation = Quaternion.Slerp(lidEngsel.localRotation, targetRotation, Time.deltaTime * openSpeed);
        }
    }
}