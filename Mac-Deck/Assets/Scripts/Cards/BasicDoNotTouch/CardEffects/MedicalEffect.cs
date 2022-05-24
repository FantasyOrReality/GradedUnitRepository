using UnityEngine;

public class MedicalEffect : BaseCardEffect
{
    public int amountToHeal;
    
    public override void SpecialEffect()
    {
        if (isThisPlayerCard)
        {
            foreach (var card in DuelManager.GetInstance().GetAllFriendlyCardsOnField())
            {
                card.ApplyHealthChange(amountToHeal, true);
            }
        }
        else
        {
            foreach (var card in DuelManager.GetInstance().GetAllAICardsOnField())
            {
                card.ApplyHealthChange(amountToHeal, true);
            }
        }

        Destroy(gameObject.transform.parent.gameObject);
    }
}
