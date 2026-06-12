using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactRange = 3f;       
    public LayerMask interactLayer;        
    public GameObject interactUI;

    void Update()
    {
        CheckForInteractable();
    }

    void CheckForInteractable()
    {
        PlayerController pc = GetComponentInParent<PlayerController>();
 
        bool isBusyInteracting = false;
        
        if (pc != null && pc.isSitting) isBusyInteracting = true;

        InteractableInfoBoard currentBoard = FindObjectOfType<InteractableInfoBoard>();
        if (currentBoard != null && currentBoard.isOpen) isBusyInteracting = true;

        if (isBusyInteracting)
        {
            if (interactUI != null) interactUI.SetActive(true);
            
            if (Input.GetKeyDown(KeyCode.E) || (MobileInputManager.Instance != null && MobileInputManager.Instance.interactPressed))
            {
            }
            return; 
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, interactRange, interactLayer))
        {
            InteractableChest chest = hit.collider.GetComponentInParent<InteractableChest>();
            if (chest != null && chest.IsOpened)
            {
                if (interactUI != null) interactUI.SetActive(false);
                return;
            }

            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                if (interactUI != null) interactUI.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E) || (MobileInputManager.Instance != null && MobileInputManager.Instance.interactPressed))
                {
                    interactable.Interact();
                }
                return;
            }
        }

        if (interactUI != null) interactUI.SetActive(false);
    }
}