using UnityEngine;

public enum LevelClearedStatus
{
    Locked,
    Unlocked,
    Cleared,
    Mastered
}

[System.Serializable]
public class LevelStatus
{
    [SerializeField] internal string levelID;
    [SerializeField] internal LevelClearedStatus clearedStatus = LevelClearedStatus.Locked;
    [SerializeField] internal int highScore = 0;
}
