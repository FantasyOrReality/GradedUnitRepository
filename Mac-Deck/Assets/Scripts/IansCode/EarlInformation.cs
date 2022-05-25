using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EarlInformation : MonoBehaviour
{
    [SerializeField] private GameObject earlPortraits;
    [SerializeField] private GameObject earlInformation;

    [SerializeField] private Image earlImage;
    [SerializeField] private TextMeshProUGUI earlName;
    [SerializeField] private TextMeshProUGUI earlDescription;

    private bool earlSelected = false;

    public void OnEarlClick(EarlData earlData)
    {
        earlImage.sprite = earlData.earlImage;
        earlName.text = earlData.earlName + earlData.earlSuffix;
        earlDescription.text = earlData.earlDescription;
        earlSelected = true;
        
        earlPortraits.SetActive(false);
        earlInformation.SetActive(true);
    }

    public void BackButton()
    {
        if (earlSelected)
        {
            earlSelected = false;
        
            earlPortraits.SetActive(true);
            earlInformation.SetActive(false);
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
