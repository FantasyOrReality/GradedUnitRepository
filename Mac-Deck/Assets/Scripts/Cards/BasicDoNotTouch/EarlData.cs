using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EarlData", menuName = "Earls/EarlData", order = 0)][Serializable]
public class EarlData : ScriptableObject
{
    public string earlName;
    public string earlSuffix;
    public int earlHealth;

    [TextArea(3, 20)]
    public string earlDescription;
    public Sprite earlImage;
    public Deck earlDeck;
}
