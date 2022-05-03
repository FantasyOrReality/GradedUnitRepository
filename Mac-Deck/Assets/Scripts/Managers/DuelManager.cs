using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelManager : MonoBehaviour
{
    static DuelManager instance;

    public SNameGenerator nameGen;

    public static DuelManager GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        GameObject.Instantiate(nameGen);
        
        if (instance != null)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public bool TryPlayCard(BaseCard cardToPlay)
    {
        Debug.Log("Card played: " + cardToPlay.GetCardData().cardName);
        return false;
    }
}
