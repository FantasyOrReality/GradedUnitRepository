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

    // Basic Set up of the card, like adding images and text, saving variables and binding events for when it is clicked, hovered and so on
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
    
    /// <summary>
    /// A simple converter that we use to be able to get the CardType as text to display on the card UI in game
    /// </summary>
    /// <param name="type">Type of CardType, the CardType to be converted to a string</param>
    /// <returns>CardType converted to String</returns>
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
        // When the card is selected, update it's position based on the mouse cursor
        if (isCardSelected)
        {
            Vector3 mousePos = Camera.main!.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z;
            gameObject.transform.position = mousePos;
        }
    }
    
    /// <summary>
    /// When we click the LMB, we select the card and attach it to the mouse cursor, then changing it's position in Update
    /// </summary>
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

    /// <summary>
    /// This is only called once we have selected a card and we release the left mouse button and evaluates if the card has been played
    /// </summary>
    private void PlayCard()
    {
        if (!isCardSelected || cardPlayed) return;
        
        
        isCardSelected = false;
        if (!DuelManager.GetInstance().TryPlayCard(this))
        {
            // If the card can not be played, return it to it's original position
            StartCoroutine(ReturnCardToPosition());
        }
        else
        {
            // If the card has been played, disable it's raycast targeting, so that we can no longer select it
            cardPlayed = true;
            foreach (var image in GetComponentsInChildren<Image>())
            {
                if (image.CompareTag("Template")) image.raycastTarget = false;
            }
        }
    }

    /// <summary>
    /// On Hover effect to enlarge the card, so that it is easier to read
    /// </summary>
    private void CardHoverEnter()
    {
        if (isCardReturningToPos || isCardSelected || cardPlayed) return;
        
        initialCardPosition = transform.position;
        
        if (cardHover != null)
            StopCoroutine(cardHover);
        
        cardHover = StartCoroutine(CardScaleOnHover(false));
    }

    /// <summary>
    /// After hovering on the card, exiting hover, scales down the card and returns it to it's original state
    /// </summary>
    private void CardHoverExit()
    {
        if (isCardReturningToPos || isCardSelected || cardPlayed) return;
        
        if (cardHover != null)
            StopCoroutine(cardHover);
        
        cardHover = StartCoroutine(CardScaleOnHover(true));
    }
    
    /// <summary>
    /// SetUp called from the DuelManager to make sure that we have correct positions for the card hover effect
    /// </summary>
    public void SetUp()
    {
        targetHoverCardLocation = new Vector3(transform.position.x, transform.position.y + hoverYoffset, transform.position.z);
        hoverMaxYoffset = targetHoverCardLocation.y;
        hoverMinYoffset = targetHoverCardLocation.y - hoverYoffset;
        targetHoverCardScale = new Vector3(1.5f, 1.5f, 1.5f);
    }
    
    // Basic Getters
    public bool IsCardTactic()
    {
        return cardData.cardType == CardType.Tactic;;
    }
    
    public bool IsCardSpecial()
    {
        return cardData.cardType == CardType.Special;;
    }

    private bool IsCardTacticOrSpecial()
    {
        return IsCardTactic() || IsCardSpecial();
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
    
    /// <summary>
    /// Applying health change to a card, then invoking a UnityEvent to let know other scripts that this card has had it's health changed
    /// </summary>
    /// <param name="delta">Can be a number, positive numbers increase the health, negative decrease it.</param>
    /// <returns></returns>
    // Example: If we play as Duncan, we can use this to determine if a card has been healed and if so, we can add 1 to the card healed counter
    public bool ApplyHealthChange(int delta)
    {
        cardHealth += delta;

        int actualDelta = cardHealth - cardData.cardHealth;
        
        DuelManager.GetInstance().OnCardHealthChanged?.Invoke(this, actualDelta);
        return cardHealth > 0;
    }

    /// <summary>
    /// Applying change in attack of the card, an example, can be Provision, which adds 1 to the card Attack
    /// </summary>
    /// <param name="delta">Can be a number, positive numbers increase attack, negative decrease it.</param>
    public void ApplyAttackChange(int delta)
    {
        cardStrength += delta;
    }
    
    /// <summary>
    /// This function deals with the hover effect, enlarging the card or returning it back to it's original state
    /// </summary>
    /// <param name="reversed">Can be true of false, TRUE when the card is going to back to it's original state and FALSE when it is being enlarged</param>
    /// <returns>null</returns>
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

    /// <summary>
    /// Returns the card to it's original position
    /// </summary>
    /// <returns>null</returns>
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
    
    /// <summary>
    /// Most likely will need to be removed
    /// </summary>
    public virtual void CardEffect()
    {
        //@ TODO: Figure out a way to implement different card effects
        
        Destroy(gameObject);
    }
}
