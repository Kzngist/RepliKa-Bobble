using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

[SelectionBase]
public class TheArrow : MonoBehaviour
{
    internal static TheArrow current;
    
    [SerializeField] BobbleDictionary bobbleDictionary;
    [SerializeField] Transform currentSlot;
    [SerializeField] Transform nextSlot;
    [SerializeField] AimGuide aimGuide;
    [SerializeField] TMP_Text shotsText;

    TravellingBobble currentBobble;
    TravellingBobble nextBobble;
    internal int shotsLeft = 999; // how many shots the player has before game over

    float currentRotationSpeed;
    float rotationInput;
    [SerializeField] [Range(36f, 360f)] float maxRotationSpeed = 360f;
    [SerializeField] [Range(36f, 360f)] float acceleration = 360f;
    [SerializeField] [Range(0, 180f)] float rotationClamping = 60f;
    
    [SerializeField] Transform spawnPortal;
    [SerializeField] float animationDuration = 0.3f;
    int unfinishedActions = 0;
    bool isReadyToInitialize = false;
    
    [Space]
    [Header("Audio")]
    [SerializeField] AudioClipProfile bobbleSpawnSFX;
    [SerializeField] AudioClipProfile bobbleGearingSFX;
    
    
    void Start()
    {
        InputManager.OnPointerInput += CursorAim;
        InputManager.OnPrimaryActionButtonInput += OnShootInput;
        InputManager.OnSecondaryActionButtonInput += OnSwapInput;
        InputManager.OnRotateInput += OnRotateInput;
        
        CurrentLevelManager.Instance.bobbleTray.OnInitializationComplete += OnInitialize;
        CurrentLevelManager.Instance.bobbleTray.OnTrayLoopComplete += OnTrayLoopComplete;
    }

    void Update()
    {
        // call rotate even input == 0 to decelerate rotation
        if (!GameManager.Instance.isPaused)
        {
            Rotate();
        }
    }

    IEnumerator Initialize()
    {
        // wait for state loading to finish if any
        while (!isReadyToInitialize) yield return null;
        
        // check if loaded state already spawned bobbles on the arrow and spawn additional random bobbles accordingly
        if (currentBobble != null) yield break;
        
        if (nextBobble == null)
        {
            yield return RefillSlot();
        }
        yield return RefillSlot();
    }
    
    void Rotate()
    {
        currentRotationSpeed += rotationInput * acceleration * Time.unscaledDeltaTime;
        currentRotationSpeed = Mathf.Clamp(currentRotationSpeed, -maxRotationSpeed * Mathf.Abs(rotationInput), maxRotationSpeed * Mathf.Abs(rotationInput));
        if (currentRotationSpeed == 0 && rotationInput == 0) return;
        Quaternion targetRotation = Quaternion.AngleAxis(currentRotationSpeed * Time.unscaledDeltaTime, Vector3.back) * transform.rotation;
        transform.rotation = Quaternion.RotateTowards(Quaternion.LookRotation(Vector3.up, Vector3.back), targetRotation, rotationClamping);

        aimGuide.DrawAimGuide(currentSlot.position, transform.forward);
    }
    
    /// <summary>
    /// Aims arrow given cursor location on screen
    /// </summary>
    void CursorAim(Vector2 screenPosition)
    {
        if (GameManager.Instance.isPaused || DebugConsole.Instance.isActive) return;

        float distanceFromCamera = Mathf.Abs(transform.position.z - GameManager.Instance.mainCamera.transform.position.z);
        Vector3 aimTarget = GameManager.Instance.mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, distanceFromCamera));
        Vector3 aimDirection = (aimTarget - transform.position).normalized;
        
        Quaternion targetRotation = Quaternion.LookRotation(aimDirection, Vector3.back);
        transform.rotation = Quaternion.RotateTowards(Quaternion.LookRotation(Vector3.up, Vector3.back), targetRotation, rotationClamping);
        
        aimGuide.DrawAimGuide(currentSlot.position, transform.forward);
        
        DebugScreen.Instance.UpdateDebugText(2, $"Aiming: {aimTarget}");
    }

    TravellingBobble GenerateTravellingBobble(BobbleColour colour = BobbleColour.Default)
    {
        if (colour == BobbleColour.Default)
        {
            BobbleColour[] spawnableBobbleColours = CurrentLevelManager.Instance.bobbleTray.GetBobbleColours();
            colour = spawnableBobbleColours[Random.Range(0, spawnableBobbleColours.Length)];
        }

        TravellingBobble travellingBobble = Instantiate(bobbleDictionary[colour].travellingBobblePrefab);
        return travellingBobble;
    }
    
    IEnumerator RefillSlot(BobbleColour colour = BobbleColour.Default)
    {
        unfinishedActions++;

        // if next bobble exists
        if (nextBobble != null)
        {
            // check if next bobble colour exists in tray, if not, dump and spawn a new one
            if (!ValidateBobbleColour(nextBobble.colour))
            {
                yield return StartCoroutine(DumpNextBobble());
                yield return RefillSlot();
            }
            
            // swap next bobble to current slot
            yield return SwapSlot();
        }
        
        unfinishedActions--;

        nextBobble = GenerateTravellingBobble(colour);
        nextBobble.transform.SetParent(nextSlot, false);
        nextBobble.transform.position = nextSlot.position - 0.5f * Vector3.up;
        nextBobble.transform.localScale = Vector3.zero;
        nextBobble.transform.DOScale(1f, animationDuration).SetEase(Ease.OutCirc);
        nextBobble.transform.DOMove(nextSlot.position, animationDuration).SetEase(Ease.OutBack);

        spawnPortal.DOLocalRotate(new Vector3(0, 180, 0), animationDuration, RotateMode.LocalAxisAdd).SetEase(Ease.OutCubic);
    }

    IEnumerator SwapSlot()
    {
        unfinishedActions++;
        
        (currentBobble, nextBobble) = (nextBobble, currentBobble);
        
        Vector3 cross = Vector3.Cross(Vector3.forward, currentSlot.position - nextSlot.position);

        // move next bobble to current slot
        if (currentBobble != null)
        {
            Vector3[] pathA =
            {
                nextSlot.position,
                Vector3.Lerp(nextSlot.position, currentSlot.position, .5f) + 0.2f * cross,
                currentSlot.position
            };
            currentBobble.transform.DOPath(pathA, animationDuration, PathType.CatmullRom).SetEase(Ease.OutSine).OnComplete(() =>
            {
                AudioManager.Instance.PlaySound(bobbleGearingSFX, currentBobble.transform);
            });
            currentBobble.transform.SetParent(currentSlot);
        }

        // move current bobble to next slot
        if (nextBobble != null)
        {
            Vector3[] pathB =
            {
                currentSlot.position,
                Vector3.Lerp(currentSlot.position, nextSlot.position, .5f) - 0.2f * cross,
                nextSlot.position
            };
            nextBobble.transform.DOPath(pathB, animationDuration, PathType.CatmullRom).SetEase(Ease.OutSine);
            nextBobble.transform.SetParent(nextSlot);
        }

        yield return new WaitForSeconds(animationDuration);

        unfinishedActions--;
    }

    bool ValidateBobbleColour(BobbleColour bobbleColour)
    {
        return CurrentLevelManager.Instance.bobbleTray.GetBobbleColours().Contains(bobbleColour);
    }

    IEnumerator DumpNextBobble()
    {
        TravellingBobble bobbleToDump = nextBobble;
        nextBobble = null;
        
        yield return bobbleToDump.transform.DOScale(0f, animationDuration).SetEase(Ease.InCirc).WaitForCompletion();
        Destroy(bobbleToDump.gameObject);
    }

    void UpdateShotsText()
    {
        shotsText.text = shotsLeft.ToString();
    }

    internal ArrowState GetState()
    {
        return new ArrowState(currentBobble.colour, nextBobble.colour, shotsLeft);
    }

    internal IEnumerator LoadState(ArrowState state)
    {
        isReadyToInitialize = false;
        
        if (currentBobble != null)
        {
            Destroy(currentBobble.gameObject);
            currentBobble = null;
        }
        if (nextBobble != null)
        {
            Destroy(nextBobble.gameObject);
            nextBobble = null;
        }

        // check if current or next bobbles are defined in state,
        // if so, spawn according to the state;
        // if not, let initialization spawn at random
        if (state.currentBobbleColour != BobbleColour.Default)
        {
            yield return RefillSlot(state.currentBobbleColour);
        }
        if (state.nextBobbleColour != BobbleColour.Default)
        {
            yield return RefillSlot(state.nextBobbleColour);
        }

        shotsLeft = state.shotsLeft;
        
        isReadyToInitialize = true;
    }
    
    void OnShootInput()
    {
        if (CurrentLevelManager.Instance.isTrayEventInProgress || GameManager.Instance.isPaused || DebugConsole.Instance.isActive) return;

        if (unfinishedActions > 0) return;

        unfinishedActions++;
        currentBobble.Weeeeee(transform.forward);
    }
    
    void OnSwapInput()
    {
        if (CurrentLevelManager.Instance.isTrayEventInProgress || GameManager.Instance.isPaused || DebugConsole.Instance.isActive) return;

        if (unfinishedActions > 0) return;
        
        StartCoroutine(SwapSlot());
    }

    void OnRotateInput(float amount)
    {
        rotationInput = amount;
    }
    
    void OnInitialize()
    {
        StartCoroutine(Initialize());
    }

    void OnTrayLoopComplete()
    {
        unfinishedActions--;
        StartCoroutine(RefillSlot());
    }
    
    void OnEnable()
    {
        current = this;
    }
    
    void OnDisable()
    {
        InputManager.OnPointerInput -= CursorAim;
        InputManager.OnPrimaryActionButtonInput -= OnShootInput;
        InputManager.OnSecondaryActionButtonInput -= OnSwapInput;
        InputManager.OnRotateInput -= OnRotateInput;

        if (CurrentLevelManager.Instance)
        {
            CurrentLevelManager.Instance.bobbleTray.OnInitializationComplete -= OnInitialize;
            CurrentLevelManager.Instance.bobbleTray.OnTrayLoopComplete -= OnTrayLoopComplete;
        }

        current = null;
    }
}
