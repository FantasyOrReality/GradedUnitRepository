using UnityEngine;

public class MedicalEffect : BaseCardEffect
{
    public int amountToHeal;
    
    public override void SpecialEffect()
    {
        foreach (var card in DuelManager.GetInstance().GetAllFriendlyCardsOnField())
        {
            card.ApplyHealthChange(amountToHeal);
        }
        
        Destroy(gameObject.transform.parent.gameObject);
    }
}
