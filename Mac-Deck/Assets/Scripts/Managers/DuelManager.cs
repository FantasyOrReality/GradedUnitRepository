using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Random = System.Random;

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
    public UnityEvent<BaseCard> OnCardSummoned;

    [HideInInspector]
    public UnityEvent<BaseCard, int> OnCardHealthChanged;

    [SerializeField] private List<DuelLanes> playerDuelLanes = new List<DuelLanes>(4);
    [SerializeField] private List<CardTarget> cardTargets = new List<CardTarget>(6);
    private List<BaseCard> playerHand = new List<BaseCard>(6);

    [Space(10)]
    [Header("Transforms")]
    [SerializeField] private Transform tacticUpperLimit;
    [SerializeField] private Transform tacticLowerLimit;
    [SerializeField] private Transform cardSpawn;
    [SerializeField] private Transform earlTarget;

    [Space(10)]
    [Header("User Interface")]
    [SerializeField] private Text remainingCardsText;
    
    [Space(10)]
    [Header("Earls")]
    [SerializeField] private BaseEarl selectedPlayerEarl;

    private List<BaseCard> cardsInDeck = new List<BaseCard>(25);

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
    /// Finds the first target transform that does not have a card in it
    /// </summary>
    /// <returns>Returns a position of where we can place the last card</returns>
    private Vector3 GetFirstUnoccupiedTargetTransform()
    {
        for (int i = 0; i < cardTargets.Count; i++)
        {
            if (!cardTargets[i].occupied)
            {
                cardTargets[i].SetIsOccupied(true);
                return cardTargets[i].target.transform.position;
            }
        }

        cardTargets[0].SetIsOccupied(true);
        return cardTargets[0].target.transform.position;
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
    /// <returns>Boolean, true when the card can be played and false when it can not be</returns>
    public bool TryPlayCard(BaseCard cardToPlay)
    {
        // Check if the card is a tactic and if so use the tactic card limits
        if (cardToPlay.IsCardTactic())
        {
            if (cardToPlay.transform.position.x > tacticUpperLimit.position.x &&
                cardToPlay.transform.position.y < tacticUpperLimit.position.y &&
                cardToPlay.transform.position.x < tacticLowerLimit.position.x &&
                cardToPlay.transform.position.y > tacticLowerLimit.position.y)
            {
                SortOutHand(cardToPlay);
                cardToPlay.CardEffect();
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
                
                    StartCoroutine(LerpCardToPlace(cardToPlay, i));
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
        if (playerHand.Count >= 6) return;
        
        for (int i = 0; i < quantity; ++i)
        {
            if (cardsInDeck.Count == 0) return;
            
            BaseCard card = Instantiate(cardsInDeck[i], cardSpawn.position, cardSpawn.rotation);
            playerHand.Add(card);
            cardsInDeck.RemoveAt(i);
            StartCoroutine(LerpCardFromDeckToLocation(card, GetFirstUnoccupiedTargetTransform()));
        }
        
        if (cardsInDeck.Count != 0)
            remainingCardsText.text = cardsInDeck.Count.ToString();
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
    
    /// <summary>
    /// Lerp the card from the Deck to the required hand location, drawn from DrawCardFromDeck function
    /// </summary>
    /// <param name="card">BaseCard type, card that is drawn</param>
    /// <param name="target">Vector3 type, the target to where the card should lerp to, usually a hand position</param>
    /// <returns>null</returns>
    IEnumerator LerpCardFromDeckToLocation(BaseCard card, Vector3 target)
    {
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
        
        yield return null;
    }
    
    /// <summary>
    /// Lerp the card into the duel lane position, where it was played at
    /// </summary>
    /// <param name="card">BaseCard type, the card that needs to be moved</param>
    /// <param name="laneIndex">Integer, the index of lane the card should move to</param>
    /// <returns>null</returns>
    IEnumerator LerpCardToPlace(BaseCard card, int laneIndex)
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
        OnCardSummoned?.Invoke(card);
        yield return null;
    }
}
