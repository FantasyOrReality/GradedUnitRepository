using System;
using System.Collections.Generic;
using UnityEngine;

public class DuncanEffect : BaseEarlEffect
{
    private int numUnitsHealed;
    [SerializeField] private int amountToHeal = 1;
    
    public override void SetUp()
    {
        DuelManager.GetInstance().OnCardHealthChanged.AddListener(OnCardHealthChanged);
        DuelManager.GetInstance().OnEarlHealthChanged.AddListener(OnEarlHealthChanged);
    }

    private void OnDisable()
    {
        DuelManager.GetInstance().OnCardHealthChanged.RemoveListener(OnCardHealthChanged);
        DuelManager.GetInstance().OnEarlHealthChanged.RemoveListener(OnEarlHealthChanged);
    }

    public override void SpecialEffect()
    {
        if (audioSource)
            audioSource.Play();
        if (isThisPlayerEarl)
        {
            List<BaseCard> friendlyCardsOnField = DuelManager.GetInstance().GetAllFriendlyCardsOnField();
        
            foreach (var card in friendlyCardsOnField)
            {
                card.ApplyHealthChange(amountToHeal);
            }
        }
        else
        {
            List<BaseCard> friendlyCardsOnField = DuelManager.GetInstance().GetAllAICardsOnField();
        
            foreach (var card in friendlyCardsOnField)
            {
                card.ApplyHealthChange(amountToHeal);
            }
        }
    }

    private void OnEarlHealthChanged(BaseEarl earl, int delta, bool isPlayer)
    {
        if (isPlayer != isThisPlayerEarl) return;
        
        CheckWasUnitHealed(delta);
    }

    private void OnCardHealthChanged(BaseCard card, int delta, bool isPlayer)
    {
        if (isPlayer != card.GetIsPlayerCard()) return;
        
        CheckWasUnitHealed(delta);
    }

    private void CheckWasUnitHealed(int delta)
    {
        if (usedThisTurn) return;
        
        if (delta > 0)
        {
            numUnitsHealed++;
            if (numUnitsHealed == 4)
            {
                usedThisTurn = true;
                SpecialEffect();
            }
        }
    }
}
