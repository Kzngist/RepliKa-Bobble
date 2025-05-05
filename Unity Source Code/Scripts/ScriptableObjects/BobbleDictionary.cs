using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BobbleDictionary", menuName = "Scriptable Objects/Bobble Dictionary", order = 0)]
public class BobbleDictionary : ScriptableObject
{
    [SerializeField] BobbleProfile[] bobbleProfiles;
    
    Dictionary<BobbleColour, BobbleProfile> bobbleDictionary;

    internal BobbleProfile this[BobbleColour colour]
    {
        get
        {
            if (bobbleDictionary == null)
            {
                InitializeDictionary();
            }
            
            bobbleDictionary!.TryGetValue(colour, out BobbleProfile bobbleProfile);
            return bobbleProfile;
        }
    }

    void InitializeDictionary()
    {
        bobbleDictionary = new Dictionary<BobbleColour, BobbleProfile>();
        
        foreach (BobbleProfile bobbleProfile in bobbleProfiles)
        {
            bobbleDictionary[bobbleProfile.colour] = bobbleProfile;
        }
    }
}