using UnityEngine;

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
        owningCard.ApplyHealthChange(1);
        owningCard.ApplyAttackChange(2);
        
        if (audioSource)
            audioSource.Play();
    }

    private void OnCardSummon(BaseCard card, bool isPlayerCard)
    {
        if (isPlayerCard == isThisPlayerCard)
        {
            if (card.GetCardType() == CardType.Special && card.GetCardName() == "Double Trouble")
            {
                SpecialEffect();
            }
        }
    }
}
