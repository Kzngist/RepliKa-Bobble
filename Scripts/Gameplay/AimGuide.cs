using System.Collections.Generic;
using UnityEngine;

public class AimGuide : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] [Range(0, 1f)] float lineWidth = 0.2f;
    [SerializeField] int maxBounces = 16; // max wall bounces the line can render
    [SerializeField] float maxDistance = 64f; // max distance the line can render
    [SerializeField] float fadeDistance = 24f; // how much distance to fade out on end of the line
    
    float wallCollisionRadius = 0.5f;
    float bobbleCollisionRadius = 0.2f;
    
    List<Vector3> lineRendererPoints = new();

    internal void DrawAimGuide(Vector3 origin, Vector3 direction)
    {
        // draw line
        lineRendererPoints.Clear();
        lineRendererPoints.Add(origin);
        float distanceLeft = CalculateAimGuidePoints(origin, direction, maxBounces, maxDistance);
        lineRenderer.positionCount = lineRendererPoints.Count;
        lineRenderer.SetPositions(lineRendererPoints.ToArray());
        
        // set renderer and material properties
        lineRenderer.widthMultiplier = lineWidth;
        lineRenderer.textureScale = new Vector2(1f / lineWidth, 1);
        float distanceTravelled = maxDistance - distanceLeft;
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetFloat("_Length", distanceTravelled / lineWidth);
        materialPropertyBlock.SetFloat("_EndFadeLength", fadeDistance / lineWidth);
        lineRenderer.SetPropertyBlock(materialPropertyBlock);
    }
    
    /// <summary>
    /// Recursively calculates collision points of given trajectory and store them in lineRendererPoints list
    /// </summary>
    /// <returns> how much distance is left </returns>
    float CalculateAimGuidePoints(Vector3 origin, Vector3 direction, int bouncesRemaining, float distanceRemaining)
    {
        // return if out of bounces
        if (bouncesRemaining < 0) return distanceRemaining;

        // if an attachable is in front, record the contact point
        Vector3 attachableCollisionPoint = Vector3.zero;
        float attachableCollisionDistance = 0;
        if (Physics.SphereCast(origin, bobbleCollisionRadius, direction, out RaycastHit attachableHit, distanceRemaining, Layer.BobbleTrayAttachableMask))
        {
            Vector3 hitPosition = attachableHit.point + attachableHit.normal * bobbleCollisionRadius;
            attachableCollisionPoint = hitPosition;
            attachableCollisionDistance = attachableHit.distance;
        }

        // if a wall is in front, if there's no attachable in front, or the attachable is further away, add the contact position and recursively calculate collisions after bouncing;
        // otherwise add the attachable collision point and return distance left.
        if (Physics.SphereCast(origin, wallCollisionRadius, direction, out RaycastHit wallHit, distanceRemaining, Layer.WallReflectionMask))
        {
            Vector3 collisionPoint = wallHit.point + wallHit.normal * wallCollisionRadius;
            
            if (attachableCollisionDistance == 0 || attachableCollisionDistance > wallHit.distance)
            {
                lineRendererPoints.Add(collisionPoint);
            }
            else
            {
                lineRendererPoints.Add(attachableCollisionPoint);
                distanceRemaining -= attachableCollisionDistance;
                return distanceRemaining;
            }

            direction = Vector3.Reflect(direction, wallHit.normal);
            bouncesRemaining--;
            distanceRemaining -= wallHit.distance;
            return CalculateAimGuidePoints(collisionPoint, direction, bouncesRemaining, distanceRemaining);
        }

        // if an attachable is in front, and there's no wall in front, add the attachable collision point and return distance left.
        if (attachableCollisionDistance != 0)
        {
            lineRendererPoints.Add(attachableCollisionPoint);
            distanceRemaining -= attachableCollisionDistance;
            return distanceRemaining;
        }
        
        // if nothing is hit, add the farthest position reached
        lineRendererPoints.Add(origin + distanceRemaining * direction);
        return 0;
    }

    internal void Clear()
    {
        lineRenderer.positionCount = 0;
    }

    internal void ToggleLineRenderer(bool toggle)
    {
        lineRenderer.enabled = toggle;
    }
}
