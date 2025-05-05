using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectionManager : MonoBehaviour
{
    internal static LevelSelectionManager Instance;
    
    [SerializeField] internal GameMode currentGameMode;

    [SerializeField] internal LevelList currentLevelList;
    [SerializeField] internal LevelProfile currentLevelProfile;
    [SerializeField] internal LevelStatus currentLevelStatus;
    LevelList loadingLevelList;
    LevelProfile loadingLevelProfile;
    LevelStatus loadingLevelStatus;

    [SerializeField] internal LevelList arcadeLevelList; // level list used in Arcade Mode
    internal int arcadeScore; // currently going arcade mode score
    internal int previousArcadeHighScore; // high score before current game

    void Awake()
    {
        if (Instance != null)
        {
            DebugConsole.Instance.Log("<LevelSelectionManger> already exists!", LogType.Warning);
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    internal void InitializeArcadeMode()
    {
        currentGameMode = GameMode.Arcade;
        arcadeScore = 0;
        previousArcadeHighScore = SaveDataManager.LevelStatusesData.arcadeHighScore;
        currentLevelList = arcadeLevelList;
        currentLevelProfile = arcadeLevelList.levels[0];
    }

    void ApplyLevelSelection()
    {
        if (loadingLevelList != null)
        {
            currentLevelList = loadingLevelList;
            loadingLevelList = null;
        }

        if (loadingLevelProfile != null)
        {
            currentLevelProfile = loadingLevelProfile;
            loadingLevelProfile = null;
        }

        if (loadingLevelStatus != null)
        {
            currentLevelStatus = loadingLevelStatus;
            loadingLevelStatus = null;
        }
    }

    /// <summary>
    /// Load level scene with LevelProfile and levelList
    /// </summary>
    internal void LoadLevel(LevelProfile levelProfile, LevelList levelList = null)
    {
        loadingLevelList = levelList;
        loadingLevelProfile = levelProfile;
        loadingLevelStatus = SaveDataManager.LevelStatusesData.levelStatuses.Find(status => status.levelID.Equals(loadingLevelProfile.levelID));

        switch (currentGameMode)
        {
            case GameMode.Adventure:
                SceneLoadManager.Instance.LoadScene("Level");
                break;
            case GameMode.Arcade:
                SceneLoadManager.Instance.LoadScene("ArcadeLevel");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    internal void LoadNextLevel()
    {
        LevelProfile levelProfile = currentLevelList.GetNext(currentLevelProfile);
        LoadLevel(levelProfile, currentLevelList);
    }
    
    /// <summary>
    /// Replace current scene with level asset by name
    /// </summary>
    internal void LoadLevelAsset(string levelName)
    {
        string path = Path.Combine("LevelData/", levelName);
        
        LevelProfile levelProfile = SaveDataManager.LoadAsset<LevelProfile>(path);
        if (levelProfile == null)
        {
            DebugConsole.Instance.Log(levelName + " does not exist!", LogType.Warning);
            return;
        }
        
        currentLevelProfile = levelProfile;
        currentLevelList = null;
        currentLevelStatus = SaveDataManager.LevelStatusesData.levelStatuses.Find(status => status.levelID.Equals(currentLevelProfile.levelID));
        
        CurrentLevelManager.Instance.LoadLevelAsset(currentLevelProfile);
    }

    internal bool HasNextLevel()
    {
        return currentLevelList == null ? false : currentLevelList.GetNext(currentLevelProfile);
    }

    internal bool IsNextLevelUnlocked()
    {
        LevelProfile nextLevel = currentLevelList.GetNext(currentLevelProfile);
        LevelStatus levelStatus = SaveDataManager.LevelStatusesData.levelStatuses.Find(status => status.levelID.Equals(nextLevel.levelID));
        return levelStatus.clearedStatus > LevelClearedStatus.Locked;
    }

    internal void UnlockNextLevel()
    {
        LevelProfile nextLevel = currentLevelList.GetNext(currentLevelProfile);
        
        // special case, unlock level 0 when current level is last in the list
        if (nextLevel == null)
        {
            nextLevel = currentLevelList.levels[0];
        }
        
        LevelStatus levelStatus = SaveDataManager.LevelStatusesData.levelStatuses.Find(status => status.levelID.Equals(nextLevel.levelID));
        if (levelStatus.clearedStatus == LevelClearedStatus.Locked)
        {
            levelStatus.clearedStatus = LevelClearedStatus.Unlocked;
        }
    }

    void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
    {
        ApplyLevelSelection();
    }

    void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }
}
