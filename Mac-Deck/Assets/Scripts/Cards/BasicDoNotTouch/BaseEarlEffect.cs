using UnityEngine;

public class BaseEarlEffect : MonoBehaviour
{
    protected bool usedThisTurn = false;
    
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
}
