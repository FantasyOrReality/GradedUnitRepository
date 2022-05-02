using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class BetterButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
{
    public UnityEvent OnClickEvent;
    public UnityEvent OnReleasedEvent;
    public UnityEvent OnHoldEvent;
    public UnityEvent OnHoverEnter;
    public UnityEvent OnHoverExit;

    private Button button;

    private bool buttonPressed = false;
    
    // Start is called before the first frame update
    void Awake()
    {
        button = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        if (buttonPressed)
            OnHoldEvent?.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;

        OnClickEvent?.Invoke();
        buttonPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable) return;

        if (buttonPressed)
        {
            OnReleasedEvent?.Invoke();
            buttonPressed = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;

        if (!buttonPressed)
        {
            OnHoverEnter?.Invoke();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!button.interactable) return;

        if (!buttonPressed)
        {
            OnHoverExit?.Invoke();
        }
    }
}
