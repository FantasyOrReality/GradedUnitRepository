using UnityEngine;

public class ProvisionsEffect : BaseCardEffect
{
    [SerializeField] private int attackToAdd = 1;
    
    public override void SpecialEffect()
    {
        foreach (var card in DuelManager.GetInstance().GetAllFriendlyCardsOnField())
        {
            card.ApplyAttackChange(attackToAdd);
        }
        
        Destroy(gameObject.transform.parent.gameObject);
    }
}
