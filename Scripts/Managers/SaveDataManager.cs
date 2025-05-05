using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    internal static SaveDataManager Instance;

    internal static SaveData_LevelStatuses LevelStatusesData;
    internal static SaveData_GameProgress gameProgressData;
    internal static SaveData_Preferences preferencesData;
    
    static string levelStatusesDataPath;
    static string gameProgressDataPath;
    static string referencesDataPath;
    
    void Awake()
    {
        #region Singleton
        if (Instance != null)
        {
            DebugConsole.Instance.Log("<SaveDataManager> already exists!", LogType.Warning);
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        #endregion

        levelStatusesDataPath = Path.Combine(Application.persistentDataPath, "LevelStatuses_v1.txt");
        gameProgressDataPath = Path.Combine(Application.persistentDataPath, "SaveData_v1.txt");
        referencesDataPath = Path.Combine(Application.persistentDataPath, "UserPreferences_v1.txt");

        ReadData(out LevelStatusesData, levelStatusesDataPath);
        ReadData(out gameProgressData, gameProgressDataPath);
        ReadData(out preferencesData, referencesDataPath);
    }
    
    static void WriteData<T>(T data, string filePath)
    {
        string jsonData = JsonUtility.ToJson(data, true);
        using StreamWriter streamWriter = File.CreateText(filePath);
        streamWriter.Write(jsonData);
        
        // DebugConsole.Instance.Log(data + " saved!", LogType.Log);
    }

    static void ReadData<T>(out T data, string filePath) where T : new()
    {
        if (File.Exists(filePath))
        {
            using StreamReader streamReader = File.OpenText(filePath);
            string jsonData = streamReader.ReadToEnd();
            data = JsonUtility.FromJson<T>(jsonData);
            
            // DebugConsole.Instance.Log(data + " loaded!", LogType.Log);
        }
        else
        {
            data = new T();
            WriteData(data, filePath);
            
            DebugConsole.Instance.Log(data + " does not exist, a new file is created at " + filePath, LogType.Log);
        }
    }

    internal static void SaveAsset(Object asset, string path)
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(asset);
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
    }

    internal static T LoadAsset<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    }

    // write data to disk
    internal static void Save()
    {
        WriteData(LevelStatusesData, levelStatusesDataPath);
        WriteData(gameProgressData, gameProgressDataPath);
        WriteData(preferencesData, referencesDataPath);
    }

    // load data form disk
    internal static void Load()
    {
        ReadData(out LevelStatusesData, levelStatusesDataPath);
        ReadData(out gameProgressData, gameProgressDataPath);
        ReadData(out preferencesData, referencesDataPath);
    }
}
