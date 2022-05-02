using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaseCard : MonoBehaviour, CardInterface
{
    [SerializeField] private BasicCardScriptable cardData;

    [SerializeField] private bool isTacticCard = false;
    private bool isCardSelected = false;
    private bool isCardReturningToPos = false;
    
    private Vector3 initialCardPosition;
    private Vector3 targetHoverCardScale;
    private Vector3 targetHoverCardLocation;

    private Coroutine cardHover;

    private void Awake() 
    {
        GetComponentInChildren<Canvas>().worldCamera = Camera.main;

        foreach (var image in GetComponentsInChildren<Image>())
        {
            if (image.CompareTag("Image")) image.sprite = cardData.CardImage;
        }

        foreach (var text in GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (text.CompareTag("Strength")) text.text = cardData.cardStrength.ToString();
            else if (text.CompareTag("Health")) text.text = cardData.cardHealth.ToString();
            else if (text.CompareTag("Description")) text.text = cardData.cardDescription;
            else if (text.CompareTag("Name")) text.text = cardData.cardName.ToString();
            else if (text.CompareTag("Type")) text.text = CardTypeToString(cardData.cardType);
        }

        BetterButton cardButton = GetComponentInChildren<BetterButton>();
        cardButton.OnClickEvent.AddListener(SelectCard);
        cardButton.OnReleasedEvent.AddListener(PlayCard);
        cardButton.OnHoverEnter.AddListener(CardHoverEnter);
        cardButton.OnHoverExit.AddListener(CardHoverExit);

        targetHoverCardLocation = new Vector3(transform.position.x, transform.position.y + 50, transform.position.z);
        targetHoverCardScale = new Vector3(1.5f, 1.5f, 1.5f);
    }

    private string CardTypeToString(CardType type)
    {
        switch (type)
        {
            case CardType.Infantry:
                return "Infantry";
            case CardType.Tactic:
                return "Tactic";
            case CardType.Special:
                return "Special";
        }

        return null;
    }

    private void Update() 
    {
        if (isCardSelected)
        {
            Vector3 mousePos = Camera.main!.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z;
            gameObject.transform.position = mousePos;
        }
    }
    
    private void SelectCard()
    {
        if (!isCardSelected)
        {
            initialCardPosition = transform.position;
            StopCoroutine(cardHover);
            transform.position = targetHoverCardLocation;
            transform.localScale = targetHoverCardScale;
            isCardSelected = true;
        }
    }

    private void PlayCard()
    {
        if (!isCardSelected) return;
        
        isCardSelected = false;
        if (DuelManager.GetInstance().TryPlayCard(this))
        {
            // @TODO: Add stuff in here
        }
        else StartCoroutine(ReturnCardToPosition());
    }

    private void CardHoverEnter()
    {
        if (isCardReturningToPos || isCardSelected) return;
        
        if (cardHover != null)
            StopCoroutine(cardHover);
        
        cardHover = StartCoroutine(CardScaleOnHover(false));
    }

    private void CardHoverExit()
    {
        if (isCardReturningToPos || isCardSelected) return;
        
        if (cardHover != null)
            StopCoroutine(cardHover);
        
        cardHover = StartCoroutine(CardScaleOnHover(true));
    }

    public BasicCardScriptable GetCardData()
    {
        return cardData;
    }
    
    public bool GetIsCardTactic()
    {
        return isTacticCard;
    }

    IEnumerator CardScaleOnHover(bool reversed)
    {
        float delta = 0;
        if (reversed)
        {
            targetHoverCardLocation *= -1;
            targetHoverCardScale = new Vector3(1.0f, 1.0f, 1.0f);
        }

        Vector3 cInitialPosition = transform.position;
        Vector3 initialScale = transform.localScale;

        while (delta < 1)
        {
            transform.position = Vector3.Lerp(cInitialPosition, targetHoverCardLocation, delta);
            transform.localScale = Vector3.Lerp(initialScale, targetHoverCardScale, delta);
            delta += Time.deltaTime * 5f;
            yield return null;
        }

        if (reversed)
        {
            targetHoverCardLocation *= -1;
            targetHoverCardScale = new Vector3(1.5f, 1.5f, 1.5f);
        }
        
        yield return null;
    }

    IEnumerator ReturnCardToPosition()
    {
        isCardReturningToPos = true;
        bool shouldScaleDown = transform.localScale.x > 1;
        Vector3 initialPosition = transform.position;
        float delta = 0;
        while (delta < 1)
        {
            transform.position = Vector3.Lerp(initialPosition, initialCardPosition, delta);
            delta += Time.deltaTime * 5f;
            yield return null;
        }
        
        isCardReturningToPos = false;
        CardHoverExit();
        yield return null;
    }
    
    public virtual void CardEffect()
    {
        // @NOTE: This will be overridden
    }
}
