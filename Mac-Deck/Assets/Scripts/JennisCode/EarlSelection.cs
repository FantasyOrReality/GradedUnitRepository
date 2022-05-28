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
}
