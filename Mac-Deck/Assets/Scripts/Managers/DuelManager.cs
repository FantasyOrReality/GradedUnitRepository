using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DuelLanes
{
    public Transform upperLimit;
    public Transform lowerLimit;
    public Transform snapPoint;

    public BaseCard cardInLane;

    public void SetCardInLane(BaseCard inCard)
    {
        cardInLane = inCard;
    }
}

public class DuelManager : MonoBehaviour
{
    static DuelManager instance;

    public SNameGenerator nameGen;

    [SerializeField] private List<DuelLanes> playerDuelLanes = new List<DuelLanes>(4);
    [SerializeField] private Transform tacticUpperLimit;
    [SerializeField] private Transform tacticLowerLimit;

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
                StartCoroutine(LerpCardToPlace(cardToPlay, i));
                return true;
            }
            
        }
        return false;
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
        
        playerDuelLanes[laneIndex].SetCardInLane(card);
        yield return null;
    }
}
