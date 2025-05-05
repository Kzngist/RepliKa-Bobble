using System;
using System.IO;
using TMPro;
using UnityEngine;

public class CurrentLevelManager : MonoBehaviour
{
    internal static CurrentLevelManager Instance;
    
    [SerializeField] internal BobbleTray bobbleTray;
    [SerializeField] internal TheArrow theArrow => TheArrow.current;
    [SerializeField] internal BobbleDictionary bobbleDictionary;
    [SerializeField] internal BobbleTrayObjectDictionary ceilingDictionary;
    [SerializeField] internal GamePlayUIHandler gamePlayUIHandler;
    
    [SerializeField] ScorePopUp scorePopUpPrefab;
    
    int score;
    internal bool isTrayEventInProgress = false;
    
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetScore(0);
        
        if (LevelSelectionManager.Instance.currentLevelProfile != null)
        {
            LoadLevelAsset(LevelSelectionManager.Instance.currentLevelProfile);
        }
    }
        
    internal void PopScore(Vector3 worldPosition, int score)
    {
        StartCoroutine(Instantiate(scorePopUpPrefab, worldPosition, Quaternion.identity).PopScore(score));
        AddScore(score);
    }

    internal void AddScore(int value)
    {
        score += value;
        SetScore(score);
    }

    internal void SetScore(int value)
    {
        score = value;

        switch (LevelSelectionManager.Instance.currentGameMode)
        {
            case GameMode.Adventure:
                gamePlayUIHandler?.SetScoreText1($"{"SCORE: ",7}{score:000000}");
                ResultScreenUIAnimator.Instance?.SetScore(score);
                
                int highScore = LevelSelectionManager.Instance.currentLevelStatus.highScore;
                if (score > highScore)
                {
                    highScore = score;
                    gamePlayUIHandler?.ToggleNewRecordTag(true);
                    ResultScreenUIAnimator.Instance?.ToggleNewRecordTag(true);
                }
                gamePlayUIHandler?.SetScoreText2($"{"BEST: ",7}{highScore:000000}");
                break;
            
            case GameMode.Arcade:
                int totalScore = score + LevelSelectionManager.Instance.arcadeScore;
                gamePlayUIHandler?.SetScoreText1($"{"SCORE: ",7}{totalScore:00000000}");
                ResultScreenUIAnimator.Instance?.SetScore(totalScore);
                
                int highArcadeScore = LevelSelectionManager.Instance.previousArcadeHighScore;
                if (totalScore > highArcadeScore)
                {
                    highArcadeScore = totalScore;
                    gamePlayUIHandler?.ToggleNewRecordTag(true);
                    ResultScreenUIAnimator.Instance?.ToggleNewRecordTag(true);
                }
                gamePlayUIHandler?.SetScoreText2($"{"BEST: ",7}{highArcadeScore:00000000}");
                break;
        }
    }

    internal void OnLevelEnded(bool isWin)
    {
        switch (LevelSelectionManager.Instance.currentGameMode)
        {
            case GameMode.Adventure:
                if (isWin)
                {
                    LevelSelectionManager.Instance.currentLevelStatus.clearedStatus = LevelClearedStatus.Cleared;
                    LevelSelectionManager.Instance.UnlockNextLevel();
                }

                StartCoroutine(ResultScreenUIAnimator.Instance.ShowResult(isWin));
                break;
            case GameMode.Arcade:
                StartCoroutine(ResultScreenUIAnimator.Instance.ShowResult(isWin));
                break;
        }
    }

    void SaveAdventureStatus()
    {
        LevelStatus levelStatus = LevelSelectionManager.Instance.currentLevelStatus;
        if (levelStatus == null) return;
        
        if (levelStatus.clearedStatus != LevelClearedStatus.Cleared) return;

        levelStatus.highScore = score > levelStatus.highScore ? score : levelStatus.highScore;

        SaveDataManager.Save();
    }

    void SaveArcadeScore()
    {
        int totalScore = score + LevelSelectionManager.Instance.arcadeScore;
        LevelSelectionManager.Instance.arcadeScore = totalScore;
        if (totalScore <= SaveDataManager.LevelStatusesData.arcadeHighScore) return;
        
        SaveDataManager.LevelStatusesData.arcadeHighScore = totalScore;
        SaveDataManager.Save();
    }
    
    internal void SaveLevelStatus()
    {
        switch (LevelSelectionManager.Instance.currentGameMode)
        {
            case GameMode.Adventure:
                SaveAdventureStatus();
                break;
            case GameMode.Arcade:
                SaveArcadeScore();
                break;
        }
    }
    
    internal void OnTrayEvent(bool isStart)
    {
        isTrayEventInProgress = isStart;
    }

    internal LevelState GetState()
    {
        BobbleTrayState bobbleTrayState = bobbleTray.GetState();
        ArrowState arrowState = theArrow?.GetState();
        
        LevelState levelState = new LevelState(bobbleTrayState, arrowState, score);
        return levelState;
    }

    internal void LoadState(LevelState state)
    {
        SetScore(state.score);
        StartCoroutine(bobbleTray.LoadState(state.bobbleTrayState));
        if (theArrow != null)
        {
            StartCoroutine(theArrow.LoadState(state.arrowState));
        }
    }
    
    /// <summary>
    /// Set level profile and load level states
    /// </summary>
    internal void LoadLevelAsset(LevelProfile levelProfile)
    {
        bobbleDictionary = levelProfile.bobbleDictionary;
        ceilingDictionary = levelProfile.ceilingDictionary;

        LoadState(levelProfile.levelState);
        gamePlayUIHandler?.SetLevelNameText(levelProfile.levelName);

        DebugConsole.Instance.Log("Loaded level " + levelProfile.levelID);
    }

    /// <summary>
    /// Creates an instance of the level and stores it in Assets
    /// </summary>
    /// <param name="levelID"> Name of the asset to be stored </param>
    /// <param name="levelName"> Name displayed </param>
    internal void SaveLevelAsset(string levelID, string levelName)
    {
        LevelProfile levelProfile = ScriptableObject.CreateInstance<LevelProfile>();
        levelProfile.levelState = GetState();
        levelProfile.levelID = levelID;
        levelProfile.levelName = levelName;
        levelProfile.levelDescription = levelName;
        levelProfile.bobbleDictionary = bobbleDictionary;
        levelProfile.ceilingDictionary = ceilingDictionary;

        levelID = string.IsNullOrWhiteSpace(levelID) ? "TLevel.asset" : levelID + ".asset";
        string path = Path.Combine("Assets/Resources/LevelData/", levelID);
        SaveDataManager.SaveAsset(levelProfile, path);
        
        DebugConsole.Instance.Log("Level file saved to " + path);
    }

    void OnDisable()
    {
        SaveLevelStatus();
        Instance = null;
    }
}
