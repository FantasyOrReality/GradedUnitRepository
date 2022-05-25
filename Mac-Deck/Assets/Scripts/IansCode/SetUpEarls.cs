using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetUpEarls : MonoBehaviour
{
    [SerializeField] private EarlData data;

    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI earlName;

    public void Awake()
    {
        image.sprite = data.earlImage;
        earlName.text = data.earlName + data.earlSuffix;
    }
}
