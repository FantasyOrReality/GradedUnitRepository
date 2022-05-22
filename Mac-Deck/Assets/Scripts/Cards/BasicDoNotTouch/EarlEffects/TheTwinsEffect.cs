using System.Collections.Generic;
using UnityEngine;

public class TheTwinsEffect : BaseEarlEffect
{
    private BaseCard lastCardSummoned;

    [SerializeField] private int attackAndHealthToAdd;
    
    public override void SetUp()
    {
        DuelManager.GetInstance().OnCardSummoned.AddListener(OnCardSummoned);
    }

    public override void SpecialEffect()
    {
        List<BaseCard> cards = DuelManager.GetInstance().GetAllFriendlyCardsOnFieldOfType(CardType.Special);

        foreach (var card in cards)
        {
            if (card.GetCardName() == "Double Trouble")
            {
                card.ApplyAttackChange(attackAndHealthToAdd);
                card.ApplyHealthChange(attackAndHealthToAdd);
                card.FlipStats();
            }
        }
    }

    private void OnCardSummoned(BaseCard card, bool isPlayerCard)
    {
        if (lastCardSummoned == null || usedThisTurn)
        {
            lastCardSummoned = card;
            return;
        }

        if (lastCardSummoned.GetCardType() == card.GetCardType())
        {
            SpecialEffect();
        }

        usedThisTurn = true;
        lastCardSummoned = card;
    }
}
