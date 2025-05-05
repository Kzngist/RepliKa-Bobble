using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class TouchScreenUIHandler : MonoBehaviour
{
    internal static TouchScreenUIHandler Instance;
    
    [SerializeField] GameObject touchScreenUIPanel;

    void Start()
    {
        // OnControlsChanged(InputManager.currentControlScheme);
        ToggleAimButtons(InputManager.isTouchScreenSupported && SaveDataManager.preferencesData.doShowAimButtons);
    }

    internal void ToggleAimButtons(bool toggle)
    {
        touchScreenUIPanel.SetActive(toggle);
    }

    void OnControlsChanged(ControlScheme controlScheme)
    {
        // if (controlScheme == ControlScheme.Touchscreen && SaveDataManager.preferencesData.doShowAimButtons)
        // {
        //     ToggleAimButtons(true);
        // }
        // else
        // {
        //     ToggleAimButtons(false);
        // }
    }

    void OnEnable()
    {
        Instance = this;
        InputManager.OnControlSchemeChanged += OnControlsChanged;
    }

    void OnDisable()
    {
        InputManager.OnControlSchemeChanged -= OnControlsChanged;
        Instance = null;
    }
}
