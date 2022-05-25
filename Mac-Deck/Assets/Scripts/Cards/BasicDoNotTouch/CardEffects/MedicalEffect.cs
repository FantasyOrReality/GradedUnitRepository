using UnityEngine;
using UnityEngine.Audio;


public class MedicalEffect : BaseCardEffect
{
    public int amountToHeal;
    
    public override void SpecialEffect()
    {
        if (audioSource)
            audioSource.Play();

        if (isThisPlayerCard)
        {
            foreach (var card in DuelManager.GetInstance().GetAllFriendlyCardsOnField())
            {
                card.ApplyHealthChange(amountToHeal);
            }
        }
        else
        {
            foreach (var card in DuelManager.GetInstance().GetAllAICardsOnField())
            {
                card.ApplyHealthChange(amountToHeal);
            }
        }

        Destroy(gameObject.transform.parent.gameObject);
    }
}
