using UnityEngine;

public class BaseCardEffect : MonoBehaviour, SpecialAbilityInterface
{
    protected bool usedThisTurn = false;
    protected bool isThisPlayerCard = true;
    protected BaseCard owningCard;

    public void SetOwningCard(BaseCard owning)
    {
        owningCard = owning;
    }
    
    public virtual void SpecialEffect()
    {
        
    }
    public void ResetAfterTurn()
    {
        usedThisTurn = false;
    }

    public void SetIsThisPlayerCard(bool newValue)
    {
        isThisPlayerCard = newValue;
    }
}
