using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = System.Random;

/// <summary>
/// An enum used to determine the phase of the duel and evaluate the actions
/// </summary>
[Serializable]
public enum DuelPhase
{
    StartPhase,
    DrawingPhase,
    MainPhase,
    CombatPhase,
    AIPhase,
    None
}

/// <summary>
/// Class used to define the DuelLanes, later more will be added here
/// </summary>
[Serializable]
public class DuelLanes
{
    public Transform upperLimit;
    public Transform lowerLimit;
    public Transform snapPoint;

    public bool occupied;

    public BaseCard cardInLane;

    public void SetCardInLane(BaseCard inCard)
    {
        cardInLane = inCard;
        occupied = true;
    }
}

/// <summary>
/// Class used to find the CardTargets for the player hand
/// </summary>
[Serializable]
public class CardTarget
{
    public Transform target;
    public bool occupied;

    public void SetIsOccupied(bool inBool) => occupied = inBool;
}

public class DuelManager : MonoBehaviour
{
    static DuelManager instance;
    
    [Header("Basics")]
    public SNameGenerator nameGen;

    [HideInInspector]
    public UnityEvent<BaseCard, bool> OnCardSummoned;

    [HideInInspector]
    public UnityEvent<BaseCard, int, bool> OnCardHealthChanged;

    [HideInInspector] 
    public UnityEvent<BaseCard, bool> OnCardDestroyed;

    [HideInInspector] 
    public UnityEvent<BaseEarl, int, bool> OnEarlHealthChanged;

    [HideInInspector] 
    public UnityEvent<bool> OnTurnEnded;
    
    [Space(10)]
    [Header("Player Targets")]
    [SerializeField] private List<DuelLanes> playerDuelLanes = new List<DuelLanes>(4);
    [SerializeField] private List<CardTarget> cardTargets = new List<CardTarget>(6);
    private List<BaseCard> playerHand = new List<BaseCard>(6);
    
    
    [Space(10)]
    [Header("AI Targets")]
    [SerializeField] private List<DuelLanes> aiDuelLanes = new List<DuelLanes>(4);
    [SerializeField] private List<CardTarget> aiCardTargets = new List<CardTarget>(6);
    private List<BaseCard> aiHand = new List<BaseCard>(6);


    [Space(10)]
    [Header("Transforms")]
    [SerializeField] private Transform tacticUpperLimit;
    [SerializeField] private Transform tacticLowerLimit;
    [SerializeField] private Transform cardSpawn;
    [SerializeField] private Transform earlTarget;
    [SerializeField] private Transform aiEarlTarget;
    [SerializeField] private Transform aiCardSpawn;

    
    [Space(10)]
    [Header("User Interface")]
    [SerializeField] private Text remainingCardsText;
    [SerializeField] private Text remainingCardsTextAI;
    
    
    [Space(10)]
    [Header("Earls")]
    [SerializeField] private BaseEarl selectedPlayerEarl;
    [SerializeField] private BaseEarl AIEarl;

    private List<BaseCard> cardsInDeck = new List<BaseCard>(25);
    private List<BaseCard> aiCardsInDeck = new List<BaseCard>(25);
    
    private bool isDrawing = false;
    private DuelPhase duelPhase = DuelPhase.StartPhase;
    private DuelPhase previousDuelPhase = DuelPhase.None;

    public static DuelManager GetInstance()
    {
        return instance;
    }
    
    // This function sets up the board and the player deck, it also makes sure that the DuelManager is a SINGLETON
    private void Awake()
    {
        // Singleton PART Start
        if (instance != null)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // Singleton PART End
        
        Instantiate(nameGen);
        
        selectedPlayerEarl = Instantiate(selectedPlayerEarl, earlTarget.position, earlTarget.rotation);

        // Set up the deck to be used based on the Earl
        foreach (var card in selectedPlayerEarl.GetEarlDeck())
        {
            cardsInDeck.Add(card);
        }

        cardsInDeck = ShuffleDeck(cardsInDeck);

        // Set up the User Interface
        remainingCardsText.text = cardsInDeck.Count.ToString();
    }

    /// <summary>
    /// Gets the first unoccupied target transform
    /// </summary>
    /// <param name="isPlayerTargets">Is the player calling this function</param>
    /// <returns>Vector3 type, the position the card should lerp to</returns>
    private Vector3 GetFirstUnoccupiedTargetTransform(bool isPlayerTargets = true)
    {
        List<CardTarget> targets = isPlayerTargets ? cardTargets : aiCardTargets;

        for (int i = 0; i < targets.Count; i++)
        {
            if (!targets[i].occupied)
            {
                targets[i].SetIsOccupied(true);
                return targets[i].target.transform.position;
            }
        }

        Vector3 targetToReturn;

        if (isPlayerTargets)
        {
            cardTargets[0].SetIsOccupied(true);
            targetToReturn = cardTargets[0].target.transform.position;
        }
        else
        {
            aiCardTargets[0].SetIsOccupied(true);
            targetToReturn = aiCardTargets[0].target.transform.position;
        }
        return targetToReturn;
    }

    /// <summary>
    /// Fixes the position of the cards after you play one
    /// </summary>
    /// <param name="cardPlayed">BaseCard type,card played from the hand</param>
    private void SortOutHand(BaseCard cardPlayed)
    {
        playerHand.Remove(cardPlayed);

        for (int i = 0; i < cardTargets.Count; i++)
        {
            if (i < playerHand.Count && playerHand[i] != null)
            {
                playerHand[i].transform.position = cardTargets[i].target.position;
                playerHand[i].SetUp();
            }
            else
            {
                cardTargets[i].SetIsOccupied(false);
            }
        }
    }

    /// <summary>
    /// Tries to play a card at the position it was released
    /// </summary>
    /// <param name="cardToPlay">BaseCard type, card to be played</param>
    /// <param name="executeEffect">Should the card effect be executed</param>
    /// <returns>Boolean, true when the card can be played and false when it can not be</returns>
    public bool TryPlayCard(BaseCard cardToPlay, bool executeEffect = false)
    {
        if (duelPhase != DuelPhase.MainPhase || duelPhase == DuelPhase.StartPhase) return false;
        
        // Check if the card is a tactic and if so use the tactic card limits
        if (cardToPlay.IsCardTactic())
        {
            if (cardToPlay.transform.position.x > tacticUpperLimit.position.x &&
                cardToPlay.transform.position.y < tacticUpperLimit.position.y &&
                cardToPlay.transform.position.x < tacticLowerLimit.position.x &&
                cardToPlay.transform.position.y > tacticLowerLimit.position.y)
            {
                cardToPlay.GetComponentInChildren<BaseCardEffect>().SpecialEffect();
                SortOutHand(cardToPlay);
                return true;
            }
        }
        
        // If the card is not tactic, we use the player duel lanes limits
        else
        {
            for (int i = 0; i < playerDuelLanes.Count; i++)
            {
                if (cardToPlay.transform.position.x > playerDuelLanes[i].upperLimit.position.x &&
                    cardToPlay.transform.position.y < playerDuelLanes[i].upperLimit.position.y &&
                    cardToPlay.transform.position.x < playerDuelLanes[i].lowerLimit.position.x &&
                    cardToPlay.transform.position.y > playerDuelLanes[i].lowerLimit.position.y)
                {
                    // If player duel lane is occupied we return
                    if (playerDuelLanes[i].occupied) return false;

                    // Else we sort out the hand and play the card
                    SortOutHand(cardToPlay);
                    StartCoroutine(LerpCardToPlace(cardToPlay, i, executeEffect));
                    return true;
                }
            
            }
        }
        
        // Return FALSE, letting the card know that it can not be played
        return false;
    }
    
    /// <summary>
    ///  Draws a card from the Deck and removes it from the List
    /// </summary>
    /// <param name="quantity">Integer value, how many cards to draw</param>
    public void DrawCardFromDeck(int quantity = 1)
    {
        if (isDrawing) return;

        ChangeDuelPhase(DuelPhase.DrawingPhase);
        StartCoroutine(DrawAndLerpCardFromDeckToLocation(GetFirstUnoccupiedTargetTransform(), quantity));
    }

    public List<BaseCard> GetAllFriendlyCardsOnField()
    {
        List<BaseCard> cardsOnField = new List<BaseCard>();
        
        for (int i = 0; i < playerDuelLanes.Count; i++)
        {
            if (playerDuelLanes[i].cardInLane != null)
                cardsOnField.Add(playerDuelLanes[i].cardInLane);
        }
        
        return cardsOnField;
    }
    
    public List<BaseCard> GetAllFriendlyCardsOnFieldOfType(CardType cardType)
    {
        List<BaseCard> cardsOnField = new List<BaseCard>();

        for (int i = 0; i < playerDuelLanes.Count; i++)
        {
            if (playerDuelLanes[i].cardInLane.GetCardType() == cardType)
                if (playerDuelLanes[i].cardInLane != null)
                    cardsOnField.Add(playerDuelLanes[i].cardInLane);
        }
        
        return cardsOnField;
    }
    
    public BaseEarl GetPlayerEarl()
    {
        return selectedPlayerEarl;
    }

    public void SetPlayerEarl(BaseEarl earlToSet)
    {
        selectedPlayerEarl = earlToSet;
    }

    public BaseEarl GetAIEarl()
    {
        return AIEarl;
    }

    public void SetAIEarl(BaseEarl earlToSet)
    {
        AIEarl = earlToSet;
    }

    public DuelPhase GetDuelPhase()
    {
        return duelPhase;
    }
    
    /// <summary>
    /// Changes the duel phase to a new one and stores the previous one in a variable
    /// </summary>
    /// <param name="newPhase"></param>
    public void ChangeDuelPhase(DuelPhase newPhase)
    {
        previousDuelPhase = duelPhase;
        duelPhase = newPhase;
    }

    /// <summary>
    /// Switches to combat state
    /// </summary>
    public void SwitchToCombatState()
    {
        if (duelPhase == DuelPhase.AIPhase || duelPhase == DuelPhase.StartPhase) return;

        if (duelPhase == DuelPhase.CombatPhase) 
        {
            ChangeDuelPhase(DuelPhase.MainPhase);
            foreach (var duelLane in playerDuelLanes)
            {
                if (duelLane.occupied)
                {
                    duelLane.cardInLane.cardTemplate.raycastTarget = false;
                    if (duelLane.cardInLane.GetCardName() != "The Priest")
                        duelLane.cardInLane.OnCardSelected.RemoveListener(SetCardToAttack);
                }
            }
            return;
        }
        
        ChangeDuelPhase(DuelPhase.CombatPhase);
        foreach (var duelLane in playerDuelLanes)
        {
            if (duelLane.occupied)
            {
                duelLane.cardInLane.cardTemplate.raycastTarget = true;
                if (duelLane.cardInLane.GetCardName() != "The Priest")
                    duelLane.cardInLane.OnCardSelected.AddListener(SetCardToAttack);
            }
        }
    }

    /// <summary>
    /// Ends the current turn
    /// </summary>
    /// <param name="aiEndingTurn">If the AI is the one ending the turn</param>
    public void EndTurn(bool aiEndingTurn = false)
    {
        for (int i = 0; i < playerDuelLanes.Count; i++)
        {
            if (playerDuelLanes[i].occupied)
            {
                int damage = -playerDuelLanes[i].cardInLane.GetCardStrength();
                if (aiDuelLanes[i].occupied)
                {
                    aiDuelLanes[i].cardInLane.ApplyHealthChange(damage);
                }
                else AIEarl.ApplyHealthChange(damage);
            }

            if (aiDuelLanes[i].occupied)
            {
                int damage = -aiDuelLanes[i].cardInLane.GetCardStrength();
                if (playerDuelLanes[i].occupied)
                {
                    playerDuelLanes[i].cardInLane.ApplyHealthChange(damage);
                }
                else selectedPlayerEarl.ApplyHealthChange(damage);
            }
        }
        
        if (duelPhase != DuelPhase.AIPhase)
        {
            duelPhase = DuelPhase.AIPhase;
        }
        else if (aiEndingTurn)
        {
            DrawCardFromDeck();
        }
    }

    /// <summary>
    /// Shuffles the deck randomly
    /// </summary>
    /// <param name="deckToShuffle">List of BaseCard type, which is the deck to be shuffled</param>
    /// <returns>Lis of BaseCard type, a shuffled deck</returns>
    private List<BaseCard> ShuffleDeck(List<BaseCard> deckToShuffle)
    {
        Random rnd = new Random();
        return deckToShuffle.OrderBy(a => rnd.Next()).ToList();
    }

    private void SetCardToAttack(BaseCard card)
    {
        card.SetShouldCardAttack(!card.GetIsCardSetToAttack());
    }

    /// <summary>
    /// Draw and Lerp the card from the Deck to the required hand location
    /// </summary>
    /// <param name="target">Vector3 type, the target to where the card should lerp to, usually a hand position</param>
    /// <param name="quantity">Number of cards to Draw</param>
    /// <returns>null</returns>
    IEnumerator DrawAndLerpCardFromDeckToLocation(Vector3 target, int quantity = 1)
    {
        if (cardsInDeck.Count == 0) yield break;
        if (playerHand.Count >= 6) yield break;

        isDrawing = true;
        BaseCard card = Instantiate(cardsInDeck[0], cardSpawn.position, cardSpawn.rotation);
        Vector3 initialPos = card.transform.position;
        float delta = 0;

        while (delta < 1)
        {
            card.transform.position = Vector3.Lerp(initialPos, target, delta);
            delta += Time.deltaTime * 10f;
            yield return null;
        }
    
        card.transform.position = target;
        card.SetUp();
        playerHand.Add(card);
        cardsInDeck.RemoveAt(0);
        
        if (cardsInDeck.Count != 0)
            remainingCardsText.text = cardsInDeck.Count.ToString();
        
        if (quantity > 1)
            StartCoroutine(DrawAndLerpCardFromDeckToLocation(GetFirstUnoccupiedTargetTransform(), quantity - 1));

        isDrawing = false;
        ChangeDuelPhase(DuelPhase.MainPhase);
        yield return null;
    }

    /// <summary>
    /// Lerp the card into the duel lane position, where it was played at
    /// </summary>
    /// <param name="card">BaseCard type, the card that needs to be moved</param>
    /// <param name="laneIndex">Integer, the index of lane the card should move to</param>
    /// <param name="executeEffect">Should the card effect be executed</param>
    /// <returns>null</returns>
    IEnumerator LerpCardToPlace(BaseCard card, int laneIndex, bool executeEffect = false)
    {
        float delta = 0;
        Vector3 initialCardPos = card.transform.position;
        Vector3 initialScale = card.transform.localScale;

        while (delta < 1)
        {
            card.transform.position = Vector3.Lerp(initialCardPos, playerDuelLanes[laneIndex].snapPoint.transform.position, delta);
            card.transform.localScale = Vector3.Lerp(initialScale, new Vector3(1.0f, 1.0f, 1.0f), delta);
            delta += Time.deltaTime * 10f;
            yield return null;
        }

        card.transform.position = playerDuelLanes[laneIndex].snapPoint.transform.position;
        card.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        
        playerDuelLanes[laneIndex].SetCardInLane(card);
        if (executeEffect) card.ExecuteCardEffect();
        OnCardSummoned?.Invoke(card, card.GetIsPlayerCard());
        yield return null;
    }
}
