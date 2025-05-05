using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public enum AudioType
{
    Master,
    SFX,
    BGM
}

public class AudioManager : MonoBehaviour
{
    internal static AudioManager Instance;

    [SerializeField] internal AudioMixer audioMixer;
    [SerializeField] AudioSource audioSourceMusic;
    [SerializeField] AudioSource audioSourceSFXPrefab;

    Coroutine musicLoopCoroutine;
    AudioClipProfile[] currentMusicClips;
    internal bool doPlayMusic = true;
    
    [Space]
    [Header("Defaults")]
    [SerializeField] internal AudioClipProfile UISelectSFX;
    [SerializeField] internal AudioClipProfile UIClickSFX;

    void Awake()
    {
        if (Instance != null)
        {
            DebugConsole.Instance.Log("<AudioManager> already exists!", LogType.Warning);
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    internal void PlaySound(AudioClipProfile[] clipProfiles, Transform parent = null, Vector3 position = default, float pitchMultiplier = 0.0f)
    {
        AudioClipProfile clipProfile = clipProfiles[Random.Range(0, clipProfiles.Length)];
        PlaySound(clipProfile, parent, position, pitchMultiplier);
    }
    
    internal void PlaySound(AudioClipProfile clipProfile, Transform parent = null, Vector3 position = default, float pitchMultiplier = 0.0f)
    {
        AudioSource audioSource = Instantiate(audioSourceSFXPrefab, parent, false);
        audioSource.transform.localPosition = position;
        audioSource.clip = clipProfile.clip;
        audioSource.volume = clipProfile.volume;
        audioSource.pitch = clipProfile.pitch * (pitchMultiplier == 0.0f ? 1 + Random.Range(-clipProfile.pitchRandomness, clipProfile.pitchRandomness) : pitchMultiplier);
        audioSource.spatialBlend = clipProfile.spatialBlend;
        audioSource.loop = clipProfile.doLoop;
        audioSource.Play();
        
        float audioLength = audioSource.clip.length / Mathf.Abs(audioSource.pitch);
        if (parent == null) DontDestroyOnLoad(audioSource);
        StartCoroutine(DestroyObjectRealTime(audioSource.gameObject, audioLength));
    }

    internal void PlayBGM(AudioClipProfile[] musicClips)
    {
        // stop music if new clip is null
        if (musicClips == null)
        {
            audioSourceMusic.Stop();
            if (musicLoopCoroutine != null)
            {
                StopCoroutine(musicLoopCoroutine);
            }
            return;
        }
        
        // check if new clips are identical to current clips, if so ignore new clips
        if (currentMusicClips != null && musicClips.SequenceEqual(currentMusicClips)) return;
        
        if (musicLoopCoroutine != null)
        {
            StopCoroutine(musicLoopCoroutine);
        }
        
        currentMusicClips = musicClips;
        musicLoopCoroutine = StartCoroutine(MusicLoop());
    }
    
    internal void PlayBGM(AudioClipProfile musicClip)
    {
        PlayBGM(new[] {musicClip});
    }

    IEnumerator MusicLoop()
    {
        int index = Random.Range(0, currentMusicClips.Length);

        while (doPlayMusic)
        {
            AudioClipProfile clipProfile = currentMusicClips[index];

            audioSourceMusic.clip = clipProfile.clip;
            audioSourceMusic.volume = clipProfile.volume;
            audioSourceMusic.pitch = clipProfile.pitch + Random.Range(-clipProfile.pitchRandomness, clipProfile.pitchRandomness);
            audioSourceMusic.spatialBlend = clipProfile.spatialBlend;
            audioSourceMusic.loop = clipProfile.doLoop;
            audioSourceMusic.Play();
            
            float audioLength = audioSourceMusic.clip.length / Mathf.Abs(audioSourceMusic.pitch);
            yield return new WaitUntil(() => audioSourceMusic.time >= audioLength);
            index = (index + Random.Range(1, currentMusicClips.Length)).Mod(currentMusicClips.Length);
        }
    }
    
    internal void SetVolume(AudioType type, float value)
    {
        // converts scale of [1, 100] to logarithmic scale on the mixer, mute if 0
        float volume = value < 1 ? -80 : Mathf.Log10(value) * 50 - 80;

        switch (type)
        {
            case AudioType.Master:
                audioMixer.SetFloat("MasterVolume", volume);
                break;
            case AudioType.SFX:
                audioMixer.SetFloat("SFXVolume", volume);
                break;
            case AudioType.BGM:
                audioMixer.SetFloat("BGMVolume", volume);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    IEnumerator DestroyObjectRealTime(Object obj, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        Destroy(obj);
    }
}