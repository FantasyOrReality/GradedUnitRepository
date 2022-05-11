using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BaseEarl : MonoBehaviour
{
    [SerializeField] private EarlData earlData;

    private int currHealth;

    private void Awake()
    {
        currHealth = earlData.earlHealth;

        GetComponentInChildren<Image>().sprite = earlData.earlImage;
        GetComponentInChildren<TextMeshProUGUI>().text = GetEarlNameAndSuffix();
        
        // @TODO: Instantiate components on these objects
        switch (earlData.earlName)
        {
            case "MacBeth":
                
                break;
            case "Duncan":
                
                break;
            case "Siward":
                
                break;
            case "The Twins":
                
                break;
        }
    }

    public bool ApplyHealthChange(int delta)
    {
        currHealth += delta;
        
        return currHealth != 0;
    }

    public int GetHealth()
    {
        return currHealth;
    }

    public Sprite GetEarlImage()
    {
        return earlData.earlImage;
    }

    public string GetEarlName()
    {
        return earlData.earlName;
    }

    public string GetEarlSuffix()
    {
        return earlData.earlSuffix;
    }

    public string GetEarlDescription()
    {
        return earlData.earlDescription;
    }

    public string GetEarlNameAndSuffix()
    {
        return earlData.earlName + "<br>" + earlData.earlSuffix;
    }
    
    public List<BaseCard> GetEarlDeck()
    {
        return earlData.earlDeck.cardsInDeck;
    }
}
