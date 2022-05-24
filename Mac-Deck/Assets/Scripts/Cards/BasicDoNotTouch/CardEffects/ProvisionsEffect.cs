using UnityEngine;

public class ProvisionsEffect : BaseCardEffect
{
    [SerializeField] private int attackToAdd = 1;
    
    public override void SpecialEffect()
    {
        if (isThisPlayerCard)
        {
            foreach (var card in DuelManager.GetInstance().GetAllFriendlyCardsOnField())
            {
                card.ApplyAttackChange(attackToAdd, true);
            }
        }
        else
        {
            foreach (var card in DuelManager.GetInstance().GetAllAICardsOnField())
            {
                card.ApplyAttackChange(attackToAdd, true);
            }
        }

        Destroy(gameObject.transform.parent.gameObject);
    }
}
