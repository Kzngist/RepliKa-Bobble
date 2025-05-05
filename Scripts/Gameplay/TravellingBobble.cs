using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

[SelectionBase]
public class TravellingBobble : MonoBehaviour
{
    [SerializeField] internal BobbleColour colour;
    [SerializeField] float wallCollisionRadius = 0.5f;
    [SerializeField] float bobbleCollisionRadius = 0.2f;
    [SerializeField] internal float travellingSpeed = 32f; // travelling speed
    [SerializeField] float attachingSpeed = 3.2f;
    
    [Space]
    [Header("Audio")]
    [SerializeField] AudioClipProfile[] wallCollisionSFXs;
    [SerializeField] AudioClipProfile[] bobbleAttachmentSFXs;
    
    bool isMoving = false; // should the bobble move over time
    Vector3 travellingDirection;

    void FixedUpdate()
    {
        if (!isMoving) return;
        
        // check if bobble will collide with an attachable
        if (CheckAttachable()) return;

        // check if bouncing off wall, in which case the bobble is moved to the collision position
        if (CheckWallCollision()) return;
        
        // otherwise move bobble forward
        MoveForward();
    }
    
    void MoveForward()
    {
        transform.rotation = Quaternion.LookRotation(travellingDirection);
        transform.position += travellingSpeed * travellingDirection * Time.fixedDeltaTime;
        
        DebugWorld.Instance.ObjectAtPosition(transform.position, transform.rotation, DebugObjectType.Stick);
    }
    
    
    /// <summary>
    /// Check if bobble will collide with an attachable this frame, if so, attach the bobble
    /// </summary>
    bool CheckAttachable()
    {
        float deltaPositionPerFrame = travellingSpeed * Time.fixedDeltaTime;
        if (Physics.SphereCast(transform.position, bobbleCollisionRadius, travellingDirection, out RaycastHit hit, wallCollisionRadius - bobbleCollisionRadius + deltaPositionPerFrame, Layer.BobbleTrayAttachableMask))
        {
            BobbleTray tray = hit.transform.GetComponentInParent<BobbleTray>();
            if (tray)
            {
                AudioManager.Instance.PlaySound(bobbleAttachmentSFXs, null, hit.point);
                
                StartCoroutine(SpawnBobbleCoroutine(tray));
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if bobble will collide with a wall this frame, if so, reflect travellingDirection and move the bobble to the collision position
    /// </summary>
    bool CheckWallCollision()
    {
        float deltaPositionPerFrame = travellingSpeed * Time.fixedDeltaTime;
        if (Physics.SphereCast(transform.position, wallCollisionRadius, travellingDirection, out RaycastHit hit, deltaPositionPerFrame, Layer.WallReflectionMask))
        {
            AudioManager.Instance.PlaySound(wallCollisionSFXs, null, hit.point);

            transform.position = hit.point + hit.normal * wallCollisionRadius;
            travellingDirection = Vector3.Reflect(travellingDirection, hit.normal);
            
            return true;
        }

        return false;
    }
    
    /// <summary>
    ///   <para> Release bobble </para>
    /// </summary>
    internal void Weeeeee(Vector3 direction)
    {
        transform.SetParent(null);
        isMoving = true;
        travellingDirection = direction;
    }

    IEnumerator SpawnBobbleCoroutine(BobbleTray tray)
    {
        DebugWorld.Instance.DrawSphere(transform.position, .2f);

        isMoving = false;
        
        // parent transform to bobble container so it follows any ongoing movement of the container
        transform.SetParent(tray.attachableContainer);
        // get the nearest attachable position in container and move towards it
        Coordinates targetCoordinates = tray.GetClosestCoordinates(transform.position);
        Vector3 targetPositionInContainer = targetCoordinates.GetPosition();
        yield return transform.DOLocalMove(targetPositionInContainer, attachingSpeed).SetSpeedBased().WaitForCompletion();
        // yield return rb.DOMove(targetPositionInContainer, attachingSpeed).SetSpeedBased().WaitForCompletion();

        tray.SpawnBobbleAtCoordinates(colour, targetCoordinates, true);
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        DebugWorld.Instance.DrawSphere(collision.GetContact(0).point + collision.GetContact(0).normal * wallCollisionRadius, .2f);
    }
}
