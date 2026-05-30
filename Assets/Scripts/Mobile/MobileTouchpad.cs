using UnityEngine;
using UnityEngine.EventSystems;

public class MobileTouchpad : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Touch Settings")]
    [Range(0.01f, 2.0f)]
    public float sensitivity = 0.5f; // Pengaturan sensitivitas di Inspector

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        if (MobileInputManager.Instance != null)
        {
            MobileInputManager.Instance.lookInput = eventData.delta * sensitivity;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (MobileInputManager.Instance != null)
        {
            MobileInputManager.Instance.lookInput = Vector2.zero;
        }
    }
}