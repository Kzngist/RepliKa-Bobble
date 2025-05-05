using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum LogType
{
    Log,
    Warning
}

public class DebugConsole : MonoBehaviour
{
    internal static DebugConsole Instance;
    [SerializeField] bool isEnabled = true;

    [SerializeField] GameObject debugConsolePanel;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TextMeshProUGUI logText;
    
    internal bool isActive;

    static DebugCommand Help;
    static DebugCommand Quit;
    static DebugCommand Restart;
    static DebugCommand Reset;
    static DebugCommand<string> LoadScene;
    static DebugCommand<int> SetScore;
    static DebugCommand<float> SetTimer;
    static DebugCommand<float> SetGamespeed;
    static DebugCommand<int> SetQuality;
    static DebugCommand<int> SetFrameRate;
    static DebugCommand<bool> ToggleDebug;
    static DebugCommand<bool> ToggleFPSText;
    
    static DebugCommand<string> LoadLevelAsset;
    static DebugCommand<string, string> SaveLevelAsset;
    static DebugCommand LevelPainter;
    
    List<object> commandList;
    void Awake()
    {
        if (Instance != null)
        {
            DebugConsole.Instance.Log("<DebugConsole> already exists!", LogType.Warning);
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // if (!Application.isEditor)
        // {
        //     isEnabled = false;
        // }
        
        Help = new DebugCommand("/help", "Show help", "/help", () =>
        {
            Log("[HELP]");
            foreach (DebugCommandBase commandBase in commandList)
            {
                Log($"{commandBase.format} - {commandBase.description}");
            }
        });
        
        Quit = new DebugCommand("/quit", "Quit game", "/quit", () =>
        {
            Application.Quit();
        });
        
        Restart = new DebugCommand("/restart", "Restart game", "/restart", () =>
        {
            GameManager.Instance.Restart();
        });
        
        Reset = new DebugCommand("/reset", "Resets player", "/reset", () =>
        {
            Log("NOT IMPLEMENTED");
        });
        
        LoadScene = new DebugCommand<string>("/ls", "Load scene by name", "/ls <string>", (value) =>
        {
            SceneLoadManager.Instance.LoadScene(value);
        });

        SetScore = new DebugCommand<int>("/score", "Modify score", "/score <int>", (value) =>
        {
            // ScoreBoard.Instance.SetScore(value);
            // Log("[SYSTEM] Score set to " + value);
            Log("NOT IMPLEMENTED");
        });
        
        SetTimer = new DebugCommand<float>("/timer", "Modify timer", "/timer <float>", (value) =>
        {
            // ScoreBoard.Instance.SetTimer(value);
            // Log("[SYSTEM] Timer set to " + value);
            Log("NOT IMPLEMENTED");
        });
        
        SetGamespeed = new DebugCommand<float>("/gamespeed", "Sets game speed", "/gamespeed <float>", (value) =>
        {
            GameManager.Instance.SetTimeScale(value);
            Log("Game speed set to " + value);
        });
        
        SetQuality = new DebugCommand<int>("/quality", "Sets graphic quality", "/quality <int>", (value) =>
        {
            GameManager.Instance.SetQuality((GraphicQuality) value);
            Log("Quality set to " + value);
        });       
        
        SetFrameRate = new DebugCommand<int>("/framerate", "Sets target frame rate, 0 for device default", "/framerate <int>", (value) =>
        {
            GameManager.Instance.SetTargetFrameRate(value);
            Log("Frame rate set to " + value);
        });
        
        ToggleDebug = new DebugCommand<bool>("/debug", "Toggle debugging", "/debug <bool>", (value) =>
        {
            DebugWorld.Instance.ToggleDebugging(value);
            DebugScreen.Instance.ToggleDebugText(value);
            Log("Toggle debugging " + value);
        });
        
        ToggleFPSText = new DebugCommand<bool>("/fps", "Toggle FPS text", "/fps <bool>", (value) =>
        {
            DebugScreen.Instance.ToggleFPSText(value);
            Log("Toggle FPS " + value);
        });
        
        LoadLevelAsset = new DebugCommand<string>("/lla", "Load level asset by name", "/lla <string>", (value) =>
        {
            LevelSelectionManager.Instance.LoadLevelAsset(value);
        });
        
        SaveLevelAsset = new DebugCommand<string, string>("/sla", "Save level by id, followed by name (optional)", "/sla <string> (optional)<string>", (value, value2) =>
        {
#if UNITY_EDITOR
            CurrentLevelManager.Instance.SaveLevelAsset(value, value2);
#else
            DebugConsole.Instance.Log("Editor Only!");
#endif
        });

        LevelPainter = new DebugCommand("/llp", "Launch level painter", "/llp", () =>
        {
#if UNITY_EDITOR
            SceneLoadManager.Instance.LoadScene("LevelPainter");
#else
            DebugConsole.Instance.Log("Editor Only!");
#endif
        });
        
        commandList = new List<object>
        {
            Help,
            Quit,
            Restart,
            Reset,
            LoadScene,
            SetScore,
            SetTimer,
            SetGamespeed,
            SetQuality,
            SetFrameRate,
            ToggleDebug,
            ToggleFPSText,
            
            LoadLevelAsset,
            SaveLevelAsset,
            LevelPainter,
        };
    }

    void Start()
    {
        debugConsolePanel.SetActive(false);
        logText.text = "Type '/help' for help";
    }

    void ToggleConsole()
    {
        isActive = !isActive;
        debugConsolePanel.SetActive(isActive);
        
        if (isActive)
        {
            inputField.text = "/";
            inputField.MoveToEndOfLine(false, false);
            inputField.ActivateInputField();
        }
    }

    void ReturnConsole()
    {
        isActive = !isActive;
        debugConsolePanel.SetActive(isActive);
        
        if (!isActive)
        {
            HandleInput();
        }
        else
        {
            inputField.MoveToEndOfLine(false, false);
            inputField.ActivateInputField();
        }
    }
    
    void HandleInput()
    {
        string input = inputField.text;
        string[] properties = input.Split(' ');

        // casting <DebugCommand> to <DebugCommandBase>, for handling multiple types of classes together
        foreach (DebugCommandBase commandBase in commandList)
        {
            // if (!input.Contains(commandBase.id)) continue;
            if (!properties[0].Equals(commandBase.id)) continue;

            switch (commandBase)
            {
                // if input matches a certain <CommandBase>, cast <CommandBase> into its original form to perform actions
                case DebugCommand command:
                    command.Invoke();
                    break;
                case DebugCommand<int> command:
                    // todo try-catch
                    command.Invoke(int.Parse(properties[^1]));
                    break;
                case DebugCommand<float> command:
                    // todo try-catch
                    command.Invoke(float.Parse(properties[^1]));
                    break;
                case DebugCommand<bool> command:
                    // todo try-catch
                    command.Invoke(bool.Parse(properties[^1]));
                    break;
                case DebugCommand<string> command:
                    // todo try-catch
                    command.Invoke(properties[^1]);
                    break;
                case DebugCommand<Vector2> command:
                    // todo try-catch
                    command.Invoke(new Vector2(int.Parse(properties[^2]), int.Parse(properties[^1])));
                    break;
                case DebugCommand<Vector3> command:
                    // todo try-catch
                    command.Invoke(new Vector3(int.Parse(properties[^3]), int.Parse(properties[^2]), int.Parse(properties[^1])));
                    break;
                case DebugCommand<string, string> command:
                    // todo try-catch
                    if (properties.Length == 2)
                    {
                        command.Invoke(properties[^1], properties[^1]);
                    }
                    else
                    {
                        command.Invoke(properties[^2], properties[^1]);
                    }
                    break;
            }

            break;
        }
    }

    internal void Log(string msg, LogType logType = LogType.Log)
    {
        switch (logType)
        {
            case LogType.Log:
                logText.text += "\n[SYSTEM] " + msg;
                break;
            case LogType.Warning:
                logText.text += "\n[WARNING] " + msg;
                break;
            default:
                logText.text += "\n[UNKNOWN]" + msg;
                break;
        }
    }


    void OnEnable()
    {
        InputManager.OnDebugInput += ToggleConsole;
        InputManager.OnReturnInput += ReturnConsole;
    }

    void OnDisable()
    {
        InputManager.OnDebugInput -= ToggleConsole;
        InputManager.OnReturnInput -= ReturnConsole;
    }
}