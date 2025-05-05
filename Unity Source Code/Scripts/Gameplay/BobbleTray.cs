using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class BobbleTray : MonoBehaviour
{
    [Header("Bobble Tray")]
    [SerializeField] internal Transform attachableContainer;
    [SerializeField] internal Transform leftWallContainer;
    [SerializeField] internal Transform rightWallContainer;
    [SerializeField] internal Transform deathZoneContainer;
    
    [Space]
    [Header("Behaviour")]
    [SerializeField] internal List<BobbleColour> spawnableBobbleColours = new(); // list of bobble colours that can spawn
    [SerializeField] internal int trayWidth = 9; // how distant apart bobble tray walls are
    [SerializeField] internal int ceilingDropInterval = 6; // how many bobbles need to be attached before ceiling drops by one level
    [SerializeField] internal float ceilingDropAmount = 1f; // how many bobble length to drop each time
    [SerializeField] internal int ceilingDropCounter = 6; // tracks next ceiling drop

    [Space]
    [Header("Shared Behaviour")]
    [SerializeField] [Range(0f, 8f)] float minVisibleBobbleRows = 2f; // bobble tray will drop if less than this amount of rows are visible
    [SerializeField] [Range(0f, 1f)] float bobbleSpawnInterval = 0.05f; // how long to wait in between each bobble generation
    [SerializeField] [Range(0f, 0.1f)] float looseBobbleDroppingInterval = 0.05f;
    [SerializeField] [Range(0f, 1f)] float looseBobbleDroppingIntervalRandomness = 0.00f;
    [SerializeField] [Range(0, 1)] float ceilingDropSpeed = 0.5f; // how fast the ceiling drops
    [SerializeField] [Range(0, 0.1f)] float ceilingShakeIntensity = 0.05f;
    [SerializeField] [Range(10, 100)] int ceilingShakeVibrato = 50;
    [SerializeField] AnimationCurve ceilingDropCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Space]
    [Header("Level Generator")]
    [SerializeField] bool doGenerateLevel = false; // should the level be procedurally generated
    [SerializeField] [Range(0, 10)] int generationRadius = 4;
    [SerializeField] [Range(0, 32)] int generationDepth = 7; // depth of lowest bobble
    [SerializeField] [Range(0f, 1f)] float emptyChance = 0.2f; // chance for a bobble slot to be vacant

    List<BobbleTrayAttachable> allAttachables = new(); // all attachables in tray, including bobbles, ceilingTiles
    
    List<Bobble> matchingBobbles = new(); // bobbles that has a matching connection to current bobble
    readonly Collider[] overlapSphereCache = new Collider[1];
    Sequence ceilingShakeSequence;

    internal event Action OnTrayLoopComplete;
    internal event Action OnInitializationComplete;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if (doGenerateLevel)
        {
            ceilingDropCounter = ceilingDropInterval;
            StartCoroutine(LevelGeneratorCoroutine());
        }
    }

    /// <summary>
    ///   <para> Spawns bobble at local coordinates </para>
    /// </summary>
    internal void SpawnBobbleAtCoordinates(BobbleColour colour, Coordinates coordinates, bool doMatchBobble)
    {
        // check if given coordinates is valid for spawning
        if (!CheckCoordinates(coordinates))
        {
            // DebugConsole.Instance.Log($"Invalid Bobble spawning position at {coordinates}", LogType.Warning);
            return;
        }

        // ! [Engine Glitch] spawn with a distant offset to counteract false collision detections at (0, 0, 0)
        Bobble bobble = Instantiate(CurrentLevelManager.Instance.bobbleDictionary[colour].bobblePrefab, 99999f * Vector3.back, Quaternion.identity);
        bobble.transform.SetParent(attachableContainer, false);
        bobble.transform.localPosition = coordinates.GetPosition();
        
        allAttachables.Add(bobble);
        
        if (doMatchBobble)
        {
            StartCoroutine(BobbleTrayLoopCoroutine(bobble));
        }
    }

    /// <summary>
    /// Performs matching with neighbour bobbles, triggers bobble detachment and ceiling drop
    /// </summary>
    IEnumerator BobbleTrayLoopCoroutine(Bobble attachedBobble)
    {
        // wait a frame for objects to spawn in
        yield return null;
        // wait for objects to set up
        yield return new WaitForEndOfFrame();
        
        // try matching bobble and pop matching bobbles
        yield return StartCoroutine(PopMatchingBobblesCoroutine(attachedBobble));
        // detach bobbles that have no connection to ceiling
        yield return StartCoroutine(DetachLooseBobblesCoroutine());
        // check winning after matching
        if (CheckWinning()) yield break;
        
        // check losing before ceiling drop
        if (CheckLosing()) yield break;
        // count down ceiling drop counter
        ceilingDropCounter--;
        // wait for dropping animation if necessary
        yield return StartCoroutine(UpdateCeiling());
        // check losing after ceiling drop
        if (CheckLosing()) yield break;
        
        OnTrayLoopComplete?.Invoke();
    }
    
    /// <summary>
    ///   <para> Pops all matching bobbles </para>
    /// </summary>
    IEnumerator PopMatchingBobblesCoroutine(Bobble bobble)
    {
        // Find all matching neighbours of bobble and store them in matchingBobbles list
        allAttachables.ForEach(attachable => attachable.hasVisited = false);
        matchingBobbles.Clear();
        bobble.hasVisited = true;
        MatchNeighbours(bobble);

        // if the bobble matches three or more, pop the group
        if (matchingBobbles.Count > 2)
        {
            // for each matching bobble, remove references and play pop animation
            foreach (Bobble matchingBobble in matchingBobbles)
            {
                allAttachables.Remove(matchingBobble);
                matchingBobble.RemoveNeighbourReferences();
                StartCoroutine(matchingBobble.Pop());

                matchingBobble.scoreValue = (int) 4 * matchingBobbles.Count;
            }
        
            // wait for popping to complete
            yield return new WaitForSeconds(Bobble.poppingSpeed);
        }
    }
    
    /// <summary>
    /// Recursively traverse through neighbours and register neighbours of matching colour
    /// </summary>
    void MatchNeighbours(Bobble current)
    {
        // add to matching list
        matchingBobbles.Add(current);
        
        // check neighbours for matches
        foreach (Bobble neighbour in current.GetNeighbours().Where(attachable => attachable is Bobble).Cast<Bobble>())
        {
            // skip if neighbour DNE or already visited
            if (!neighbour || neighbour.hasVisited) continue;

            // flag to avoid revisit
            neighbour.hasVisited = true;

            // if neighbour matches, proceed to neighbour to check further matches
            if (neighbour.colour == current.colour)
            {
                MatchNeighbours(neighbour);
            }
        }
    }
    
    /// <summary>
    /// Establish a search from every ceiling tile and detach all bobbles that have no connection to ceiling
    /// </summary>
    IEnumerator DetachLooseBobblesCoroutine()
    {
        // reset flags
        allAttachables.ForEach(attachable => attachable.hasVisited = false);

        // flag all attachables that has a connection to the ceiling
        foreach (Ceiling ceilingTile in allAttachables.Where(attachable => attachable is Ceiling).Cast<Ceiling>())
        {
            if (ceilingTile == null || ceilingTile.hasVisited) continue;
            
            ceilingTile.FlagAllConnected();
        }

        // unflagged attachables are not connect to the ceiling, put them in a list sorted in a certain order for better dropping order
        List<Bobble> looseBobbles = allAttachables.Where(attachable => !attachable.hasVisited && attachable is Bobble).Cast<Bobble>()
            .OrderBy(bobble => bobble.transform.position.y)
            .ThenBy(bobble => -bobble.coordinates.N).ToList();

        // remove reference and detach all loose bobbles
        foreach (Bobble looseBobble in looseBobbles)
        {
            yield return new WaitForSeconds(looseBobbleDroppingInterval * (1f + Random.Range(-looseBobbleDroppingIntervalRandomness, looseBobbleDroppingIntervalRandomness)));
            
            allAttachables.Remove(looseBobble);
            looseBobble.RemoveNeighbourReferences();
            StartCoroutine(looseBobble.Detach());
        }
    }

    /// <summary>
    /// Spawns object according to state
    /// </summary>
    internal void SpawnObject(BobbleTrayObjectState state)
    {
        if (!CheckCoordinates(state.coordinates)) return;
        
        switch (state.type)
        {
            case BobbleTrayObjectType.Bobble:
            {
                BobbleColour bobbleColour = state.colour;
                if (state.colour == BobbleColour.Random)
                {
                    bobbleColour = spawnableBobbleColours[Random.Range(0, spawnableBobbleColours.Count)];
                }

                SpawnBobbleAtCoordinates(bobbleColour, state.coordinates, false);
                break;
            }
            case BobbleTrayObjectType.Ceiling:
            {
                BobbleTrayObject ceilingTile = Instantiate(CurrentLevelManager.Instance.ceilingDictionary[state.variant].objectPrefab, 99999f * Vector3.back, Quaternion.identity);
                ceilingTile.transform.SetParent(attachableContainer, false);
                ceilingTile.transform.SetLocalPositionAndRotation(state.coordinates.GetPosition(), state.orientation.ToRotation());
                
                allAttachables.Add(ceilingTile as BobbleTrayAttachable);
                break;
            }
            case BobbleTrayObjectType.Wall:
                break;
            case BobbleTrayObjectType.Other:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    /// <summary>
    /// Removes attachable at local coordinates
    /// </summary>
    internal void RemoveAttachableAtCoordinates(Coordinates coordinates)
    {
        foreach (BobbleTrayAttachable attachable in allAttachables.Where(attachable => attachable.coordinates.Equals(coordinates)))
        {
            allAttachables.Remove(attachable);
            attachable.RemoveNeighbourReferences();
            attachable.Destroy();
            return;
        }  
    }
        
    /// <summary>
    ///   <para> Returns whether given coordinates is available for spawning </para>
    /// </summary>
    bool CheckCoordinates(Coordinates coordinates)
    {
        return Physics.OverlapSphereNonAlloc(attachableContainer.TransformPoint(coordinates.GetPosition()), 0.45f, overlapSphereCache, Layer.BobbleTrayAttachableMask) == 0;
    }
    
    /// <summary>
    /// Given world position, returns the closest coordinates in tray
    /// </summary>
    internal Coordinates GetClosestCoordinates(Vector3 worldPosition)
    {
        return Coordinates.FromPosition(attachableContainer.InverseTransformPoint(worldPosition));
    }
    
    /// <summary>
    /// Destroys and dereferences all attachables in tray immediately
    /// </summary>
    void ClearTray()
    {
        foreach (BobbleTrayAttachable attachable in allAttachables)
        {
            attachable.Destroy();
        }
        allAttachables.Clear();
    }

    /// <summary>
    /// Checks if no bobble is left in tray, and trigger winning upon which
    /// </summary>
    bool CheckWinning()
    {
        if (allAttachables.Count(attachable => attachable is Bobble) == 0)
        {
            CurrentLevelManager.Instance.OnLevelEnded(true);
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Checks if any bobble touches the death zone, and triggers losing upon which
    /// </summary>
    bool CheckLosing()
    {
        if (allAttachables.Where(attachable => attachable is Bobble)
            .Any(attachable => Physics.CheckSphere(attachable.transform.position, 0.45f, Layer.DeathZoneMask, QueryTriggerInteraction.Collide)))
        {
            CurrentLevelManager.Instance.OnLevelEnded(false);
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Drop ceiling if the lowest bobble in tray is too high up, then shake and drop ceiling according ceilingDropCounter
    /// </summary>
    IEnumerator UpdateCeiling()
    {
        // avoid calculation when tray is loading
        if (allAttachables.Count > 0)
        {
            // calculate the world position of minimal visible bobble height from top centre of screen
            float minVisibleBobbleHeight = GetScreenTopHeight() - minVisibleBobbleRows * Coordinates.scale;
            // find the lowest bobble in tray
            float lowestBobbleHeight = allAttachables.Where(attachable => attachable is Bobble).Min(attachable => attachable.transform.position.y);
            // if the lowest bobble in tray is too high up, drop ceiling to allow more bobbles to be visible, also resets ceilingDropCounter
            // 0.2f * scale tolerance to avoid tiny drops
            if (lowestBobbleHeight > minVisibleBobbleHeight + 0.2f * Coordinates.scale)
            {
                ShakeContainer(0);
                float difference = lowestBobbleHeight - minVisibleBobbleHeight;
                yield return attachableContainer.DOMove(Vector3.up * -difference, ceilingDropSpeed * 2f).SetRelative(true).SetEase(Ease.OutQuad).WaitForCompletion();
                ceilingDropCounter = ceilingDropInterval;
            }
        }

        switch (ceilingDropCounter)
        {
            case 0:
                // stop shaking
                ShakeContainer(0);
                // drop the ceiling by one level
                yield return attachableContainer.DOMove(-Vector3.up * (ceilingDropAmount * Coordinates.scale), ceilingDropSpeed).SetRelative(true).SetEase(ceilingDropCurve).WaitForCompletion();
                // reset counter
                ceilingDropCounter = ceilingDropInterval;
                // update the ceiling again according to the new counter
                yield return StartCoroutine(UpdateCeiling());
                break;
            case 1:
                // shake less if ceilingDropInterval == 1 to avoid visual strain
                ShakeContainer(ceilingDropInterval == 1 ? 1 : 2);
                break;
            case 2:
                ShakeContainer(1);
                break;
            default:
                ShakeContainer(0);
                break;
        }
    }

    /// <summary>
    /// Visually shakes the ceiling and bobbles attached
    /// </summary>
    /// <param name="level"> How much to shake <br/>
    /// 0 = stop shaking <br/>
    /// 1 = shake periodically <br/>
    /// 2 = shake continuously
    /// </param>
    void ShakeContainer(int level)
    {
        // kill the current shaking sequences
        ceilingShakeSequence.Rewind();
        ceilingShakeSequence.Kill();

        switch (level)
        {
            case 0:
                break;
            case 1:
                ceilingShakeSequence = DOTween.Sequence()
                    .Append(attachableContainer.DOShakePosition(1f, ceilingShakeIntensity, ceilingShakeVibrato, 90f, false, true, ShakeRandomnessMode.Harmonic))
                    .AppendInterval(3f)
                    .SetLoops(-1);
                break;
            case 2:
                ceilingShakeSequence = DOTween.Sequence()
                    .Append(attachableContainer.DOShakePosition(10f, ceilingShakeIntensity, ceilingShakeVibrato, 90f, false, false, ShakeRandomnessMode.Harmonic))
                    .SetLoops(-1);
                break;
        }
    }

    internal void SetTrayWidth(int width)
    {
        trayWidth = width;
        leftWallContainer.localPosition = new Vector3(-trayWidth * 1f / Coordinates.diagonalMultiplier, 0, 0);
        rightWallContainer.localPosition = new Vector3(trayWidth * 1f / Coordinates.diagonalMultiplier, 0, 0);
        deathZoneContainer.localScale = new Vector3(2 * trayWidth * 1f / Coordinates.diagonalMultiplier, 1, 1);
    }

    IEnumerator LevelGeneratorCoroutine()
    {
        CurrentLevelManager.Instance.OnTrayEvent(true);

        for (int k = -generationRadius; k <= generationRadius; k++)
        {
            int depth = Mathf.FloorToInt(generationDepth / 2f - generationRadius - k * 1 / 2f);
            for (int z = Mathf.Max(-k - generationRadius, -generationRadius); z <= depth; z++)
            // hexagonal shape
            // for (int z = Mathf.Max(-k - generationWidth, -generationWidth); z <= depth && z <= Mathf.Min(-k + generationWidth, generationWidth); z++)
            {
                if (bobbleSpawnInterval != 0)
                {
                    yield return new WaitForSeconds(bobbleSpawnInterval);
                }
                
                // chance of leaving bobble slot empty
                if (Random.Range(0f, 1f) < emptyChance) continue;
                
                Coordinates coordinates = new Coordinates(k, z + generationRadius);
                BobbleColour colour = spawnableBobbleColours[Random.Range(0, spawnableBobbleColours.Count)];
                SpawnBobbleAtCoordinates(colour, coordinates, false);
            }
        }

        // wait a frame for instantiations to complete
        yield return null;
        
        // remove all floating bobbles generated
        yield return DetachLooseBobblesCoroutine();

        CurrentLevelManager.Instance.OnTrayEvent(false);
    }

    internal float GetScreenTopHeight()
    {
        // calculate the world position of minimal visible bobble height from top centre of screen
        float distanceFromCamera = Mathf.Abs(transform.position.z - GameManager.Instance.mainCamera.transform.position.z);
        float screenTopHeight = GameManager.Instance.mainCamera.ScreenToWorldPoint(new Vector3(0.5f * Screen.width - 1, Screen.height - 1, distanceFromCamera)).y;
        return screenTopHeight;
    }

    /// <summary>
    /// Returns currently present bobble colours in view
    /// </summary>
    internal BobbleColour[] GetBobbleColours()
    {
        // float topVisibleBobbleHeight = GetScreenTopHeight() + 0.5f * Coordinates.scale;
        float topVisibleBobbleHeight = GetScreenTopHeight();
        return allAttachables.Where(attachable => attachable is Bobble).Cast<Bobble>()
            .Where(attachable => attachable.transform.position.y < topVisibleBobbleHeight)
            .Select(bobble => bobble.colour).Distinct().ToArray();
    }

    /// <summary>
    /// Returns state of this bobble tray
    /// </summary>
    internal BobbleTrayState GetState()
    {
        List<BobbleTrayObjectState> objectStates = allAttachables.Select(attachable => attachable.GetState()).ToList();
        BobbleTrayState bobbleTrayState = new BobbleTrayState(spawnableBobbleColours.ToList(), trayWidth, ceilingDropInterval, ceilingDropAmount,
            attachableContainer.position, objectStates, ceilingDropCounter);
        return bobbleTrayState;
    }
    
    /// <summary>
    /// Load this bobble tray with given state
    /// </summary>
    internal IEnumerator LoadState(BobbleTrayState bobbleTrayState)
    {
        CurrentLevelManager.Instance.OnTrayEvent(true);

        // clear tray
        ClearTray();
        // stop shaking first to avoid transform overwrites caused by animation
        ShakeContainer(0);
        
        // restore parameters
        spawnableBobbleColours = bobbleTrayState.spawnableBobbleColours.ToList(); // copies the list
        ceilingDropAmount = bobbleTrayState.ceilingDropAmount;
        ceilingDropInterval = bobbleTrayState.ceilingDropInterval;
        // restore death line and wall positions
        SetTrayWidth(bobbleTrayState.trayWidth);
        // restore container position and ceiling drop counter
        attachableContainer.transform.position = bobbleTrayState.trayPosition;
        ceilingDropCounter = bobbleTrayState.ceilingDropCounter;
        yield return StartCoroutine(UpdateCeiling());
        
        // spawn in ceilings first
        foreach (BobbleTrayObjectState ceilingState in bobbleTrayState.objectStates.Where(state => state.type == BobbleTrayObjectType.Ceiling))
        {
            SpawnObject(ceilingState);
        }
        // spawn in bobbles
        foreach (BobbleTrayObjectState bobbleState in bobbleTrayState.objectStates.Where(state => state.type == BobbleTrayObjectType.Bobble))
        {
            if (bobbleSpawnInterval != 0)
            {
                yield return new WaitForSeconds(bobbleSpawnInterval);
            }
                    
            SpawnObject(bobbleState);
        }

        // wait a frame for instantiations to complete
        yield return null;

        CurrentLevelManager.Instance.OnTrayEvent(false);
        OnInitializationComplete?.Invoke();
    }

    void OnDestroy()
    {
        ceilingShakeSequence.Kill();
    }
}
