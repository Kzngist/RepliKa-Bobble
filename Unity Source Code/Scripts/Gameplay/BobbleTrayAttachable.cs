using System;
using UnityEngine;

[SelectionBase]
public abstract class BobbleTrayAttachable : BobbleTrayObject
{
    protected BobbleTrayAttachable[] neighbours = new BobbleTrayAttachable[6];
    
    internal bool hasVisited; // flag used in recursive calls
    Collider[] neighbourCheckCache = new Collider[1];

    internal override void SetUp()
    {
        base.SetUp();
        
        UpdateNeighbours();
    }
    
    internal BobbleTrayAttachable GetNeighbour(Direction direction)
    {
        return neighbours[(int)direction];
    }
    
    internal BobbleTrayAttachable[] GetNeighbours()
    {
        return neighbours;
    }
    
    /// <summary>
    ///   <para> Sets neighbour at direction </para>
    /// </summary>
    internal void SetNeighbour(Direction direction, BobbleTrayAttachable neighbour)
    {
        neighbours[(int)direction] = neighbour;

        if (neighbour)
        {
            neighbour.neighbours[(int)direction.Opposite()] = this;
        }
    }
    
    /// <summary>
    ///   <para> Checks and updates all neighbours of this object, also updates neighbour </para>
    /// </summary>
    void UpdateNeighbours()
    {
        foreach (Direction direction in Enum.GetValues(typeof(Direction)))
        {
            if (Physics.OverlapSphereNonAlloc(GetNeighbourWorldPosition(direction), 0.3f, neighbourCheckCache, Layer.BobbleTrayAttachableMask) == 0)
            {
                SetNeighbour(direction, null);
            }
            else
            {
                BobbleTrayAttachable neighbour = neighbourCheckCache[0].GetComponentInParent<BobbleTrayAttachable>();
                if (neighbour)
                {
                    if (neighbour != this)
                    {
                        SetNeighbour(direction, neighbour);
                    }
                    else
                    {
                        DebugConsole.Instance.Log($"{direction} of {coordinates} is self!", LogType.Warning);
                    }
                }
                else
                {
                    DebugConsole.Instance.Log($"{direction} of {coordinates} is missing component <BobBobbleTrayAttachable>", LogType.Warning);
                }
            }
        }
    }

    /// <summary>
    /// Removes references from all neighbours
    /// </summary>
    internal void RemoveNeighbourReferences()
    {
        foreach (Direction neighbourDirection in Enum.GetValues(typeof(Direction)))
        {
            BobbleTrayAttachable neighbour = GetNeighbour(neighbourDirection);
            
            if (neighbour != null)
            {
                neighbour.SetNeighbour(neighbourDirection.Opposite(), null);
            }
        }
    }

    /// <summary>
    /// Returns world position of neighbour in given direction
    /// </summary>
    internal Vector3 GetNeighbourWorldPosition(Direction direction)
    {
        return transform.TransformPoint(Coordinates.FromDirection(direction).GetPosition());
    }
    
        
    /// <summary>
    /// Recursively flag all connected bobbles, effectively flags all attachables of an island
    /// </summary>
    internal void FlagAllConnected()
    {
        hasVisited = true;

        foreach (BobbleTrayAttachable neighbour in neighbours)
        {
            if (neighbour == null || neighbour.hasVisited) continue;

            neighbour.FlagAllConnected();
        }
    }
}
