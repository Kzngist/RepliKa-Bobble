using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BobbleTrayState
{
    [SerializeField] internal List<BobbleColour> spawnableBobbleColours; // list of bobble colours that can spawn in this tray
    [SerializeField] internal int trayWidth = 9; // how distant apart bobble tray walls are
    [SerializeField] internal int ceilingDropInterval = 6; // how many bobbles need to be attached before ceiling drops by one level
    [SerializeField] internal float ceilingDropAmount = 1f; // how many bobble length to drop each time
    
    [SerializeField] internal Vector3 trayPosition;
    [SerializeField] internal List<BobbleTrayObjectState> objectStates;
    [SerializeField] internal int ceilingDropCounter;
    

    internal BobbleTrayState(List<BobbleColour> spawnableBobbleColours, int trayWidth, int ceilingDropInterval, float ceilingDropAmount,
        Vector3 trayPosition, List<BobbleTrayObjectState> objectStates, int ceilingDropCounter)
    {
        this.spawnableBobbleColours = spawnableBobbleColours;
        this.trayWidth = trayWidth;
        this.ceilingDropInterval = ceilingDropInterval;
        this.ceilingDropAmount = ceilingDropAmount;
        
        this.trayPosition = trayPosition;
        this.objectStates = objectStates;
        this.ceilingDropCounter = ceilingDropCounter;
    }
}
