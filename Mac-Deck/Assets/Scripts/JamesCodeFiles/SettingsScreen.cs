using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsScreen : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private void Start()
    {
        slider.value = AudioListener.volume;
    }

    public void SwitchToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    public void AdjustVolume()
    {
        AudioListener.volume = slider.value;
    }
}
