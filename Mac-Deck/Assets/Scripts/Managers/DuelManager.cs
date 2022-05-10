using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = System.Random;

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

    public SNameGenerator nameGen;

    public UnityEvent<BaseCard> OnCardSummoned;
    public UnityEvent<BaseCard, int> OnCardHealthChanged;

    [SerializeField] private List<DuelLanes> playerDuelLanes = new List<DuelLanes>(4);
    [SerializeField] private List<CardTarget> cardTargets = new List<CardTarget>(6);
    [SerializeField] private List<BaseCard> playerHand = new List<BaseCard>(6);
    [SerializeField] private Transform tacticUpperLimit;
    [SerializeField] private Transform tacticLowerLimit;
    [SerializeField] private Transform cardSpawn;
    [SerializeField] private Text remainingCardsText;

    [SerializeField] private Deck selectedDeck;

    private List<BaseCard> cardsInDeck = new List<BaseCard>(25);

    public static DuelManager GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        GameObject.Instantiate(nameGen);
        
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

        foreach (var card in selectedDeck.cardsInDeck)
        {
            cardsInDeck.Add(card);
        }

        Random rnd = new Random();
        cardsInDeck = cardsInDeck.OrderBy(a => rnd.Next()).ToList(); 

        remainingCardsText.text = cardsInDeck.Count.ToString();
    }

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

    public bool TryPlayCard(BaseCard cardToPlay)
    {
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
        
        for (int i = 0; i < playerDuelLanes.Count; i++)
        {
            if (cardToPlay.transform.position.x > playerDuelLanes[i].upperLimit.position.x &&
                cardToPlay.transform.position.y < playerDuelLanes[i].upperLimit.position.y &&
                cardToPlay.transform.position.x < playerDuelLanes[i].lowerLimit.position.x &&
                cardToPlay.transform.position.y > playerDuelLanes[i].lowerLimit.position.y)
            {
                if (playerDuelLanes[i].occupied) return false;

                SortOutHand(cardToPlay);
                
                StartCoroutine(LerpCardToPlace(cardToPlay, i));
                return true;
            }
            
        }
        return false;
    }

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
