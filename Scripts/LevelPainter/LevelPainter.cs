using System;
using System.IO;
using UnityEngine;

public class LevelPainter : MonoBehaviour
{
    [SerializeField] BobbleDictionary bobbleDictionary;
    [SerializeField] BobbleTrayObjectDictionary ceilingDictionary;

    [SerializeField] BobbleTrayObjectType currentObjectType = BobbleTrayObjectType.Bobble;
    [SerializeField] BobbleColour currentBobbleColour = BobbleColour.Random;
    [SerializeField] BobbleTrayObjectVariant currentObjectVariant = BobbleTrayObjectVariant.Regular;
    [SerializeField] Vector2 cursorPosition;
    [SerializeField] Coordinates coordinatesInContainer;
    [SerializeField] Direction orientation;
    
    bool isPainting;
    bool isRemoving;

    void Start()
    {
        LevelSelectionManager.Instance.currentGameMode = GameMode.LevelPainter;
    }

    void FixedUpdate()
    {
        if (isPainting)
        {
            Paint();
        }
        else if (isRemoving)
        {
            Remove();
        }
        else
        {
            Preview();
        }
    }

    void MoveCursor(Vector2 position)
    {
        if (GameManager.Instance.isPaused || DebugConsole.Instance.isActive) return;

        cursorPosition = position;
        float distanceFromCamera = Mathf.Abs(transform.position.z - GameManager.Instance.mainCamera.transform.position.z);
        Vector3 aimPosition = GameManager.Instance.mainCamera.ScreenToWorldPoint(new Vector3(cursorPosition.x, cursorPosition.y, distanceFromCamera));
        coordinatesInContainer = CurrentLevelManager.Instance.bobbleTray.GetClosestCoordinates(aimPosition);
    }

    void Paint()
    {
        // Remove();
        
        switch (currentObjectType)
        {
            case BobbleTrayObjectType.Bobble:
                CurrentLevelManager.Instance.bobbleTray.SpawnBobbleAtCoordinates(currentBobbleColour, coordinatesInContainer, false);
                break;
            case BobbleTrayObjectType.Ceiling:
                CurrentLevelManager.Instance.bobbleTray.SpawnObject(new BobbleTrayObjectState(currentObjectType, coordinatesInContainer, orientation, currentObjectVariant));
                break;
            case BobbleTrayObjectType.Wall:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void Remove()
    {
        CurrentLevelManager.Instance.bobbleTray.RemoveAttachableAtCoordinates(coordinatesInContainer);
    }

    void Preview()
    {
        
    }

    void OnClick(bool toggle)
    {
        if (toggle == false)
        {
            TogglePainting(false);
            return;
        }
        
        if (GameManager.Instance.isPaused || DebugConsole.Instance.isActive) return;
        
        Ray ray = GameManager.Instance.mainCamera.ScreenPointToRay(cursorPosition);
        Physics.Raycast(ray, out RaycastHit hit);
        if (hit.collider == null)
        {
            TogglePainting(true);
        }
        else
        {
            // check if is selecting paintable
            LevelPainterSelectable selectable = hit.collider.GetComponent<LevelPainterSelectable>();
            if (selectable != null)
            {
                currentObjectType = selectable.type;

                switch (selectable.type)
                {
                    case BobbleTrayObjectType.Bobble:
                        currentBobbleColour = selectable.bobbleColour;
                        break;
                    case BobbleTrayObjectType.Ceiling:
                        currentObjectVariant = selectable.objectVariant;
                        break;
                    case BobbleTrayObjectType.Wall:
                        currentObjectVariant = selectable.objectVariant;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                // if not, start painting
                TogglePainting(true);
            }
        }
    }

    void TogglePainting(bool toggle)
    {
        if (toggle)
        {
            if (GameManager.Instance.isPaused || DebugConsole.Instance.isActive) return;

            isPainting = true;
        }
        else
        {
            isPainting = false;

        }
    }

    void ToggleRemoving(bool toggle)
    {
        if (toggle)
        {
            if (GameManager.Instance.isPaused || DebugConsole.Instance.isActive) return;
            isRemoving = true;
        }
        else
        {
            isRemoving = false;

        }
    }

    void Rotate(float rotation)
    {
        if (GameManager.Instance.isPaused || DebugConsole.Instance.isActive) return;
            
        if (rotation < 0)
        {
            orientation = orientation.Prev();
        }
        else if (rotation > 0)
        {
            orientation = orientation.Next();
        }
    }
    
    void OnEnable()
    {
        InputManager.OnPointerInput += MoveCursor;
        InputManager.OnPrimaryActionPointerInput += OnClick;
        InputManager.OnSecondaryActionPointerInput += ToggleRemoving;
        InputManager.OnScrollInput += Rotate;
    }

    void OnDisable()
    {
        InputManager.OnPointerInput -= MoveCursor;
        InputManager.OnPrimaryActionPointerInput -= OnClick;
        InputManager.OnSecondaryActionPointerInput -= ToggleRemoving;
        InputManager.OnScrollInput -= Rotate;
    }
}
