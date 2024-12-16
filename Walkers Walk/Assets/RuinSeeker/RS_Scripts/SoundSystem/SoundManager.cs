using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum SoundType
{
    BUTTON,
    CHECKPOINT,
    GEMPICKUP,
    KILLINGENEMY,
    MINEENEMY,
    BOOMERANGSHOT,
    JUMP,
    LEVELCOMPLETED,
    MUSIC,
    WALLBUMP
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] soundList;
    [SerializeField] private Slider effectsVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;  
    private static SoundManager instance;
    public AudioSource effectsAudioSource;
    public AudioSource musicAudioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        
    }

    private void Start()
    {


        if (effectsVolumeSlider != null)
        {
            effectsVolumeSlider.onValueChanged.AddListener(SetEffectsVolume);
            effectsVolumeSlider.value = effectsAudioSource.volume; 
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            musicVolumeSlider.value = musicAudioSource.volume;
        }
    }

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        if (instance != null && instance.effectsAudioSource != null && instance.soundList != null)
        {
            instance.effectsAudioSource.PlayOneShot(instance.soundList[(int)sound], volume * instance.effectsAudioSource.volume);
        }
    }

    public static void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (instance != null && instance.musicAudioSource != null)
        {
            instance.musicAudioSource.clip = musicClip;
            instance.musicAudioSource.loop = loop;
            instance.musicAudioSource.Play();
        }
    }

    public void SetEffectsVolume(float volume)
    {

        effectsAudioSource.volume = volume;
    }

    public void SetMusicVolume(float volume)
    {
        musicAudioSource.volume = volume;
    }
}
