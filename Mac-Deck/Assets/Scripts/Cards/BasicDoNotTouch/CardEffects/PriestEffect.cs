using System;
using UnityEngine;
using UnityEngine.UI;

public class PriestEffect : BaseCardEffect
{
    [SerializeField] private int amountToHeal = 1;
    
    private bool selectingCard = false;

    private void Awake()
    {
        GetComponentInChildren<Canvas>().worldCamera = Camera.main;
        foreach (var card in DuelManager.GetInstance().GetAllFriendlyCardsOnField())
        {
            card.OnCardSelected.AddListener(HealCard);
        }

        DuelManager.GetInstance().OnCardSummoned.AddListener(OnCardSummoned);
        DuelManager.GetInstance().OnTurnEnded.AddListener(OnTurnEnded);
    }

    private void OnDisable()
    {
        foreach (var card in DuelManager.GetInstance().GetAllFriendlyCardsOnField())
        {
            card.OnCardSelected.RemoveListener(HealCard);
        }
        
        DuelManager.GetInstance().OnCardSummoned.RemoveListener(OnCardSummoned);
        DuelManager.GetInstance().OnTurnEnded.RemoveListener(OnTurnEnded);
    }

    private void Update()
    {
        if (selectingCard)
            if (Input.GetMouseButtonDown(1))
                selectingCard = false;
    }

    private void OnCardSummoned(BaseCard card, bool isPlayerCard)
    {
        if (isPlayerCard == isThisPlayerCard)
            card.OnCardSelected.AddListener(HealCard);
    }

    private void HealCard(BaseCard cardToHeal)
    {
        if (!selectingCard || usedThisTurn) return;
        
        cardToHeal.ApplyHealthChange(amountToHeal);
        selectingCard = false;
        usedThisTurn = true;
        
        foreach (var card in DuelManager.GetInstance().GetAllFriendlyCardsOnField())
        {
            card.cardTemplate.raycastTarget = false;
        }
        
        Image img = gameObject.GetComponentInChildren<Image>();
        if (img)
            img.raycastTarget = true;
    }

    private void OnTurnEnded(bool wasPlayerTurn)
    {
        ResetAfterTurn();
    }
    
    public override void SpecialEffect()
    {
        if (usedThisTurn || DuelManager.GetInstance().GetDuelPhase() != DuelPhase.MainPhase) return;
        
        Image img = gameObject.GetComponentInChildren<Image>();
        if (img)
            img.raycastTarget = false;
        
        foreach (var card in DuelManager.GetInstance().GetAllFriendlyCardsOnField())
        {
            card.cardTemplate.raycastTarget = true;
        }
        
        selectingCard = true;
    }
}
