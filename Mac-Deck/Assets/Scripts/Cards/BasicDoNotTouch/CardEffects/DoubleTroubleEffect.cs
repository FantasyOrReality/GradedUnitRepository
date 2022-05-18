using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class DoubleTroubleEffect : BaseCardEffect
{
    private void OnEnable()
    {
        DuelManager.GetInstance().OnCardSummoned.AddListener(OnCardSummon);
    }

    private void OnDisable()
    {
        DuelManager.GetInstance().OnCardSummoned.RemoveListener(OnCardSummon);
    }

    public override void SpecialEffect()
    {
        BaseCard card = gameObject.GetComponentInChildren<BaseCard>();
        card.ApplyHealthChange(1);
        card.ApplyAttackChange(2);
    }

    private void OnCardSummon(BaseCard card)
    {
        if (card.GetCardType() == CardType.Special && card.GetCardName() == "Double Trouble")
        {
            SpecialEffect();
        }
    }
}
