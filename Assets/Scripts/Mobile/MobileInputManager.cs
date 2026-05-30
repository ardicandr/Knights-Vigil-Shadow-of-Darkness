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

        jumpPressed = false;
        interactPressed = false;
        attack1Pressed = false;
        attack2Pressed = false;
        attack3Pressed = false;
        kickPressed = false;
        
        lookInput = Vector2.zero;
    }

    private void ResetAllInputs()
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
    public void OnJumpPressed()     => jumpPressed = true;
    public void OnInteractPressed() => interactPressed = true;
    public void OnAttack1Pressed()  => attack1Pressed = true;
    public void OnAttack2Pressed()  => attack2Pressed = true;
    public void OnAttack3Pressed()  => attack3Pressed = true;
    public void OnKickPressed()     => kickPressed = true;
    
    public void SetBlockHeld(bool state) => blockHeld = state;

    public void OnCrouchToggle()
    {
        crouchPressed = !crouchPressed;
        if (!crouchPressed && playerCombat != null)
        {
            playerCombat.ResetAllTriggers();
        }
    }
}