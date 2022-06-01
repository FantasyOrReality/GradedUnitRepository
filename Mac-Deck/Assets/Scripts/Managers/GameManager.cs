using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class GameManager : MonoBehaviour
{
    private GameObject innerDuelMan;
    [SerializeField] private GameObject duelMan;
    [SerializeField] private string mainMenuScene;
    [SerializeField] private string duelScene;
    [SerializeField] public BaseEarl playerEarl;
    [SerializeField] private List<BaseEarl> Earls;

    private List<BaseEarl> internalEarls = new List<BaseEarl>(4);

    private static GameManager instance;

    private void Awake()
    {
        // Singleton PART Start
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
        // Singleton PART End

        PopulateEarlList()  ;
    }

    public void SwitchToDuelScene()
    {
        StartCoroutine(SwitchToDuelSceneInternal());
    }

    public void SwitchToMainMenuFromGame()
    {
        Destroy(innerDuelMan.gameObject);
        PopulateEarlList();
        SceneManager.LoadScene(mainMenuScene);
        Destroy(gameObject);
    }
    
    private IEnumerator SwitchToDuelSceneInternal()
    {
        // Start loading the scene
        AsyncOperation asyncLoadLevel = SceneManager.LoadSceneAsync(duelScene, LoadSceneMode.Single);
        // Wait until the level finish loading
        while (!asyncLoadLevel.isDone)
            yield return null;
        // Wait a frame so every Awake and Start method is called
        yield return new WaitForEndOfFrame();
        
        innerDuelMan = Instantiate(duelMan, Vector3.zero, Quaternion.identity);

        DuelManager man = innerDuelMan.GetComponent<DuelManager>();
        
        if (playerEarl != null)
            man.SetPlayerEarl(playerEarl);

        man.SetAIEarl(SelectAIEarl());
        man.GetReadyForDuel();
        man.mainMenuButton.onClick.AddListener(SwitchToMainMenuFromGame);
    }

    public static GameManager GetInstance()
    {
        return instance;
    }

    private void PopulateEarlList()
    {
        foreach (var earl in Earls)
        {
            internalEarls.Add(earl);
        }
    }

    public void SetPlayerEarl(BaseEarl selectedEarl)
    {
        playerEarl = selectedEarl;
        internalEarls.Remove(playerEarl);
    }

    private BaseEarl SelectAIEarl()
    {
        Random rnd = new Random();
        return internalEarls[rnd.Next(0, internalEarls.Count)];
    }
}