using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] internal AudioClipProfile[] musicClips;

    void Start()
    {
        AudioManager.Instance.PlayBGM(musicClips);
    }
}
