using UnityEngine;

public class InteractableChair : MonoBehaviour, IInteractable
{
    public Transform sitPoint;
    public Transform exitPoint;
    public bool isOccupied = false;

    public void Interact()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null) return;

        if (!player.isSitting)
        {
            isOccupied = true;
            player.SetSitting(true, sitPoint, this);
        }
        else
        {
            isOccupied = false;
            player.SetSitting(false, null, null);
        }
    }
}