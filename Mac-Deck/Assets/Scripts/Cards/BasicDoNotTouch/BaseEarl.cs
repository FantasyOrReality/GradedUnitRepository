using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BaseEarl : MonoBehaviour
{
    [SerializeField] private EarlData earlData;
    [SerializeField] private TextMeshProUGUI earlName;
    [SerializeField] private TextMeshProUGUI earlHealth;
    [SerializeField] private Image earlEffectSprite;

    private GameObject earlEffect;
    private bool isPlayerEarl = true;

    private int currHealth;
    
    // In awake we set up the earl class to be used
    private void Awake()
    {
        currHealth = earlData.earlHealth;

        GetComponentInChildren<Image>().sprite = earlData.earlImage;
        earlName.text = GetEarlNameAndSuffix();

        if (earlData.earlEffect != null)
        {
            earlEffect = Instantiate(earlData.earlEffect, transform);
            BaseEarlEffect eff = earlEffect.GetComponent<BaseEarlEffect>();
            eff.SetUp();
            eff.SetIsThisPlayerEarl(true);
            earlEffectSprite.gameObject.SetActive(true);
            earlEffectSprite.sprite = earlData.effectSprite;
        }

        earlHealth.text = currHealth.ToString();
    }

    public void AISetUp()
    {
        Vector3 newPositionName = earlName.gameObject.transform.localPosition;
        newPositionName.y = -1 * newPositionName.y;
        isPlayerEarl = false;
        earlName.gameObject.transform.localPosition = newPositionName;
        earlEffectSprite.transform.localPosition = -earlEffectSprite.transform.localPosition;
        Vector3 newPositionHealth = earlHealth.transform.localPosition;
        newPositionHealth.x = -newPositionHealth.x;
        earlHealth.transform.localPosition = newPositionHealth;
        
        if (earlEffect)
            earlEffect.GetComponent<BaseEarlEffect>().SetIsThisPlayerEarl(false);
    }

    /// <summary>
    /// Applies a Health Change to the Earl
    /// </summary>
    /// <param name="delta">The change in health to the Earl, positive numbers heal, negative do damage</param>
    /// <returns>Boolean, if health change has been applied</returns>
    public bool ApplyHealthChange(int delta, bool instigate = true)
    {
        int previousHealth = currHealth;
        currHealth = Mathf.Clamp(currHealth + delta, 0, 100);

        earlHealth.text = currHealth.ToString();
        
        if (instigate)
            DuelManager.GetInstance().OnEarlHealthChanged?.Invoke(this, currHealth - previousHealth, isPlayerEarl);
        
        return currHealth != previousHealth;
    }

    public bool IsPlayer()
    {
        return isPlayerEarl;
    }
    
    public void SetIsPlayer(bool newValue)
    {
        isPlayerEarl = newValue;
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
