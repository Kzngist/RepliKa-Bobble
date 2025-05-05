using UnityEngine;

public class Layer : MonoBehaviour
{
    public static int UIMask { get; private set; }
    public static int BobbleTrayAttachableMask { get; private set; }
    public static int DeathZoneMask { get; private set; }
    public static int RaycastReceiverMask { get; private set; }
    public static int BaseLineMask { get; private set; }
    
    public static int WallReflectionMask { get; private set; }
    public static int TravellingBobbleCollisionMask { get; private set; }

    void Awake()
    {
        UIMask = LayerMask.NameToLayer("UI");
        BobbleTrayAttachableMask = 1 << LayerMask.NameToLayer("BobbleTrayAttachable");
        DeathZoneMask = 1 << LayerMask.NameToLayer("DeathZone");
        RaycastReceiverMask = 1 << LayerMask.NameToLayer("RaycastReceiver");
        BaseLineMask = 1 << LayerMask.NameToLayer("BaseLine");

        WallReflectionMask = ~(BobbleTrayAttachableMask);
        TravellingBobbleCollisionMask = BobbleTrayAttachableMask | WallReflectionMask;
    }
}
