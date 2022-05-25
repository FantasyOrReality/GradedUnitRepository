using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{
    public void Quit()
    {
        Application.Quit();
    }

    public void EarlSelection()
    {
        SceneManager.LoadScene("EarlSelection");
    }

    public void SettingsScreen()
    {
        SceneManager.LoadScene("Settings");
    }

    public void SwitchToEarlInformation()
    {
        SceneManager.LoadScene("EarlInformation");
    }
}
