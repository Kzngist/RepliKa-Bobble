using UnityEngine;

[SelectionBase]
public class LevelPainterSelectable : MonoBehaviour
{
    [SerializeField] internal BobbleTrayObjectType type = BobbleTrayObjectType.Bobble;
    [SerializeField] internal BobbleColour bobbleColour = BobbleColour.Default;
    [SerializeField] internal BobbleTrayObjectVariant objectVariant = BobbleTrayObjectVariant.Regular;
}
