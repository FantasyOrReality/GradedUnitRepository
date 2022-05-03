using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaseCard : MonoBehaviour, CardInterface
{
    [SerializeField] private BasicCardScriptable cardData;

    private string cardName;
    private int cardStrength;
    private int cardHealth;
    
    private bool isCardSelected = false;
    private bool isCardReturningToPos = false;
    private bool cardPlayed = false;
    
    private Vector3 initialCardPosition;
    private Vector3 targetHoverCardScale;
    private Vector3 targetHoverCardLocation;

    private float hoverMinYoffset, hoverMaxYoffset;
    private float hoverYoffset = 350;
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
            else if (text.CompareTag("Name")) text.text = IsCardTacticOrSpecial() ? cardData.cardName : SNameGenerator.GetInstance().GetRandomName();
            else if (text.CompareTag("Type")) text.text = CardTypeToString(cardData.cardType);
        }

        cardName = cardData.cardName;
        cardStrength = cardData.cardStrength;
        cardHealth = cardData.cardHealth;

        BetterButton cardButton = GetComponentInChildren<BetterButton>();
        cardButton.OnClickEvent.AddListener(SelectCard);
        cardButton.OnReleasedEvent.AddListener(PlayCard);
        cardButton.OnHoverEnter.AddListener(CardHoverEnter);
        cardButton.OnHoverExit.AddListener(CardHoverExit);
    }
    
    private string CardTypeToString(CardType type)
    {
        switch (type)
        {
            case CardType.LSoldier:
                return "L. Soldier";
            case CardType.HSoldier:
                return "H. Soldier";
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
        if (!isCardSelected && !cardPlayed)
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
        if (!isCardSelected || cardPlayed) return;
        
        isCardSelected = false;
        if (!DuelManager.GetInstance().TryPlayCard(this))
        {
            StartCoroutine(ReturnCardToPosition());
        }
        else
        {
            cardPlayed = true;
            foreach (var image in GetComponentsInChildren<Image>())
            {
                if (image.CompareTag("Template")) image.raycastTarget = false;
            }
        }
    }

    private void CardHoverEnter()
    {
        if (isCardReturningToPos || isCardSelected || cardPlayed) return;
        
        initialCardPosition = transform.position;
        
        if (cardHover != null)
            StopCoroutine(cardHover);
        
        cardHover = StartCoroutine(CardScaleOnHover(false));
    }

    private void CardHoverExit()
    {
        if (isCardReturningToPos || isCardSelected || cardPlayed) return;
        
        if (cardHover != null)
            StopCoroutine(cardHover);
        
        cardHover = StartCoroutine(CardScaleOnHover(true));
    }
    
    public void SetUp()
    {
        targetHoverCardLocation = new Vector3(transform.position.x, transform.position.y + hoverYoffset, transform.position.z);
        hoverMaxYoffset = targetHoverCardLocation.y;
        hoverMinYoffset = targetHoverCardLocation.y - hoverYoffset;
        targetHoverCardScale = new Vector3(1.5f, 1.5f, 1.5f);
    }
    
    public bool IsCardTactic()
    {
        return cardData.cardType == CardType.Tactic;;
    }
    
    public bool IsCardSpecial()
    {
        return cardData.cardType == CardType.Special;;
    }

    public int GetCardHealth()
    {
        return cardHealth;
    }

    public int GetCardStrength()
    {
        return cardStrength;
    }

    public CardType GetCardType()
    {
        return cardData.cardType;
    }

    public bool ApplyHealthChange(int delta)
    {
        cardHealth += delta;

        return cardHealth > 0;
    }

    public void ApplyAttackChange(int delta)
    {
        cardStrength += delta;
    }
    
    private bool IsCardTacticOrSpecial()
    {
        return IsCardTactic() || IsCardSpecial();
    }

    IEnumerator CardScaleOnHover(bool reversed)
    {

        Canvas currCanvas = GetComponentInChildren<Canvas>();
        float delta = 0;
        if (reversed)
        {
            targetHoverCardLocation.y = Mathf.Clamp(targetHoverCardLocation.y - hoverYoffset, hoverMinYoffset, hoverMaxYoffset);
            targetHoverCardScale = new Vector3(1.0f, 1.0f, 1.0f);
        }

        currCanvas.sortingOrder = 1;
        Vector3 cInitialPosition = transform.position;
        Vector3 initialScale = transform.localScale;

        while (delta < 1)
        {
            transform.position = Vector3.Lerp(cInitialPosition, targetHoverCardLocation, delta);
            transform.localScale = Vector3.Lerp(initialScale, targetHoverCardScale, delta);
            delta += Time.deltaTime * 5f;
            yield return null;
        }
        
        transform.position = targetHoverCardLocation;
        transform.localScale = targetHoverCardScale;

        if (reversed)
        {
            currCanvas.sortingOrder = 0;
            targetHoverCardLocation.y = Mathf.Clamp(targetHoverCardLocation.y + hoverYoffset, hoverMinYoffset, hoverMaxYoffset);
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
        Destroy(gameObject);
    }
}
