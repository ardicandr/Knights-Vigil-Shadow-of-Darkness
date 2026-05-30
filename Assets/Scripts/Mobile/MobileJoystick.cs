using UnityEngine;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform background;
    public RectTransform handle;
    public float handleRange = 100f;

    private Canvas canvas;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData e)
    {
        OnDrag(e);
    }

    public void OnDrag(PointerEventData e)
    {

        Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : e.pressEventCamera;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, e.position, cam, out localPoint))
        {
 
            Vector2 clamped = Vector2.ClampMagnitude(localPoint, handleRange);
            
            handle.anchoredPosition = clamped;

            if (MobileInputManager.Instance != null)
                MobileInputManager.Instance.moveInput = clamped / handleRange;
        }
    }

    public void OnPointerUp(PointerEventData e)
    {
        handle.anchoredPosition = Vector2.zero;
        if (MobileInputManager.Instance != null)
            MobileInputManager.Instance.moveInput = Vector2.zero;
    }
}