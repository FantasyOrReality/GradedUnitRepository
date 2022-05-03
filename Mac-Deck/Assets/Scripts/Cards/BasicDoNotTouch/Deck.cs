using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeckData", menuName = "Cards/DeckData", order = 2)][Serializable]
public class Deck : ScriptableObject
{
    public List<BaseCard> cardsInDeck = new List<BaseCard>(25);
}
