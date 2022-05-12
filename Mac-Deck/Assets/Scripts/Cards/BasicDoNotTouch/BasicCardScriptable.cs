using System;
using UnityEngine;

public enum CardType
{
    LSoldier,
    HSoldier,
    Tactic,
    Special
}

[CreateAssetMenu(fileName = "CardData", menuName = "Cards/CardData", order = 1)][Serializable]
public class BasicCardScriptable : ScriptableObject
{
    public string cardName;
    [TextArea(3, 10)]
    public string cardDescription;
    public int cardStrength;
    public int cardHealth;
    public CardType cardType;
    public Sprite CardImage;
    public GameObject cardEffect;
}
