using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public enum GameMode
{
    Adventure,
    Arcade,
    LevelPainter
}

public class GameManager : MonoBehaviour
{
    internal static GameManager Instance;
    internal Camera mainCamera => Camera.main;
    
    internal bool isPaused = false;
    float timeScale = 1f;
    int randomSeed;

    void Awake()
    {
        if (Instance != null)
        {
            DebugConsole.Instance.Log("<GameManager> already exists!", LogType.Warning);
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // initialize Random to seed if set
        if (randomSeed != 0)
        {
            Random.InitState(randomSeed);
        }

        DOTween.SetTweensCapacity(20000, 100);
    }

    void Start()
    {
        SetGameSpeed(SaveDataManager.preferencesData.gameSpeed);
        SetQuality(SaveDataManager.preferencesData.graphicQuality);
        AudioManager.Instance.SetVolume(AudioType.BGM, SaveDataManager.preferencesData.BGMVolume);
        AudioManager.Instance.SetVolume(AudioType.SFX, SaveDataManager.preferencesData.SFXVolume);
        SetHighRefreshRate(SaveDataManager.preferencesData.doHighRefreshRate);
    }
    
    internal void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    internal void SetTimeScale(float value)
    {
        timeScale = value;

        if (isPaused) return;
        Time.timeScale = timeScale;
    }
    
    internal void SetTargetFrameRate(int value)
    {
        // value of 0 targets max framerate of display device
        Application.targetFrameRate = value == 0 ? Mathf.RoundToInt((float) Screen.currentResolution.refreshRateRatio.value) : value;
    }

    internal void TogglePause(bool doPause)
    {
        Time.timeScale = doPause ? 0f : timeScale;
        isPaused = doPause;
    }

    internal void SetGameSpeed(GameSpeed gameSpeed)
    {
        float timeScaleValue = gameSpeed switch
        {
            GameSpeed.Slower => 0.5f,
            GameSpeed.Slow => 0.75f,
            GameSpeed.Normal => 1f,
            GameSpeed.Fast => 1.5f,
            GameSpeed.Faster => 2f,
            _ => throw new ArgumentOutOfRangeException(nameof(gameSpeed), gameSpeed, null)
        };
        
        SetTimeScale(timeScaleValue);
    }

    internal void SetQuality(GraphicQuality value)
    {
        QualitySettings.SetQualityLevel((int) value);
    }

    internal void SetHighRefreshRate(bool doHighRefreshRate)
    {
        SetTargetFrameRate(doHighRefreshRate ? 0 : 60);
    }
    
    void OnSceneLoadComplete(Scene current, Scene next)
    {
        TogglePause(false);
    }
    
    void SaveGameProgress()
    {
        SaveDataManager.gameProgressData.randomSeed = randomSeed;
        SaveDataManager.gameProgressData.randomState = Random.state;
        
        // retrieve current level state
        SaveDataManager.gameProgressData.levelState = CurrentLevelManager.Instance.GetState();
        // SaveDataManager.gameProgressData.arrowState = TheArrow.current.GetState();
        
        SaveDataManager.Save();
    }

    void LoadGameProgress()
    {
        SaveDataManager.Load();
        
        randomSeed = SaveDataManager.gameProgressData.randomSeed;
        Random.state = SaveDataManager.gameProgressData.randomState;
        
        // restore level state to current level
        CurrentLevelManager.Instance.LoadState(SaveDataManager.gameProgressData.levelState);
        // TheArrow.current?.LoadState(SaveDataManager.gameProgressData.arrowState);
    }
    
    void OnEnable()
    {
        InputManager.OnSaveInput += SaveGameProgress;
        InputManager.OnLoadInput += LoadGameProgress;
        
        SceneManager.activeSceneChanged += OnSceneLoadComplete;
    }

    void OnDisable()
    {
        InputManager.OnSaveInput -= SaveGameProgress;
        InputManager.OnLoadInput -= LoadGameProgress;
        
        SceneManager.activeSceneChanged -= OnSceneLoadComplete;
    }
}
