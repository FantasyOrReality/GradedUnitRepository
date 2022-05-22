using UnityEngine;

public class BaseEarlEffect : MonoBehaviour, SpecialAbilityInterface
{
    protected bool usedThisTurn = false;
    protected bool isThisPlayerEarl = true;
    
    public virtual void SetUp()
    {
        
    }
    
    public virtual void SpecialEffect()
    {
        
    }

    public void ResetAfterTurn()
    {
        usedThisTurn = false;
    }
    
    public void SetIsThisPlayerEarl(bool newValue)
    {
        isThisPlayerEarl = newValue;
    }
}
