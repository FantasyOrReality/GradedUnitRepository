using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BaseCard : MonoBehaviour, CardInterface
{
    [SerializeField] private BasicCardScriptable cardData;

    private bool isTacticCard = false;

    private void Awake() 
    {
        GetComponentInChildren<Canvas>().worldCamera = Camera.main;

        foreach (var image in GetComponentsInChildren<Image>())
        {
            if (image.tag == "Image") image.sprite = cardData.CardImage;
        }

        foreach (var text in GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (text.tag == "Strength") text.text = cardData.cardStrength.ToString();
            else if (text.tag == "Health") text.text = cardData.cardHealth.ToString();
            else if (text.tag == "Description") text.text = cardData.cardDescription;
            else if (text.tag == "Name") text.text = cardData.cardName.ToString();
            else if (text.tag == "Type") text.text = CardTypeToString(cardData.cardType);
        }
    }

    private string CardTypeToString(CardType type)
    {
        switch (type)
        {
            case CardType.Infantry:
                return "Infantry";
            case CardType.Tactic:
                return "Tactic";
            case CardType.Special:
                return "Special";
        }

        return null;
    }

    public virtual void CardEffect()
    {

    }

    public bool GetIsCardTactic()
    {
        return isTacticCard;
    }
}
