using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    static AudioController instance;
    public static AudioController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(AudioController)) as AudioController;
                if (instance == null)
                {
                    GameObject obj = new GameObject("AudioController");
                    instance = obj.AddComponent<AudioController>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }
    public AudioClip StartAndRestartButtonSFX;
    public AudioClip IntroSFX;
    public AudioClip ButtonSFX1;
    public AudioClip ButtonSFX2;
    public AudioClip RightSFX;
    public AudioClip WrongSFX;
    public AudioClip WinSFX;
    public AudioClip LoseSFX;
    public AudioClip NewQuestSFX;
    public AudioSource BackgourndFXAudioSource;
    public AudioSource EventSFXAudioSource;
    public AudioSource ButtonSFXAudioSource;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayEventSFX( AudioClip newClip)
    {
        EventSFXAudioSource.clip = newClip;
        EventSFXAudioSource.Play();
    }

    public void PlayButtonSFX(AudioClip newClip)
    {
        ButtonSFXAudioSource.clip = newClip;
        ButtonSFXAudioSource.Play();
    }

    public void FadeOut( AudioSource a, float duration)
    {
         StartCoroutine(FadeOutCore(a, duration));
    }

    private static IEnumerator FadeOutCore(AudioSource a, float duration)
    {
        float startVolume = a.volume;

        while (a.volume > 0)
        {
            a.volume -= startVolume * Time.deltaTime / duration;
            yield return new WaitForEndOfFrame();
        }

        a.Stop();
        a.volume = startVolume;


    }
}
