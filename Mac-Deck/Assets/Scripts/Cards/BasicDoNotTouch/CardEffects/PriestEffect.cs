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
        transform.parent.gameObject.GetComponent<BaseCard>().OnCardSelected.AddListener(HealCard);
    }

    private void Update()
    {
        if (selectingCard)
            if (Input.GetMouseButtonDown(1))
                selectingCard = false;
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
    
    public override void SpecialEffect()
    {
        if (usedThisTurn) return;
        
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
