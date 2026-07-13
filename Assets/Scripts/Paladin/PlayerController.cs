using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Cursor Settings")]
    public bool hideCursorOnStart = true;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float crouchSpeed = 1.5f;
    public float gravity = -20f;
    public float moveSmoothTime = 0.1f;

    [Header("Sitting Settings")]
    public bool isSitting = false;
    private bool isStandingUp = false;
    public float standUpDuration = 1.5f;
    public float sitOrbitDistance = 3f;
    public float sitCameraHeight = 1.5f;
    public float camTransitionSpeed = 5f;
    private float yRotationSit = 0f;
    private InteractableChair currentChair;
    private Vector3 originalCamLocalPos;

    [Header("Look Settings")]
    public Transform cam;
    public float mouseSensitivity = 2f;
    [Tooltip("Global multiplier untuk input sentuh mobile")]
    public float mobileLookSensitivity = 1f; 
    public bool invertYAxis = false;
    public float upperLookLimit = -60f;
    public float lowerLookLimit = 60f;

    [Header("Jump & Crouch Settings")]
    public float jumpHeight = 2f;
    public float crouchHeight = 1f;
    public float standingHeight = 2f;

    [Header("Mobile Settings")]
    public bool isMobile = true;
    public float runThreshold = 0.8f; 

    [Header("Idle Break Settings")]
    public float idleBreakDelay = 10f;
    private float idleTimer;

    [Header("Audio Settings")]
    public AudioSource footstepSource;
    public AudioClip[] walkFootsteps;
    public AudioClip[] runFootsteps;
    public AudioClip jumpSFX;
    [Range(0, 1)] public float footstepVolume = 0.5f;

    private CharacterController controller;
    private PlayerHealth healthScript;
    private Animator anim;
    private bool isCrouching = false;
    private float xRotation = 0f;
    private Vector3 currentMoveVelocity;
    private Vector3 moveDirection;
    private float verticalVelocity;
    private float currentLungeSpeed;
    private float lungeDamping = 5f;
    private float sitTime;

    private float pendingMouseX;
    private float pendingMouseY;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();

        if (hideCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        originalCamLocalPos = cam.localPosition;
        healthScript = GetComponent<PlayerHealth>();

        // Muat Sensitivitas dari PlayerPrefs jika sudah pernah disimpan
        if (PlayerPrefs.HasKey("Sensitivity"))
        {
            mobileLookSensitivity = PlayerPrefs.GetFloat("Sensitivity");
        }

        if (PlayerPrefs.HasKey("InvertY"))
        {
            invertYAxis = PlayerPrefs.GetInt("InvertY") == 1;
        }
    }

    void Update()
    {
        if (PauseMenuController.isPaused) return;
        if (TutorialManager.isTutorialActive) return;
        if (healthScript != null && healthScript.isDead) return;
        if (anim.GetBool("isDead")) return;

        if (isMobile)
        {
            if (MobileInputManager.Instance != null)
            {

                pendingMouseX = MobileInputManager.Instance.lookInput.x * mobileLookSensitivity;
                pendingMouseY = MobileInputManager.Instance.lookInput.y * mobileLookSensitivity;
            }
        }
        else
        {
            pendingMouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            pendingMouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        }

        if (!isSitting && !isStandingUp)
        {
            transform.Rotate(Vector3.up * pendingMouseX);
            HandleMovement();
            HandleIdleBreak();
        }
        else
        {
            HandleSittingLogic();
        }
    }

    public bool IsGrounded()
    {
        return controller.isGrounded;
    }

    void LateUpdate()
    {
        if (healthScript != null && healthScript.isDead) return;
        HandleLook();
    }

    void HandleSittingLogic()
    {
        if (currentChair == null) return;

        transform.position = currentChair.sitPoint.position;
        transform.rotation = currentChair.sitPoint.rotation;

        bool wantStandUp = isMobile 
            ? (MobileInputManager.Instance != null && MobileInputManager.Instance.interactPressed) 
            : Input.GetButtonDown("Interact") || Input.GetKeyDown(KeyCode.E);

        if (isSitting && !isStandingUp && Time.time > sitTime + 0.5f && wantStandUp)
        {
            currentChair.Interact();
        }
    }

    void HandleLook()
    {
        // Rotasi Vertikal (Kamera)
        float invertMultiplier = invertYAxis ? -1f : 1f;
        xRotation -= pendingMouseY * invertMultiplier;
        xRotation = Mathf.Clamp(xRotation, upperLookLimit, lowerLookLimit);

        if (!isSitting && !isStandingUp)
        {
            // MODE NORMAL
            cam.localRotation = Quaternion.Slerp(
                cam.localRotation,
                Quaternion.Euler(xRotation, 0f, 0f),
                Time.deltaTime * camTransitionSpeed
            );
            cam.localPosition = Vector3.Lerp(
                cam.localPosition,
                originalCamLocalPos,
                Time.deltaTime * camTransitionSpeed
            );
        }
        else
        {
            if (currentChair == null) return;

            // MODE ORBIT
            yRotationSit += pendingMouseX;

            Vector3 pivot       = currentChair.sitPoint.position + Vector3.up * sitCameraHeight;
            Quaternion orbitRot = currentChair.sitPoint.rotation * Quaternion.Euler(xRotation, yRotationSit, 0f);
            Vector3 targetPos   = pivot + orbitRot * Vector3.forward * sitOrbitDistance;
            Quaternion targetLook = Quaternion.LookRotation(pivot - targetPos);

            cam.position = targetPos;
            cam.rotation = targetLook;
        }
    }

    void HandleMovement()
    {
        float x, z;
        bool isRunning;
        bool wasCrouching = isCrouching;

        if (isMobile && MobileInputManager.Instance != null)
        {
            Vector2 joy = MobileInputManager.Instance.moveInput;
            x = joy.x;
            z = joy.y;
            isRunning   = joy.magnitude > runThreshold && !isCrouching;
            isCrouching = MobileInputManager.Instance.crouchPressed;
        }
        else
        {
            x = Input.GetAxisRaw("Horizontal");
            z = Input.GetAxisRaw("Vertical");
            isCrouching = Input.GetKey(KeyCode.LeftControl);
            isRunning   = Input.GetKey(KeyCode.LeftShift) && !isCrouching;
        }

        if (wasCrouching && !isCrouching)
        {
            PlayerCombat combat = GetComponent<PlayerCombat>();
            if (combat != null) combat.ResetAllTriggers();
        }

        anim.SetBool("isCrouching", isCrouching);
        controller.height = isCrouching ? crouchHeight : standingHeight;
        controller.center = new Vector3(0, controller.height / 2f, 0);

        float targetSpeed = isCrouching ? crouchSpeed : (isRunning ? runSpeed : walkSpeed);
        Vector3 targetDir = (transform.forward * z + transform.right * x).normalized;
        moveDirection     = Vector3.SmoothDamp(moveDirection, targetDir, ref currentMoveVelocity, moveSmoothTime);

        if (controller.isGrounded)
        {
            verticalVelocity = -2f;

            bool isKicking = anim.GetCurrentAnimatorStateInfo(0).IsName("Kick");

            bool wantJump = isMobile
                ? (MobileInputManager.Instance != null && MobileInputManager.Instance.jumpPressed)
                : Input.GetButtonDown("Jump");

            if (isKicking)
            {
                anim.ResetTrigger("Jump");
                if (isMobile && MobileInputManager.Instance != null) 
                    MobileInputManager.Instance.jumpPressed = false; 

                wantJump = false;
            }

            if (wantJump && !isCrouching && !isKicking)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                anim.SetTrigger("Jump");

                if (footstepSource != null && jumpSFX != null) 
                    footstepSource.PlayOneShot(jumpSFX, footstepVolume);
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 lungeMove = transform.forward * currentLungeSpeed;
        currentLungeSpeed = Mathf.Lerp(currentLungeSpeed, 0, Time.deltaTime * lungeDamping);
        controller.Move(((moveDirection * targetSpeed) + Vector3.up * verticalVelocity + lungeMove) * Time.deltaTime);

        UpdateAnimation(x, z, isRunning);
    }

    void UpdateAnimation(float x, float z, bool isRunning)
    {
        float animX = isCrouching || isRunning ? x : x * 0.5f;
        float animZ = isCrouching || isRunning ? z : z * 0.5f;
        anim.SetFloat("isCrouchingFloat", isCrouching ? 1f : 0f, 0.1f, Time.deltaTime);
        anim.SetFloat("InputX", animX, 0.1f, Time.deltaTime);
        anim.SetFloat("InputZ", animZ, 0.1f, Time.deltaTime);
    }

    void HandleIdleBreak()
    {
        float moveInput;
        bool mouseMoving;

        if (isMobile && MobileInputManager.Instance != null)
        {
            moveInput   = MobileInputManager.Instance.moveInput.magnitude;
            mouseMoving = MobileInputManager.Instance.lookInput.magnitude > 0.01f;
        }
        else
        {
            moveInput   = Mathf.Abs(Input.GetAxisRaw("Horizontal")) + Mathf.Abs(Input.GetAxisRaw("Vertical"));
            mouseMoving = Mathf.Abs(Input.GetAxis("Mouse X")) > 0.1f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.1f;
        }

        if (moveInput > 0.1f || mouseMoving || isCrouching)
        {
            idleTimer = 0;
            anim.ResetTrigger("IdleBreak");
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleBreakDelay)
            {
                idleTimer = 0;
                anim.SetInteger("IdleIndex", Random.Range(0, 3));
                anim.SetTrigger("IdleBreak");
            }
        }
    }

    public void SetSitting(bool state, Transform sitPoint, InteractableChair chair)
    {
        if (state)
        {
            isSitting    = true;
            currentChair = chair;
            sitTime      = Time.time;
            currentLungeSpeed = 0;
            verticalVelocity  = 0;
            moveDirection     = Vector3.zero;
            controller.enabled = false;
            anim.SetBool("isSitting", true);

            transform.position = sitPoint.position;
            transform.rotation = sitPoint.rotation;

            Vector3 pivot     = sitPoint.position + Vector3.up * sitCameraHeight;
            Vector3 dirToCam  = cam.position - pivot;
            float horizontalDist = new Vector2(dirToCam.x, dirToCam.z).magnitude;
            xRotation = -Mathf.Atan2(dirToCam.y, horizontalDist) * Mathf.Rad2Deg;
            xRotation = Mathf.Clamp(xRotation, upperLookLimit, lowerLookLimit);

            Vector3 dirFlat = new Vector3(dirToCam.x, 0f, dirToCam.z);
            if (dirFlat.sqrMagnitude < 0.001f) dirFlat = sitPoint.forward;
            Vector3 localDir = Quaternion.Inverse(sitPoint.rotation) * dirFlat.normalized;
            yRotationSit = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

            Quaternion orbitRot = sitPoint.rotation * Quaternion.Euler(xRotation, yRotationSit, 0f);
            Vector3 snapPos     = pivot + orbitRot * Vector3.forward * sitOrbitDistance;

            cam.position = snapPos;
            cam.rotation = Quaternion.LookRotation(pivot - snapPos);

            pendingMouseX = 0f;
            pendingMouseY = 0f;
        }
        else
        {
            StartCoroutine(StandUpRoutine());
        }
    }

    IEnumerator StandUpRoutine()
    {
        isSitting    = false;
        isStandingUp = true;
        anim.SetBool("isSitting", false);
        yield return new WaitForSeconds(standUpDuration);

        if (currentChair != null && currentChair.exitPoint != null)
        {
            transform.position = currentChair.exitPoint.position;
            transform.rotation = currentChair.exitPoint.rotation;
        }

        isStandingUp       = false;
        controller.enabled = true;
        currentChair       = null;
    }

    public void ApplyLunge(float force) { currentLungeSpeed = force; }

    public void PlayFootstepSFX()
    {
        if (!controller.isGrounded || footstepSource == null) return;
        bool isRunning = isMobile ? (MobileInputManager.Instance != null && MobileInputManager.Instance.moveInput.magnitude > runThreshold) : Input.GetKey(KeyCode.LeftShift) && !isCrouching;
        AudioClip[] currentClips = isRunning ? runFootsteps : walkFootsteps;
        if (currentClips.Length > 0) { footstepSource.PlayOneShot(currentClips[Random.Range(0, currentClips.Length)], footstepVolume); }
    }
}