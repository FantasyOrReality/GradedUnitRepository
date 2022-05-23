using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;
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

    [Space(10)] 
    [Header("Duel Variables")] 
    [SerializeField] private int maxUnitsSummonedPerTurn = 1;
    [SerializeField] private int maxTacticCardsPlayedPerTurn = 1;
    [SerializeField] private int aiSmartMoveChance = 70;

    [SerializeField] private GameObject startButton;

    private List<BaseCard> cardsInDeck = new List<BaseCard>(25);
    private List<BaseCard> aiCardsInDeck = new List<BaseCard>(25);
    
    private bool isDrawing = false;
    private DuelPhase duelPhase = DuelPhase.StartPhase;
    private DuelPhase previousDuelPhase = DuelPhase.None;

    private int numUnitsSummoned = 0;
    private int numTacticCardsPlayed = 0;

    private Random rnd;

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
        rnd = new Random();
        
        selectedPlayerEarl = Instantiate(selectedPlayerEarl, earlTarget.position, earlTarget.rotation);
        AIEarl = Instantiate(AIEarl, aiEarlTarget.position, aiEarlTarget.rotation);
        
        AIEarl.AISetUp();
        
        // Set up the deck to be used based on the Earl
        foreach (var card in selectedPlayerEarl.GetEarlDeck())
        {
            cardsInDeck.Add(card);
        }

        foreach (var card in AIEarl.GetEarlDeck())
        {
            aiCardsInDeck.Add(card);
        }

        cardsInDeck = ShuffleDeck(cardsInDeck);
        aiCardsInDeck = ShuffleDeck(aiCardsInDeck);

        // Set up the User Interface
        remainingCardsText.text = cardsInDeck.Count.ToString();
        remainingCardsTextAI.text = aiCardsInDeck.Count.ToString();
        
        OnEarlHealthChanged.AddListener(OnEarlHealthChangedInternal);
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

    public void FreeLane(int laneIndex, bool playerLane)
    {
        if (playerLane)
        {
            playerDuelLanes[laneIndex].occupied = false;
            playerDuelLanes[laneIndex].cardInLane = null;
        }
        else
        {
            aiDuelLanes[laneIndex].occupied = false;
            aiDuelLanes[laneIndex].cardInLane = null;
        }
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

    private void AISortOutHand(BaseCard cardPlayed)
    {
        aiHand.Remove(cardPlayed);

        for (int i = 0; i < aiCardTargets.Count; i++)
        {
            if (i < aiHand.Count && aiHand[i] != null)
            {
                aiHand[i].transform.position = aiCardTargets[i].target.position;
                aiHand[i].SetUp();
            }
            else
            {
                aiCardTargets[i].SetIsOccupied(false);
            }
        }
    }

    public void StartDuel()
    {
        startButton.SetActive(false);
        DrawCardFromDeck(5);
        StartCoroutine(AIDrawCard(5, false));
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
            if (numTacticCardsPlayed >= maxTacticCardsPlayedPerTurn) return false;
            if (cardToPlay.transform.position.x > tacticUpperLimit.position.x &&
                cardToPlay.transform.position.y < tacticUpperLimit.position.y &&
                cardToPlay.transform.position.x < tacticLowerLimit.position.x &&
                cardToPlay.transform.position.y > tacticLowerLimit.position.y)
            {
                cardToPlay.GetCardEffect().SpecialEffect();
                SortOutHand(cardToPlay);
                numTacticCardsPlayed++;
                return true;
            }
        }
        
        // If the card is not tactic, we use the player duel lanes limits
        else
        {
            if (numUnitsSummoned >= maxUnitsSummonedPerTurn) return false;
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
                    numUnitsSummoned++;
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
    
    public List<BaseCard> GetAllAICardsOnField()
    {
        List<BaseCard> cardsOnField = new List<BaseCard>();
        
        for (int i = 0; i < aiDuelLanes.Count; i++)
        {
            if (aiDuelLanes[i].cardInLane != null)
                cardsOnField.Add(aiDuelLanes[i].cardInLane);
        }
        
        return cardsOnField;
    }
    
    public List<BaseCard> GetAllAICardsOnFieldOfType(CardType cardType)
    {
        List<BaseCard> cardsOnField = new List<BaseCard>();

        for (int i = 0; i < aiDuelLanes.Count; i++)
        {
            if (aiDuelLanes[i].cardInLane.GetCardType() == cardType)
                if (aiDuelLanes[i].cardInLane != null)
                    cardsOnField.Add(aiDuelLanes[i].cardInLane);
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
        if (duelPhase == DuelPhase.AIPhase || duelPhase == DuelPhase.StartPhase || duelPhase == DuelPhase.DrawingPhase) return;

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
    /// Shuffles the deck randomly
    /// </summary>
    /// <param name="deckToShuffle">List of BaseCard type, which is the deck to be shuffled</param>
    /// <returns>Lis of BaseCard type, a shuffled deck</returns>
    private List<BaseCard> ShuffleDeck(List<BaseCard> deckToShuffle)
    {
        return deckToShuffle.OrderBy(a => rnd.Next()).ToList();
    }

    private void SetCardToAttack(BaseCard card)
    {
        card.SetShouldCardAttack(!card.GetIsCardSetToAttack());
    }

    private void OnEarlHealthChangedInternal(BaseEarl earl, int delta, bool playerEarl)
    {
        if (earl.GetHealth() == 0)
        {
            if (!earl.IsPlayer()) WinDuel();
            else LoseDuel();
        }
    }

    private void WinDuel()
    {
        
    }

    private void LoseDuel()
    {
        
    }
    
    /// <summary>
    /// Ends the current turn
    /// </summary>
    /// <param name="aiEndingTurn">If the AI is the one ending the turn</param>
    public void EndTurn(bool aiEndingTurn = false)
    {
        if (duelPhase == DuelPhase.CombatPhase)
        {
            SwitchToCombatState();
        }
        
        if (!aiEndingTurn)
        {
            ChangeDuelPhase(DuelPhase.AIPhase);
            StartCoroutine(LerpCardAttacks(playerDuelLanes, aiDuelLanes, 0, aiEndingTurn));
        }
        else if (aiEndingTurn)
        {
            DrawCardFromDeck();
            numUnitsSummoned = 0;
            numTacticCardsPlayed = 0;
            StartCoroutine(LerpCardAttacks(aiDuelLanes, playerDuelLanes, 0, aiEndingTurn));
        }
    }

    IEnumerator LerpCardAttacks(List<DuelLanes> dl1, List<DuelLanes> dl2, int index, bool aiEndingTurn = false)
    {
        if (!dl1[index].occupied)
        {
            if (index + 1 < playerDuelLanes.Count)
                StartCoroutine(LerpCardAttacks(dl1, dl2, index + 1, aiEndingTurn));
            
            yield break;
        }
        
        BaseCard card = dl1[index].cardInLane;

        Vector3 initialPos = card.transform.position;

        if (dl1[index].cardInLane.GetIsCardSetToAttack())
        {
            Vector3 targetPos;
            if (dl2[index].occupied)
                targetPos = dl2[index].cardInLane.transform.position;
            else if (aiEndingTurn)
                targetPos = selectedPlayerEarl.transform.position;
            else
                targetPos = AIEarl.transform.position;

            float delta = 0;

            while (delta < 1)
            {
                card.transform.position = Vector3.Lerp(initialPos, targetPos, delta);
                delta += Time.deltaTime * 3.0f;
                yield return null;
            }

            if (dl2[index].occupied)
                dl2[index].cardInLane.ApplyHealthChange(-card.GetCardStrength(), false);
            else if (aiEndingTurn)
                selectedPlayerEarl.ApplyHealthChange(-card.GetCardStrength());
            else
                AIEarl.ApplyHealthChange(-card.GetCardStrength());

            dl1[index].cardInLane.SetShouldCardAttack(false);

            while (delta > 0)
            {
                card.transform.position = Vector3.Lerp(initialPos, targetPos, delta);
                delta -= Time.deltaTime * 3.0f;
                yield return null;
            }
        
            card.transform.position = initialPos;
        }
        
        if (index + 1 < playerDuelLanes.Count)
            StartCoroutine(LerpCardAttacks(dl1, dl2, index + 1, aiEndingTurn));
        
        OnTurnEnded?.Invoke(!aiEndingTurn);
        
        if (!aiEndingTurn)
        {
            StartCoroutine(AIStartTurn());
        }
        
        yield return null;
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

        card.GetComponentInChildren<Canvas>().sortingOrder = 1;
        card.SetLaneIndex(laneIndex);
        playerDuelLanes[laneIndex].SetCardInLane(card);
        if (executeEffect) card.ExecuteCardEffect();
        OnCardSummoned?.Invoke(card, card.GetIsPlayerCard());
        yield return null;
    }

    #region AI
    
    IEnumerator AIStartTurn()
    {
        StartCoroutine(AIDrawCard());
        yield return null;
    }

    IEnumerator AIDrawCard(int quantity = 1, bool shouldPlayTurn = true)
    {
        if (aiHand.Count < 6)
        {
            BaseCard card = Instantiate(aiCardsInDeck[0], aiCardSpawn.position, aiCardSpawn.rotation);
            card.AISetUp();
            Vector3 initialPos = card.transform.position;
            Vector3 target = GetFirstUnoccupiedTargetTransform(false);
            float delta = 0;

            while (delta < 1)
            {
                card.transform.position = Vector3.Lerp(initialPos, target, delta);
                delta += Time.deltaTime * 10f;
                yield return null;
            }
            
            card.transform.position = target;
            card.SetUp(false);
            aiHand.Add(card);
            aiCardsInDeck.RemoveAt(0);

            if (quantity > 1)
                StartCoroutine(AIDrawCard(quantity - 1, shouldPlayTurn));

            yield return new WaitForSeconds(1.0f - UnityRandom.Range(-0.5f, 0.5f));
        }

        if (quantity <= 1 && shouldPlayTurn)
            StartCoroutine(AIPlayCard());
        
        yield return null;
    }

    IEnumerator AIPlayCard()
    {
        bool hasUnitsInHand = DoesAIHaveUnitsInHand();
        bool hasTacticInHand = DoesAIHaveTacticCardsInHand();
        bool playedTacticThisTurn = false;
        
        if (hasUnitsInHand && numUnitsSummoned < maxUnitsSummonedPerTurn)
        {
            int laneIndex = GetLaneIndexThatMakesSense();
            BaseCard cardToPlay = GetInfantryCardToPlay(laneIndex);

            if (cardToPlay != null && !aiDuelLanes[laneIndex].occupied && numUnitsSummoned < maxUnitsSummonedPerTurn)
            {
                cardToPlay.AIPlayedCard();
                float delta = 0;
                Vector3 initialPos = cardToPlay.transform.position;
                cardToPlay.GetComponentInChildren<Canvas>().sortingOrder = 1;
                cardToPlay.SetLaneIndex(laneIndex);
                AISortOutHand(cardToPlay);
                aiDuelLanes[laneIndex].occupied = true;
                aiDuelLanes[laneIndex].cardInLane = cardToPlay;
                numUnitsSummoned++;

                while (delta < 1)
                {
                    cardToPlay.transform.position = Vector3.Lerp(initialPos, aiDuelLanes[laneIndex].snapPoint.position, delta);
                    delta += Time.deltaTime * 10f;
                    yield return null;
                }
            }

            if (numUnitsSummoned < maxUnitsSummonedPerTurn)
            {
                if (IsHandFullWithNoInfantry())
                {
                    foreach (var card in aiHand)
                    {
                        if (card.IsCardTactic() && !playedTacticThisTurn)
                        {
                            card.GetCardEffect().SpecialEffect();
                            AISortOutHand(card);
                            playedTacticThisTurn = true;
                            yield break;
                        }
                    }
                }
            }
        }

        yield return new WaitForSeconds(1.0f - UnityRandom.Range(-0.5f, 0.5f));
        
        if (hasTacticInHand)
        {
            foreach (var card in aiHand)
            {
                if (card.IsCardTactic() && !playedTacticThisTurn)
                {
                    if (ShouldAIPlayTacticCard(card))
                    {
                        card.GetCardEffect().SpecialEffect();
                        AISortOutHand(card);
                        playedTacticThisTurn = true;
                    }
                }
            }
        }
        
        yield return new WaitForSeconds(1.0f - UnityRandom.Range(-0.5f, 0.5f));
        StartCoroutine(EvaluateForCombat());
        
        yield return null;
    }

    IEnumerator EvaluateForCombat()
    {
        for (int i = 0; i < aiDuelLanes.Count; i++)
        {
            int errorInt = rnd.Next(0, 100);
            
            if (aiDuelLanes[i].occupied)
            {
                if (errorInt < aiSmartMoveChance && aiDuelLanes[i].cardInLane.GetCardName() != "The Priest")
                {
                    aiDuelLanes[i].cardInLane.SetShouldCardAttack(true);
                    yield return new WaitForSeconds(1.0f - UnityRandom.Range(-0.5f, 0.5f));

                }
                else
                {
                    aiDuelLanes[i].cardInLane.SetShouldCardAttack(false);
                }
            }
        }
        
        yield return null;
        EndTurn(true);
    }
    
    private bool DoesAIHaveUnitsInHand()
    {
        bool unitsInHand = false;
        
        foreach (var card in aiHand)
        {
            if (!card.IsCardTactic())
                unitsInHand = true;
        }
        
        return unitsInHand;
    }

    private bool DoesAIHaveTacticCardsInHand()
    {
        bool tacticInHand = false;

        foreach (var card in aiHand)
        {
            if (card.IsCardTactic())
                tacticInHand = true;
        }

        return tacticInHand;
    }

    private bool ShouldAIPlayTacticCard(BaseCard card)
    {
        bool shouldPlay = false;
        bool isCardMedic = false;
        int errorInt = rnd.Next(0, 100);

        if (errorInt > aiSmartMoveChance) return true;
        
        if (card.IsCardTactic())
        {
            if (card.GetCardName() == "Medic")
                isCardMedic = true;
        }

        if (isCardMedic)
        {
            foreach (var lane in aiDuelLanes)
            {
                if (lane.occupied && lane.cardInLane && !lane.cardInLane.IsAtMaxHealth())
                {
                    shouldPlay = true;
                }
            }
        }
        else
        {
            int chanceToPlay = rnd.Next(0, 100);
            if (chanceToPlay > 30) shouldPlay = true;
        }

        return shouldPlay;
    }

    private bool IsHandFullWithNoInfantry()
    {
        bool hasInfantry = false;
        bool handFull = aiHand.Count == 6;

        foreach (var card in aiHand)
        {
            if (!card.IsCardTactic())
                hasInfantry = true;
        }
        
        return !hasInfantry && handFull;
    }

    private int GetLaneIndexThatMakesSense()
    {
        int laneIndex = 0;
        int errorInt = rnd.Next(0, 100);
        bool wasLaneOccupied = false;
        
        for (int i = 0; i < playerDuelLanes.Count; i++)
        {
            if (playerDuelLanes[i].occupied && !aiDuelLanes[i].occupied)
            {
                wasLaneOccupied = true;
                if (errorInt < aiSmartMoveChance)
                {
                    laneIndex = i;
                }
                else
                {
                    laneIndex = GetLaneIndexThatMakesSense();
                }
            }
        }

        if (!wasLaneOccupied)
        {
            for (int i = 0; i < aiDuelLanes.Count; i++)
            {
                if (!aiDuelLanes[i].occupied) laneIndex = i;
            }
        }

        return laneIndex;
    }

    private BaseCard GetInfantryCardToPlay(int laneIndex)
    {
        BaseCard returnCard = null;
        int errorInt = rnd.Next(0, 100);

        foreach (var card in aiHand)
        {
            if (!playerDuelLanes[laneIndex].occupied)
            {
                if (!card.IsCardTactic())
                    return card;
            }

            if (errorInt < aiSmartMoveChance)
            {
                if (card.GetCardStrength() > playerDuelLanes[laneIndex].cardInLane.GetCardStrength() && !card.IsCardTactic())
                    returnCard = card;
                else
                    returnCard = card;
            }
            else
            {
                if (card.GetCardStrength() < playerDuelLanes[laneIndex].cardInLane.GetCardStrength() && !card.IsCardTactic())
                    returnCard = card;
                else
                    returnCard = card;
            }
        }

        return returnCard;
    }

    #endregion
}
