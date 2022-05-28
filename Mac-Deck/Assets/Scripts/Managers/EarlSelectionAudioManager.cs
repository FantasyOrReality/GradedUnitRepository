using UnityEngine;

public class EarlSelectionAudioManager : MonoBehaviour
{
    private AudioSource previousSound;

    public void PlaySoundAndStopPrevious(AudioSource soundToPlay)
    {
        if (previousSound)
            previousSound.Stop();

        previousSound = soundToPlay;
        previousSound.Play();
    }
}
