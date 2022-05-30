using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject duelMan;
    [SerializeField] private string mainMenuScene;
    [SerializeField] private string duelScene;
    [SerializeField] private BaseEarl playerEarl;
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
        Destroy(duelMan.gameObject);
        PopulateEarlList();
        SceneManager.LoadScene(mainMenuScene);
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
        
        duelMan = Instantiate(duelMan, Vector3.zero, Quaternion.identity);

        DuelManager man = duelMan.GetComponent<DuelManager>();
        
        if (playerEarl)
            man.SetPlayerEarl(playerEarl);
        else
        {
            man.SetPlayerEarl(internalEarls[0]);
            internalEarls.RemoveAt(0);
        }
        
        man.SetAIEarl(SelectAIEarl());
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