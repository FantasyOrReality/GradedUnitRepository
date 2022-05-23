using System;
using System.Collections.Generic;
using UnityEngine;

public class SiwardEarlEffect : BaseEarlEffect
{
    private int numLightUnits;
    [SerializeField] private int attackToAdd = 1;
    
    public override void SetUp()
    {
        DuelManager.GetInstance().OnCardSummoned.AddListener(OnCardSummon);
    }

    private void OnDisable()
    {
        DuelManager.GetInstance().OnCardSummoned.RemoveListener(OnCardSummon);
    }

    public override void SpecialEffect()
    {
        List<BaseCard> cardsOnField = DuelManager.GetInstance().GetAllFriendlyCardsOnFieldOfType(CardType.Special);
        foreach (var card in cardsOnField)
        {
            if (card.GetCardName() == "Master Assassin")
            {
                card.SetCanBeAttacked(false);
                card.ApplyAttackChange(attackToAdd);
            }
        }
    }
    
    private void OnCardSummon(BaseCard card, bool isPlayer)
    {
        if (isPlayer != isThisPlayerEarl) return;
        
        if (card.GetCardType() == CardType.LSoldier || card.GetCardType() == CardType.Special && card.GetCardName() == "Master Assassin")
        {
            numLightUnits++;
            if (numLightUnits == 4)
            {
                SpecialEffect();
                DuelManager.GetInstance().OnCardSummoned.RemoveListener(OnCardSummon);
            }
        }
    }
}
