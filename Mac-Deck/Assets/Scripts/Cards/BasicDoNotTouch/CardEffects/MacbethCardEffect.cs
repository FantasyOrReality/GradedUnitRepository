using System;

public class MacbethCardEffect : BaseCardEffect
{
    private BaseEarl owningEarl;
    public override void SpecialEffect()
    {
        DuelManager manager = DuelManager.GetInstance();

        if (isThisPlayerCard)
            owningEarl = manager.GetPlayerEarl();
        else
            owningEarl = manager.GetAIEarl();
        
        manager.OnEarlHealthChanged.AddListener(OnEarlHealthChanged);
        manager.OnCardHealthChanged.AddListener(OnCardHealthChanged);
        
        owningCard.ApplyHealthChange(owningEarl.GetHealth(), false, true);
    }

    private void OnDisable()
    {
        DuelManager manager = DuelManager.GetInstance();
        manager.OnEarlHealthChanged.RemoveListener(OnEarlHealthChanged);
        manager.OnCardHealthChanged.RemoveListener(OnCardHealthChanged);
    }

    private void OnEarlHealthChanged(BaseEarl earl, int delta, bool isPlayer)
    {
        if (isPlayer == isThisPlayerCard)
        {
            owningCard.ApplyHealthChange(delta, false, true);
        }
    }

    public void OnCardHealthChanged(BaseCard card, int delta, bool isPlayerCard)
    {
        if (card == owningCard)
        {
            owningEarl.ApplyHealthChange(delta, false);
        }
    }
}
