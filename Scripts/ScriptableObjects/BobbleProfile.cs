using UnityEngine;

[CreateAssetMenu(fileName = "BobbleProfile", menuName = "Scriptable Objects/Bobble Profile", order = 1)]
public class BobbleProfile : ScriptableObject
{
    [SerializeField] internal BobbleColour colour;
    [SerializeField] internal Bobble bobblePrefab;
    [SerializeField] internal TravellingBobble travellingBobblePrefab;
}