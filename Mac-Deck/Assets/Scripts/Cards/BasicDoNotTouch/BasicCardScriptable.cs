using UnityEngine;

public enum CardType
{
    Special,
    Infantry,
    Tactic
}

[CreateAssetMenu(fileName = "CardData", menuName = "Cards/CardData", order = 1)]
public class BasicCardScriptable : ScriptableObject
{
    public string cardName;
    public string cardDescription;
    public int cardStrength;
    public int cardHealth;
    public CardType cardType;
    public Sprite CardImage;
}
