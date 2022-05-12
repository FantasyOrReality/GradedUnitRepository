using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BaseEarl : MonoBehaviour
{
    [SerializeField] private EarlData earlData;
    private GameObject earlEffect;

    private int currHealth;
    
    // In awake we set up the earl class to be used
    private void Awake()
    {
        currHealth = earlData.earlHealth;

        GetComponentInChildren<Image>().sprite = earlData.earlImage;
        GetComponentInChildren<TextMeshProUGUI>().text = GetEarlNameAndSuffix();

        if (earlEffect)
        {
            earlEffect = Instantiate(earlData.earlEffect, transform);
        }
    }

    /// <summary>
    /// Applies a Health Change to the Earl
    /// </summary>
    /// <param name="delta">The change in health to the Earl, positive numbers heal, negative do damage</param>
    /// <returns>Boolean, if health change has been applied</returns>
    public bool ApplyHealthChange(int delta)
    {
        int previousHealth = currHealth;
        currHealth = Mathf.Clamp(currHealth + delta, 0, 100);

        DuelManager.GetInstance().OnEarlHealthChanged?.Invoke(this, currHealth);
        return currHealth != previousHealth;
    }
    
    /// <summary>
    /// Gets the current health of the Earl
    /// </summary>
    /// <returns>Integer, the current health of the Earl</returns>
    public int GetHealth()
    {
        return currHealth;
    }

    /// <summary>
    /// Gets the Earl image
    /// </summary>
    /// <returns>Sprite, image of the Earl</returns>
    public Sprite GetEarlImage()
    {
        return earlData.earlImage;
    }

    /// <summary>
    /// Gets the Earl's name
    /// </summary>
    /// <returns>String, the name of the Earl</returns>
    public string GetEarlName()
    {
        return earlData.earlName;
    }

    /// <summary>
    /// Gets the Earl's suffix
    /// </summary>
    /// <returns>String, the suffix of the Earl</returns>
    public string GetEarlSuffix()
    {
        return earlData.earlSuffix;
    }

    /// <summary>
    /// Gets the Earl's description
    /// </summary>
    /// <returns>String, the description of the Earl</returns>
    public string GetEarlDescription()
    {
        return earlData.earlDescription;
    }

    /// <summary>
    /// Gets the suffix and name of the Earl
    /// </summary>
    /// <returns>String in RTF format, with a BR tag for a new line between name and suffix</returns>
    public string GetEarlNameAndSuffix()
    {
        return earlData.earlName + "<br>" + earlData.earlSuffix;
    }
    
    /// <summary>
    /// Gets the Earl's deck
    /// </summary>
    /// <returns>List of BaseCard, the deck of the Earl</returns>
    public List<BaseCard> GetEarlDeck()
    {
        return earlData.earlDeck.cardsInDeck;
    }
}
