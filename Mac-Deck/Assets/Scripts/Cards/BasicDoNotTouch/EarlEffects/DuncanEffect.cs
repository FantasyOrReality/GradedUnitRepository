using System.Collections.Generic;
using UnityEngine;

public class DuncanEffect : BaseEarlEffect
{
    private int numUnitsHealed;
    
    public override void SetUp()
    {
        DuelManager.GetInstance().OnCardHealthChanged.AddListener(OnCardHealthChanged);
        DuelManager.GetInstance().OnEarlHealthChanged.AddListener(OnEarlHealthChanged);
    }

    public override void SpecialEffect()
    {
        List<BaseCard> friendlyCardsOnField = DuelManager.GetInstance().GetAllFriendlyCardsOnField();

        foreach (var card in friendlyCardsOnField)
        {
            card.ApplyHealthChange(1);
        }
    }

    public void OnEarlHealthChanged(BaseEarl earl, int delta)
    {
        CheckWasUnitHealed(delta);
    }

    public void OnCardHealthChanged(BaseCard card, int delta)
    {
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
