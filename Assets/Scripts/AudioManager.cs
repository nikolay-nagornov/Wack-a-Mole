using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    [Header("Music")]
    [Tooltip("Time in seconds")] [SerializeField] private float musicFadeTime = 5f;
    [SerializeField] private AudioClip[] musicClips;
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    private AudioSource musicAudioSource;

    private int curMusicIndex;

    [Header("Sounds")]
    [SerializeField] private Sound[] soundClips;
    [SerializeField] private AudioMixerGroup soundMixerGroup;

    public static AudioManager instance; 

    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        else
            instance = this;

        musicAudioSource = Instantiate(new GameObject("MusicSource"), transform).AddComponent<AudioSource>();
        musicAudioSource.outputAudioMixerGroup = musicMixerGroup;

        audioMixer.updateMode = AudioMixerUpdateMode.UnscaledTime;

        for (int i = 0; i < soundClips.Length; i++)
        {
            AudioSource newSource = Instantiate(new GameObject("SoundSource_" + soundClips[i].name), transform).AddComponent<AudioSource>();
            soundClips[i].Init(newSource, soundMixerGroup);
        }

        PlayRandomMusic();
    }

    //private void Start()
    //{
    //    DontDestroyOnLoad(gameObject);
    //}

    public void PlaySound(string name)
    {
        Array.Find(soundClips, sound => sound.name == name).Play(false);
    }

    public void PlaySound(string name, bool randomVolume)
    {
        Array.Find(soundClips, sound => sound.name == name).Play(randomVolume);
    }

    public void PlayMusic(string name)
    {
        musicAudioSource.volume = 0;

        for (int i = 0; i < musicClips.Length; i++)
        {
            if (name != musicClips[i].name)
                continue;

            curMusicIndex = i;
            musicAudioSource.clip = musicClips[i];
        }

        musicAudioSource.Play();
        StartCoroutine(StartFade(musicFadeTime, 1f));
        StartCoroutine(PlayNextMusic(musicAudioSource.clip.length - musicFadeTime));
    }

    private void PlayMusic()
    {
        musicAudioSource.volume = 0;
        musicAudioSource.Play();
        StartCoroutine(StartFade(musicFadeTime, 1f));
        StartCoroutine(PlayNextMusic(musicAudioSource.clip.length - musicFadeTime));
    }

    public IEnumerator PlayNextMusic(float delay)
    {
        if (musicClips.Length == 0)
            yield break;

        yield return new WaitForSecondsRealtime(delay);

        StartCoroutine(StartFade(musicFadeTime, 0f));
        yield return new WaitForSecondsRealtime(musicFadeTime);

        curMusicIndex++;
        if (curMusicIndex >= musicClips.Length)
            curMusicIndex = 0;
        musicAudioSource.clip = musicClips[curMusicIndex];

        PlayMusic();
    }

    public void PlayRandomMusic()
    {
        if (musicClips.Length < 2)
        {
            StartCoroutine(PlayNextMusic(0));
            return;
        }

        int newIndex = curMusicIndex;
        int musicCount = musicClips.Length;

        while(newIndex == curMusicIndex)
        {
            curMusicIndex = UnityEngine.Random.Range(0, musicCount);
        }

        if (curMusicIndex >= musicClips.Length)
            curMusicIndex = 0;

        StartCoroutine(PlayNextMusic(0));
    }

    public IEnumerator StartFade(float duration, float targetVolume)
    {
        float currentTime = 0;
        float currentVol = musicAudioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime;
            musicAudioSource.volume = Mathf.Lerp(currentVol, targetVolume, currentTime / duration);
            yield return null;
        }
    }

    public IEnumerator StartFade(string exposedParam, float duration, float targetVolume)
    {
        float currentTime = 0;
        float currentVol;

        audioMixer.GetFloat(exposedParam, out currentVol);
        currentVol = Mathf.Pow(10f, currentVol / 20f);
        float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1f);

        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime;
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            audioMixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20f);
            yield return null;
        }
    }
}
