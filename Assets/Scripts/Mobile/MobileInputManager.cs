using UnityEngine;

public class MobileInputManager : MonoBehaviour
{
    public static MobileInputManager Instance;

    [Header("Movement & Look")]
    public Vector2 moveInput;
    public Vector2 lookInput;

    [Header("Action States")]
    public bool jumpPressed;
    public bool crouchPressed;
    public bool interactPressed;

    [Header("Combat States")]
    public bool attack1Pressed;
    public bool attack2Pressed;
    public bool attack3Pressed;
    public bool kickPressed;
    public bool blockHeld;

    private PlayerCombat playerCombat;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        playerCombat = FindObjectOfType<PlayerCombat>();
        playerHealth = FindObjectOfType<PlayerHealth>();
    }

    private void LateUpdate()
    {
        if (playerHealth != null && playerHealth.isDead)
        {
            ResetAllInputs();
            return;
        }

        // Jika tutorial aktif, paksa reset semua input setiap frame
        if (TutorialManager.isTutorialActive)
        {
            ResetAllInputs();
            return;
        }

        jumpPressed = false;
        interactPressed = false;
        attack1Pressed = false;
        attack2Pressed = false;
        attack3Pressed = false;
        kickPressed = false;
        
        lookInput = Vector2.zero;
    }

    public void ResetAllInputs()
    {
        moveInput = Vector2.zero;
        lookInput = Vector2.zero;
        jumpPressed = false;
        crouchPressed = false;
        interactPressed = false;
        attack1Pressed = false;
        attack2Pressed = false;
        attack3Pressed = false;
        kickPressed = false;
        blockHeld = false;
    }

    // --- METHODS UNTUK TOMBOL UI ---
    // Semua method ini sekarang cek isTutorialActive agar sentuhan dari
    // tombol Start tidak bocor ke kontrol game

    public void OnJumpPressed()
    {
        if (TutorialManager.isTutorialActive) return;
        jumpPressed = true;
    }

    public void OnInteractPressed()
    {
        if (TutorialManager.isTutorialActive) return;
        interactPressed = true;
    }

    public void OnAttack1Pressed()
    {
        if (TutorialManager.isTutorialActive) return;
        attack1Pressed = true;
    }

    public void OnAttack2Pressed()
    {
        if (TutorialManager.isTutorialActive) return;
        attack2Pressed = true;
    }

    public void OnAttack3Pressed()
    {
        if (TutorialManager.isTutorialActive) return;
        attack3Pressed = true;
    }

    public void OnKickPressed()
    {
        if (TutorialManager.isTutorialActive) return;
        kickPressed = true;
    }
    
    public void SetBlockHeld(bool state)
    {
        if (TutorialManager.isTutorialActive) { blockHeld = false; return; }
        blockHeld = state;
    }

    public void OnCrouchToggle()
    {
        if (TutorialManager.isTutorialActive) return;
        crouchPressed = !crouchPressed;
        if (!crouchPressed && playerCombat != null)
        {
            playerCombat.ResetAllTriggers();
        }
    }
}