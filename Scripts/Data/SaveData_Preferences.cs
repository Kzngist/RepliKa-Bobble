using UnityEngine;

public enum GameSpeed
{
    Slower,
    Slow,
    Normal,
    Fast,
    Faster
}

public enum GraphicQuality
{
    Low = 0,
    Medium = 1,
    High = 2
}

[System.Serializable]
public class SaveData_Preferences
{
    [SerializeField] [Range(0, 4)] internal GameSpeed gameSpeed = GameSpeed.Normal;
    [SerializeField] [Range(0, 2)] internal GraphicQuality graphicQuality = GraphicQuality.Medium;
    [SerializeField] [Range(0, 100)] internal float BGMVolume = 50f;
    [SerializeField] [Range(0, 100)] internal float SFXVolume = 50f;
    [SerializeField] internal bool doHighRefreshRate = false;
    [SerializeField] internal bool doShowAimButtons = true;
    [SerializeField] [Range(10, 100)] internal float sensitivity = 50f;
}
