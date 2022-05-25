using System;
using System.Collections.Generic;
using UnityEngine;

public class MacbethEffect : BaseEarlEffect
{
    private int numUnitsDead;

    [SerializeField] private int attackToAdd;
    
    public override void SetUp()
    {
        DuelManager.GetInstance().OnCardDestroyed.AddListener(OnCardHealthChanged);
    }

    private void OnDisable()
    {
        DuelManager.GetInstance().OnCardDestroyed.RemoveListener(OnCardHealthChanged);
    }

    public override void SpecialEffect()
    {
        if (audioSource)
            audioSource.Play();

        if (isThisPlayerEarl)
        {
            List<BaseCard> cards = DuelManager.GetInstance().GetAllFriendlyCardsOnField();

            foreach (var card in cards)
            {
                card.ApplyAttackChange(attackToAdd);
            }
        }
        else
        {
            List<BaseCard> cards = DuelManager.GetInstance().GetAllAICardsOnField();

            foreach (var card in cards)
            {
                card.ApplyAttackChange(attackToAdd);
            }
        }
    }

    private void OnCardHealthChanged(BaseCard card, bool isPlayer)
    {
        if (isPlayer != isThisPlayerEarl) return;
            
        numUnitsDead++;
        if (numUnitsDead == 4)
        {
            SpecialEffect();
            numUnitsDead = 0;
        }
    }
}
