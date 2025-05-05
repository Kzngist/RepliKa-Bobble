using UnityEngine;

[CreateAssetMenu(fileName = "LevelProfile", menuName = "Scriptable Objects/Level Profile", order = 2)]
public class LevelProfile : ScriptableObject
{
    [SerializeField] internal string levelID;
    [SerializeField] internal string levelName;
    [SerializeField] internal string levelDescription;
    [SerializeField] internal LevelState levelState;
    [SerializeField] internal BobbleDictionary bobbleDictionary; // dictionary to retrieve bobble prefabs from
    [SerializeField] internal BobbleTrayObjectDictionary ceilingDictionary;
    // [SerializeField] internal Theme theme;
}