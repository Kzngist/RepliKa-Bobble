using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BobbleTrayObjectDictionary", menuName = "Scriptable Objects/Bobble Tray Object Dictionary", order = 0)]
public class BobbleTrayObjectDictionary : ScriptableObject
{
    [SerializeField] BobbleTrayObjectProfile[] profiles;
    
    Dictionary<BobbleTrayObjectVariant, BobbleTrayObjectProfile> dictionary;

    internal BobbleTrayObjectProfile this[BobbleTrayObjectVariant variant]
    {
        get
        {
            if (dictionary == null)
            {
                InitializeDictionary();
            }
            
            dictionary!.TryGetValue(variant, out BobbleTrayObjectProfile profile);
            return profile;
        }
    }

    void InitializeDictionary()
    {
        dictionary = new Dictionary<BobbleTrayObjectVariant, BobbleTrayObjectProfile>();
        
        foreach (BobbleTrayObjectProfile profile in profiles)
        {
            dictionary[profile.variant] = profile;
        }
    }
}