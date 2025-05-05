using System;
using UnityEngine;

[Serializable]
public class ArrowState
{
    [SerializeField] internal BobbleColour currentBobbleColour;
    [SerializeField] internal BobbleColour nextBobbleColour;
    [SerializeField] internal int shotsLeft;

    internal ArrowState(BobbleColour currentBobbleColour, BobbleColour nextBobbleColour, int shotsLeft)
    {
        this.currentBobbleColour = currentBobbleColour;
        this.nextBobbleColour = nextBobbleColour;
        this.shotsLeft = shotsLeft;
    }
}