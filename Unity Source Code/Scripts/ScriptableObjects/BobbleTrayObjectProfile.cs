using UnityEngine;

public enum BobbleTrayObjectVariant
{
    Regular,
    Half
}

[CreateAssetMenu(fileName = "BobbleTrayObjectProfile", menuName = "Scriptable Objects/Bobble Tray Object Profile", order = 2)]
public class BobbleTrayObjectProfile : ScriptableObject
{
    [SerializeField] internal BobbleTrayObjectVariant variant;
    [SerializeField] internal BobbleTrayObject objectPrefab;
}