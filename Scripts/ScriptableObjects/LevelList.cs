using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelList", menuName = "Scriptable Objects/Level List", order = 3)]
public class LevelList : ScriptableObject
{
    [SerializeField] internal List<LevelProfile> levels;
    
    internal LevelProfile GetNext(LevelProfile current)
    {
        int currentLevelIndex = levels.IndexOf(current);
        
        return currentLevelIndex == levels.Count - 1 ? null : levels[currentLevelIndex + 1];
    }
    
    internal LevelProfile GetPrev(LevelProfile current)
    {
        int currentLevelIndex = levels.IndexOf(current);
        
        return currentLevelIndex == 0 ? null : levels[currentLevelIndex - 1];
    }
}