using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// A name generator for the basic cards
/// </summary>
[Serializable]
public class SNameGenerator : MonoBehaviour
{
    [SerializeField] List<string> firstNames = new List<string>
    {
        "William",
        "John",
        "Richard",
        "Robert",
        "Henry",
        "Ralph",
        "Thomas",
        "Walter",
        "Roger",
        "Hugh",
        "Ian",
        "James",
        "Denis",
        "Jenni"
    };
    [SerializeField] List<string> secondNames = new List<string>
    {
        "Ashdown",
        "Baker",
        "Bigge",
        "Brickenden",
        "Brooker",
        "Browne",
        "Clarke",
        "Cheeseman",
        "Godfrey",
        "Payne",
        "Ward",
        "Wood",
        "Webb"
    };

    private List<string> generatedNames = new List<string>();
    
    static SNameGenerator instance;

    private int nameIndex = 0;

    public static SNameGenerator GetInstance()
    {
        return instance;
    }
    
    private void Awake()
    {
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

        GenerateRandomNames();
    }
    
    void GenerateRandomNames()
    {
        Random rand = new Random();
        
        for (int i = 0; i < 40; i++)
        {
            generatedNames.Add($"{firstNames[rand.Next(0, firstNames.Count)]} {secondNames[rand.Next(0, secondNames.Count)]}");
        }
    }
    
    public string GetRandomName()
    {
        nameIndex++;
        return generatedNames[nameIndex - 1];
    }
}
