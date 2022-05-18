using System.Collections.Generic;

public class SiwardEarlEffect : BaseEarlEffect
{
    private int numLightUnits;
    
    public override void SetUp()
    {
        DuelManager.GetInstance().OnCardSummoned.AddListener(OnCardSummon);
    }

    public override void SpecialEffect()
    {
        List<BaseCard> cardsOnField = DuelManager.GetInstance().GetAllFriendlyCardsOnFieldOfType(CardType.Special);
        foreach (var card in cardsOnField)
        {
            if (card.GetCardName() == "Master Assassin")
            {
                card.SetCanBeAttacked(false);
                card.ApplyAttackChange(1);
            }
        }
    }
    
    public void OnCardSummon(BaseCard card)
    {
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
