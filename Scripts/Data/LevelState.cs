using System;
using UnityEngine;

[Serializable]
public class LevelState
{
    [SerializeField] internal BobbleTrayState bobbleTrayState;
    [SerializeField] internal ArrowState arrowState;
    [SerializeField] internal int score;

    internal LevelState(BobbleTrayState bobbleTrayState, ArrowState arrowState, int score)
    {
        this.bobbleTrayState = bobbleTrayState;
        this.arrowState = arrowState;
        this.score = score;
    }
}
