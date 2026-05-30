using UnityEngine;

public class PlayerCombat : MonoBehaviour {
    [Header("Input Settings")]
    public bool usePCInput = true;

    [Header("References")]
    public Animator anim;
    public WeaponSystem weapon;      
    public WeaponSystem footWeapon;  
    public PlayerController moveScript;

    [Header("Audio Settings")]
    public AudioSource combatSource;
    public AudioClip attack1SFX;
    public AudioClip attack2SFX;
    public AudioClip attack3SFX;
    public AudioClip kickSFX;
    [Range(0, 1)] public float combatVolume = 0.7f;

    [Header("Voice Settings")]
    public AudioClip[] attackVoices;
    [Range(0, 1)] public float voiceChance = 0.5f;

    [Header("Combat Forces (Lunge)")]
    public float forwardLungeForce = 5f;
    public float kickLungeForce = 3f;

    private int upperBodyLayerIndex = 1;
    private int baseLayerIndex = 0;
    private bool lastCrouchState;

    void Update() {
        if (PauseMenuController.isPaused) return;
        
        if (anim.GetBool("isDead")) {
            if (weapon != null) weapon.isAttacking = false;
            return;
        }

        // Deteksi perubahan jongkok untuk reset trigger
        bool currentCrouch = anim.GetBool("isCrouching");
        if (currentCrouch != lastCrouchState) {
            ResetAllTriggers();
            lastCrouchState = currentCrouch;
        }

        if (anim.GetBool("isSitting")) {
            anim.SetBool("isBlocking", false);
            ResetAllTriggers();
            return; 
        }

        HandleBlocking();
        HandleAttacking();
    }

    public void ResetAllTriggers() {
        anim.ResetTrigger("Attack");
        anim.ResetTrigger("Attack2");
        anim.ResetTrigger("Attack3");
        anim.ResetTrigger("Kick");
    }

    void HandleBlocking() {
        bool isBlockingInput = false;

        // Cek PC Input
        if (usePCInput && Input.GetButton("Fire2")) isBlockingInput = true;

        // Cek Mobile Input via Manager
        if (MobileInputManager.Instance != null && MobileInputManager.Instance.blockHeld) isBlockingInput = true;

        anim.SetBool("isBlocking", isBlockingInput);
    }

    public bool CanPerformAction() {
        AnimatorStateInfo upperState = anim.GetCurrentAnimatorStateInfo(upperBodyLayerIndex);
        AnimatorStateInfo baseState = anim.GetCurrentAnimatorStateInfo(baseLayerIndex);

        bool isBaseTransition = anim.IsInTransition(baseLayerIndex); // Ini sudah ada
        bool isUpperTransition = anim.IsInTransition(upperBodyLayerIndex);

        bool isBlocking = anim.GetBool("isBlocking");
        bool isAttacking = !upperState.IsName("Default") && !upperState.IsName("Sembunyi");
        bool isKicking = baseState.IsName("Kick");
        bool isChangingHeight = baseState.IsName("stand_to_crouch") || baseState.IsName("crouch_to_stand");

        return !(isBlocking || isAttacking || isKicking || isUpperTransition || isBaseTransition || isChangingHeight);
    }
    void HandleAttacking() {
        if (!CanPerformAction()) {
            ResetAllTriggers();
            return;
        }

        // --- LOGIKA INPUT GABUNGAN (PC & MOBILE) ---
        bool triggerAttack1 = false;
        bool triggerAttack2 = false;
        bool triggerAttack3 = false;
        bool triggerKick = false;

        // Ambil dari PC jika diaktifkan
        if (usePCInput) {
            if (Input.GetButtonDown("Fire1")) triggerAttack1 = true;
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f) triggerAttack2 = true;
            if (scroll < 0f) triggerAttack3 = true;
            if (Input.GetKeyDown(KeyCode.F)) triggerKick = true;
        }

        // Ambil dari Mobile Manager
        if (MobileInputManager.Instance != null) {
            if (MobileInputManager.Instance.attack1Pressed) triggerAttack1 = true;
            if (MobileInputManager.Instance.attack2Pressed) triggerAttack2 = true;
            if (MobileInputManager.Instance.attack3Pressed) triggerAttack3 = true;
            if (MobileInputManager.Instance.kickPressed)    triggerKick = true;
        }

        // Eksekusi Animasinya
        if (triggerAttack1) { anim.SetTrigger("Attack"); TryPlayAttackVoice(); }
        if (triggerAttack2) { anim.SetTrigger("Attack2"); TryPlayAttackVoice(); }
        if (triggerAttack3) { anim.SetTrigger("Attack3"); }
        if (triggerKick) 
        {
            if (moveScript != null && moveScript.IsGrounded()) 
            {
                anim.SetTrigger("Kick"); 
                TryPlayAttackVoice();
            }
            else 
            {
                anim.ResetTrigger("Kick");
            
            }
        }
    }

    // --- (ANIMATION EVENTS & VOICE) ---
    void LateUpdate() {
        AnimatorStateInfo upperState = anim.GetCurrentAnimatorStateInfo(upperBodyLayerIndex);
        if (upperState.IsName("Default") || upperState.IsName("Sembunyi")) {
            if (weapon != null && weapon.isAttacking) StopAttack(); 
        }
    }

    public void StartAttack() { 
        if (weapon != null) weapon.isAttacking = true; 
        if (moveScript != null) moveScript.ApplyLunge(forwardLungeForce);
        
        if (combatSource != null && attack1SFX != null) combatSource.PlayOneShot(attack1SFX, combatVolume);
    }

    public void StartAttack2() { 
        if (weapon != null) weapon.isAttacking = true; 
        if (moveScript != null) moveScript.ApplyLunge(forwardLungeForce * 0.8f);
        
        if (combatSource != null && attack2SFX != null) combatSource.PlayOneShot(attack2SFX, combatVolume);
    }

    public void StartAttack3() { 
        if (weapon != null) weapon.isAttacking = true; 
        if (moveScript != null) moveScript.ApplyLunge(forwardLungeForce * 1.2f);
        
        if (combatSource != null && attack3SFX != null) combatSource.PlayOneShot(attack3SFX, combatVolume);
    }

    public void StartKick() {
        if (footWeapon != null) footWeapon.isAttacking = true;
        if (moveScript != null) moveScript.ApplyLunge(kickLungeForce);
        
        if (combatSource != null && kickSFX != null) combatSource.PlayOneShot(kickSFX, combatVolume);
    }
    public void StopAttack() { if (weapon != null) { weapon.isAttacking = false; weapon.ClearHitList(); } }
    public void StopKick() { if (footWeapon != null) { footWeapon.isAttacking = false; footWeapon.ClearHitList(); } }
    void TryPlayAttackVoice() { if (attackVoices.Length > 0 && Random.value <= voiceChance) { AudioClip v = attackVoices[Random.Range(0, attackVoices.Length)]; if (combatSource != null) combatSource.PlayOneShot(v, combatVolume); } }
}