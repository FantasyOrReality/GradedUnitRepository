using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class BaseCard : MonoBehaviour
{
    [SerializeField] private BasicCardScriptable cardData;

    [HideInInspector] 
    public UnityEvent<BaseCard> OnCardSelected;

    private string cardName;
    private int cardStrength;
    private int cardHealth;
    private int laneIndex;
    
    private bool isCardSelected = false;
    private bool isCardReturningToPos = false;
    private bool cardPlayed = false;
    private bool canBeAttacked = true;
    private bool isPlayerCard = true;
    private bool isCardSetToAttack = false;

    private Vector3 initialCardPosition;
    private Vector3 targetHoverCardScale;
    private Vector3 targetHoverCardLocation;

    private GameObject cardEffect;

    private float hoverMinYoffset, hoverMaxYoffset;
    private float hoverYoffset = 350;
    private Coroutine cardHover;

    [Space(10)] [Header("Some Card Stuff")] 
    [SerializeField] private Image cardImage;
    [SerializeField] public Image cardTemplate;
    [SerializeField] public Image cardBack;

    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private GameObject attackUI;
    [SerializeField] private AudioSource soundWhenPlayed;
    [SerializeField] private AudioSource cardHealed;
    [SerializeField] private GameObject trail;
    [SerializeField] private ParticleSystem strengthIncrease;
    [SerializeField] private ParticleSystem healthIncrease;

    // Basic Set up of the card, like adding images and text, saving variables and binding events for when it is clicked, hovered and so on
    private void Awake()
    {
        GetComponentInChildren<Canvas>().worldCamera = Camera.main;

        cardBack.enabled = false;
        cardImage.sprite = cardData.CardImage;
        strengthText.text = cardData.cardStrength.ToString();
        healthText.text = cardData.cardHealth.ToString();
        descriptionText.text = cardData.cardDescription;
        nameText.text = IsCardTacticOrSpecial() ? cardData.cardName : SNameGenerator.GetInstance().GetRandomName();
        typeText.text = CardTypeToString(cardData.cardType);

        if (cardData.cardEffect && IsCardTactic())
        {
            cardEffect = Instantiate(cardData.cardEffect, transform);
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

    public void AISetUp()
    {
        cardImage.enabled = false;
        strengthText.enabled = false;
        healthText.enabled = false;
        descriptionText.enabled = false;
        nameText.enabled = false;
        typeText.enabled = false;
        cardTemplate.raycastTarget = false;

        isPlayerCard = false;
        if (cardEffect)
            GetCardEffect().SetIsThisPlayerCard(false);
        
        BetterButton cardButton = GetComponentInChildren<BetterButton>();
        cardButton.OnClickEvent.RemoveListener(SelectCard);
        cardButton.OnReleasedEvent.RemoveListener(PlayCard);
        cardButton.OnHoverEnter.RemoveListener(CardHoverEnter);
        cardButton.OnHoverExit.RemoveListener(CardHoverExit);

        cardBack.enabled = true;
    }

    public void AIPlayedCard()
    {
        cardImage.enabled = true;
        strengthText.enabled = true;
        healthText.enabled = true;
        descriptionText.enabled = true;
        nameText.enabled = true;
        typeText.enabled = true;

        cardBack.enabled = false;
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
            trail.SetActive(true);
            return;
        }
        
        if (cardPlayed)
            OnCardSelected?.Invoke(this);
    }

    /// <summary>
    /// This is only called once we have selected a card and we release the left mouse button and evaluates if the card has been played
    /// </summary>
    private void PlayCard()
    {
        if (!isCardSelected || cardPlayed) return;
        
        trail.SetActive(false);
        isCardSelected = false;
        if (!DuelManager.GetInstance().TryPlayCard(this, cardData.autoExecuteEffect))
        {
            // If the card can not be played, return it to it's original position
            StartCoroutine(ReturnCardToPosition());
        }
        else
        {
            // If the card has been played, disable it's raycast targeting, so that we can no longer select it
            cardPlayed = true;
            soundWhenPlayed.Play();
            if (cardData.cardEffect && !IsCardTactic())
            {
                cardEffect = Instantiate(cardData.cardEffect, transform);
                cardEffect.GetComponent<BaseCardEffect>().SetOwningCard(this);
            }
            
            cardTemplate.raycastTarget = false;
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
    public void SetUp(bool isPlayerCardIn = true)
    {
        targetHoverCardLocation = new Vector3(transform.position.x, transform.position.y + hoverYoffset, transform.position.z);
        hoverMaxYoffset = targetHoverCardLocation.y;
        hoverMinYoffset = targetHoverCardLocation.y - hoverYoffset;
        targetHoverCardScale = new Vector3(1.5f, 1.5f, 1.5f);
        
        SetIsPlayerCard(isPlayerCardIn);

        if (cardEffect)
            GetCardEffect().SetIsThisPlayerCard(isPlayerCardIn);
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

    public bool IsAtMaxHealth()
    {
        return cardHealth >= cardData.cardHealth;
    }
    
    public int GetCardHealth()
    {
        return cardHealth;
    }

    public int GetCardStrength()
    {
        return cardStrength;
    }

    public bool CanCardBeAttacked()
    {
        return canBeAttacked;
    }

    public void SetCanBeAttacked(bool newState)
    {
        canBeAttacked = newState;
    }

    public CardType GetCardType()
    {
        return cardData.cardType;
    }

    public string GetCardName()
    {
        return cardData.cardName;
    }

    public void SetIsPlayerCard(bool newValue)
    {
        isPlayerCard = newValue;
    }

    public bool GetIsPlayerCard()
    {
        return isPlayerCard;
    }

    public void SetShouldCardAttack(bool newValue)
    {
        isCardSetToAttack = newValue;
        attackUI.SetActive(isCardSetToAttack);
    }

    public bool GetIsCardSetToAttack()
    {
        return isCardSetToAttack;
    }

    public void SetLaneIndex(int index)
    {
        laneIndex = index;
    }

    public BaseCardEffect GetCardEffect()
    {
        if (cardEffect)
        {
            var ce = cardEffect.GetComponent<BaseCardEffect>();
            if (ce) return ce;
        }
        
        return null;
    }

    /// <summary>
    /// Flips the Health and Attack stats of the card
    /// </summary>
    public void FlipStats()
    {
        (cardHealth, cardStrength) = (cardStrength, cardHealth);
    }

    /// <summary>
    /// Applying health change to a card, then invoking a UnityEvent to let know other scripts that this card has had it's health changed
    /// </summary>
    /// <param name="delta">Can be a number, positive numbers increase the health, negative decrease it.</param>
    /// <param name="onlyThisTurn">Should the effect only last this turn</param>
    /// <param name="fromTempEffect">Was this function called to remove the temporary effects</param>
    /// <returns>Boolean, if health change has been applied</returns>
    // Example: If we play as Duncan, we can use this to determine if a card has been healed and if so, we can add 1 to the card healed counter
    public bool ApplyHealthChange(int delta, bool onlyThisTurn = false, bool fromTempEffect = false)
    {
        int previousHealth = cardHealth;
        cardHealth = Mathf.Clamp(cardHealth + delta, 0, 100);
        string newHealth = cardHealth.ToString();

        if (cardHealth > cardData.cardHealth)
        {
            newHealth = "<color=green>" + cardHealth + "ˆ</color>";
        }

        if (delta > 0)
        {
            healthIncrease.Play();
            cardHealed.Play();
        }

        healthText.text = newHealth;

        if (!fromTempEffect)
            DuelManager.GetInstance().OnCardHealthChanged?.Invoke(this, cardHealth - previousHealth, isPlayerCard);

        if (cardHealth == 0)
        {
            DuelManager.GetInstance().FreeLane(laneIndex, isPlayerCard);
            DuelManager.GetInstance().OnCardDestroyed?.Invoke(this, isPlayerCard);
            Destroy(gameObject, 0.25f);
        }
        return cardHealth != previousHealth;
    }

    /// <summary>
    /// Applying change in attack of the card, an example, can be Provision, which adds 1 to the card Attack
    /// </summary>
    /// <param name="delta">Can be a number, positive numbers increase attack, negative decrease it.</param>
    /// <param name="onlyThisTurn">Should the effect only last this turn</param>
    public void ApplyAttackChange(int delta, bool onlyThisTurn = false)
    {
        int previousStrength = cardStrength;
        cardStrength += delta;
        string newStrength = cardStrength.ToString();

        if (delta > 0)
        {
            strengthIncrease.Play();
            cardHealed.Play();
        }
        
        if (cardStrength > cardData.cardStrength)
        {
            newStrength = "<color=yellow>" + cardStrength + "ˆ</color>";
        }
        
        strengthText.text = newStrength;
    }
    
    /// <summary>
    /// Executes the special effect of the card
    /// </summary>
    public void ExecuteCardEffect()
    {
        if (cardEffect)
            cardEffect.GetComponent<BaseCardEffect>().SpecialEffect();
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
}
