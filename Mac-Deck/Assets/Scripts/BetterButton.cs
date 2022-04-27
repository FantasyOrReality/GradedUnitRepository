using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class BetterButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public UnityEvent OnClickEvent;
    public UnityEvent OnReleasedEvent;
    public UnityEvent OnHoldEvent;

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

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!button.interactable) return;

        if (buttonPressed)
        {
            OnReleasedEvent?.Invoke();
            buttonPressed = false;
        }
    }
}
