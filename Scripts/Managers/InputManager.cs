using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public enum ControlScheme
{
    General
}

public class InputManager : MonoBehaviour
{
    internal static InputManager Instance;
    
    [Header("Editor")]
    [SerializeField] bool forceTouchSupport;
    
    internal static PlayerInput playerInput;
    
    internal static bool isTouchScreenSupported;
    internal static bool isPlayerInputEnabled = true;
    internal static Vector2 pointerPosition;
    
    internal static event Action<Vector2> OnPointerInput;
    internal static event Action<bool> OnPrimaryActionPointerInput; // true = isDown; false = isUp
    internal static event Action OnPrimaryActionButtonInput;
    internal static event Action<bool> OnSecondaryActionPointerInput;
    internal static event Action OnSecondaryActionButtonInput;
    internal static event Action OnDebugInput;
    internal static event Action OnReturnInput;
    internal static event Action OnSaveInput;
    internal static event Action OnLoadInput;
    internal static event Action<float> OnScrollInput;
    internal static event Action<float> OnRotateInput;
    
    internal static event Action<ControlScheme> OnControlSchemeChanged;

    internal static bool IsPointerOverUI
    {
        get
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = pointerPosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }
    }

    void Awake()
    {
        if (Instance != null)
        {
            DebugConsole.Instance.Log("<InputManager> already exists!", LogType.Warning);
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        playerInput = GetComponent<PlayerInput>();
        isTouchScreenSupported = Application.isMobilePlatform;
        
#if UNITY_EDITOR
        if (forceTouchSupport)
        {
            isTouchScreenSupported = true;
        }
#endif
    }

    void Start()
    {
        playerInput.onControlsChanged += OnControlsChanged;
    }

    // pointer or touch position
    internal void OnPointer(InputValue value)
    {
        if (!isPlayerInputEnabled) return;
        
        pointerPosition = value.Get<Vector2>();
        
        if (IsPointerOverUI) return;

        OnPointerInput?.Invoke(pointerPosition);
    }
    
    // Pointer or touch press and release
    internal void OnPrimaryActionPointer(InputValue value)
    {
        if (!isPlayerInputEnabled) return;
        
        if (value.Get<float>() > 0)
        {
            if (IsPointerOverUI) return;
            OnPrimaryActionPointerInput?.Invoke(true);
        }
        else
        {
            OnPrimaryActionPointerInput?.Invoke(false);
        }
    }
    
    // pointer press, or touch release
    internal void OnPrimaryActionPointerTrigger(InputValue value)
    {
        if (!isPlayerInputEnabled) return;
        if (IsPointerOverUI) return;

        OnPrimaryActionButtonInput?.Invoke();
    }

    // Button press
    internal void OnPrimaryActionButton(InputValue value)
    {
        if (!isPlayerInputEnabled) return;
        
        OnPrimaryActionButtonInput?.Invoke();
    }
    
    internal void OnSecondaryActionPointer(InputValue value)
    {
        if (!isPlayerInputEnabled) return;

        if (value.Get<float>() > 0)
        {
            if (IsPointerOverUI) return;
            OnSecondaryActionPointerInput?.Invoke(true);
            OnSecondaryActionButtonInput?.Invoke();
        }
        else
        {
            OnSecondaryActionPointerInput?.Invoke(false);
        }
    }
    
    internal void OnSecondaryActionButton(InputValue value)
    {
        if (!isPlayerInputEnabled) return;
        
        OnSecondaryActionButtonInput?.Invoke();
    }

    internal void OnDebug()
    {
        OnDebugInput?.Invoke();
    }

    internal void OnReturn()
    {
        OnReturnInput?.Invoke();
    }

    internal void OnSave()
    {
        if (!isPlayerInputEnabled || GameManager.Instance.isPaused || DebugConsole.Instance.isActive) return;

        OnSaveInput?.Invoke();
    }

    internal void OnLoad()
    {
        if (!isPlayerInputEnabled || GameManager.Instance.isPaused || DebugConsole.Instance.isActive) return;

        OnLoadInput?.Invoke();
    }

    internal void OnScroll(InputValue value)
    {
        if (!isPlayerInputEnabled) return;

        OnScrollInput?.Invoke(value.Get<float>());
    }

    internal void OnRotate(InputValue value)
    {
        if (!isPlayerInputEnabled) return;
        
        OnRotateInput?.Invoke(value.Get<float>());
    }

    void OnControlsChanged(PlayerInput context)
    {
        ControlScheme newScheme = context.currentControlScheme switch
        {
            "General" => ControlScheme.General,
            _ => default
        };

        // print(context.gameObject.name + ": " + context.currentControlScheme);

        // if (currentControlScheme == newScheme) return;
        
        OnControlSchemeChanged?.Invoke(newScheme);
    }

    void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.onControlsChanged -= OnControlsChanged;
        }
    }
}
