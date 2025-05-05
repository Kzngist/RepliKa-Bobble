using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData_LevelStatuses
{
    [SerializeField] internal List<LevelStatus> levelStatuses = new();
    [SerializeField] internal int arcadeHighScore;
}
