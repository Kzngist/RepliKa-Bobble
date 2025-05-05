using System;
using UnityEngine;

[Serializable]
public class BobbleTrayObjectState
{
    [SerializeField] internal BobbleTrayObjectType type;
    [SerializeField] internal Coordinates coordinates;
    [SerializeField] internal Direction orientation;
    [SerializeField] internal BobbleTrayObjectVariant variant;
    [SerializeField] internal BobbleColour colour;

    internal BobbleTrayObjectState(BobbleTrayObjectType type, Coordinates coordinates, Direction orientation = Direction.NE, BobbleTrayObjectVariant variant = BobbleTrayObjectVariant.Regular)
    {
        this.type = type;
        this.coordinates = coordinates;
        this.orientation = orientation;
        this.variant = variant;
    }

    internal BobbleTrayObjectState(Coordinates coordinates, BobbleColour colour = BobbleColour.Default)
    {
        this.type = BobbleTrayObjectType.Bobble;
        this.coordinates = coordinates;
        this.colour = colour;
    }
}
