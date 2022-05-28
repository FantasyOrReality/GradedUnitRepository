using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class StartOfDuelPlayer : MonoBehaviour
{
    [Header("Sounds")] 
    [SerializeField] private List<AudioClip> DuncanMacbethVoices;
    [SerializeField] private List<AudioClip> DuncanSiwardVoices;
    [SerializeField] private List<AudioClip> DuncanTwinsVoices;
    [SerializeField] private List<AudioClip> MacbethDuncanVoices;
    [SerializeField] private List<AudioClip> MacbethSiwardVoices;
    [SerializeField] private List<AudioClip> MacbethTwinsVoices;
    [SerializeField] private List<AudioClip> SiwardMacbethVoices;
    [SerializeField] private List<AudioClip> SiwardDucanVoices;
    [SerializeField] private List<AudioClip> SiwardTwinsVoices;
    [SerializeField] private List<AudioClip> TwinsSiwardVoices;
    [SerializeField] private List<AudioClip> TwinsMacbethVoices;
    [SerializeField] private List<AudioClip> TwinsDucanVoices;
    
    [SerializeField] private AudioSource audioSource;

    public void PlayStartDialogue(string playerEarlName, string AIEarlName, Random rnd)
    {
        switch (playerEarlName)
        {
            case "Duncan" when AIEarlName == "Macbeth":
                audioSource.clip = DuncanMacbethVoices[rnd.Next(0, DuncanMacbethVoices.Count)];
                break;
            case "Duncan" when AIEarlName == "Siward":
                audioSource.clip = DuncanSiwardVoices[rnd.Next(0, DuncanSiwardVoices.Count)];
                break;
            case "Duncan":
                audioSource.clip = DuncanTwinsVoices[rnd.Next(0, DuncanTwinsVoices.Count)];
                break;
            case "Siward" when AIEarlName == "Macbeth":
                audioSource.clip = SiwardMacbethVoices[rnd.Next(0, SiwardMacbethVoices.Count)];
                break;
            case "Siward" when AIEarlName == "Duncan":
                audioSource.clip = SiwardDucanVoices[rnd.Next(0, SiwardDucanVoices.Count)];
                break;
            case "Siward":
                audioSource.clip = SiwardTwinsVoices[rnd.Next(0, SiwardTwinsVoices.Count)];
                break;
            case "Macbeth" when AIEarlName == "Siward":
                audioSource.clip = MacbethSiwardVoices[rnd.Next(0, MacbethSiwardVoices.Count)];
                break;
            case "Macbeth" when AIEarlName == "Duncan":
                audioSource.clip = MacbethDuncanVoices[rnd.Next(0, MacbethDuncanVoices.Count)];
                break;
            case "Macbeth":
                audioSource.clip = MacbethTwinsVoices[rnd.Next(0, MacbethTwinsVoices.Count)];
                break;
            default:
            {
                if (AIEarlName == "Siward")
                    audioSource.clip = TwinsSiwardVoices[rnd.Next(0, TwinsSiwardVoices.Count)];
                else if (AIEarlName == "Duncan")
                    audioSource.clip = TwinsDucanVoices[rnd.Next(0, TwinsDucanVoices.Count)];
                else
                    audioSource.clip = TwinsMacbethVoices[rnd.Next(0, TwinsMacbethVoices.Count)];
                break;
            }
        }
        
        audioSource.Play();
    }
}
