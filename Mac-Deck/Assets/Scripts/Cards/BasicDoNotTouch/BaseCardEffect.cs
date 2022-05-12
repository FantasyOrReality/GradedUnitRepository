using UnityEngine;

public class BaseCardEffect : MonoBehaviour, SpecialAbilityInterface
{
    protected bool usedThisTurn = false;
    
    public virtual void SpecialEffect()
    {
        
    }

    public void ResetAfterTurn()
    {
        usedThisTurn = false;
    }
}
