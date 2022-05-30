using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EarlSelection : MonoBehaviour
{
    [SerializeField] private List<EarlData> earls;
    [SerializeField] private List<Image> images;
    [SerializeField] private List<TextMeshProUGUI> names;
    [SerializeField] private Button duelButton;
    [SerializeField] private TextMeshProUGUI selectEarlText;

    public void Awake()
    {
        for (int i = 0; i < earls.Count; i++)
        {
            images[i].sprite = earls[i].earlImage;
            names[i].text = earls[i].earlName + " " + earls[i].earlSuffix;
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Start()
    {
        duelButton.onClick.AddListener(ChangeSceneToDuelScene);
    }

    public void ChangeSceneToDuelScene()
    {
        if (GameManager.GetInstance().playerEarl != null)
            GameManager.GetInstance().SwitchToDuelScene();
        else
            selectEarlText.gameObject.SetActive(true);
    }
}
