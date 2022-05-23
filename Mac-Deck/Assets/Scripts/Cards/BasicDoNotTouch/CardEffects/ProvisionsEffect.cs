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
                card.ApplyAttackChange(attackToAdd);
            }
        }
        else
        {
            foreach (var card in DuelManager.GetInstance().GetAllAICardsOnField())
            {
                card.ApplyHealthChange(attackToAdd);
            }
        }

        Destroy(gameObject.transform.parent.gameObject);
    }
}
