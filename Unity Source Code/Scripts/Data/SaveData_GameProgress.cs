using UnityEngine;

[System.Serializable]
public class SaveData_GameProgress
{
    [SerializeField] internal int randomSeed;
    [SerializeField] internal Random.State randomState;

    [SerializeField] internal int score;
    [SerializeField] internal LevelState levelState;
    // [SerializeField] internal ArrowState arrowState;
}
