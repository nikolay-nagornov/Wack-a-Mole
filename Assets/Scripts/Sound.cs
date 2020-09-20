using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;

    [SerializeField] private AudioClip clip;
    private AudioSource source;
    private AudioMixer mixer;

    [Range(0, 1f)] [SerializeField] private float volume = 1f;
    [Range(0.1f, 3f)] [SerializeField] private float pitch = 1f;

    public bool loop;
    public bool playOnAwake;

    public void Init(AudioSource source, AudioMixerGroup mixerGroup)
    {
        this.source = source;
        source.outputAudioMixerGroup = mixerGroup;
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.playOnAwake = playOnAwake;
    }

    public void Play(bool randomVolume)
    {
        if (source == null)
            return;

        if(randomVolume)
            source.volume = Random.Range(volume - 0.3f, volume);
        source.Play();
    }
}
