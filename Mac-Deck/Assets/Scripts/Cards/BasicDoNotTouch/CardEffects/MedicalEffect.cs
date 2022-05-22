using UnityEngine;

public class MedicalEffect : BaseCardEffect
{
    public int amountToHeal;
    
    public override void SpecialEffect()
    {
        // @TODO: Once enemy cards have been implemented convert this to be able to heal enemy cards as well, also fix Provisions
        
        foreach (var card in DuelManager.GetInstance().GetAllFriendlyCardsOnField())
        {
            card.ApplyHealthChange(amountToHeal);
        }
        
        Destroy(gameObject.transform.parent.gameObject);
    }
}
