using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public enum DebugObjectType
{
    Stick,
    Sphere
}

public class DebugWorld : MonoBehaviour
{
    internal static DebugWorld Instance;

    [SerializeField] internal bool isEnabled = false;

    [SerializeField] Transform debugObjectsContainer;
    [SerializeField] int debugObjectsMaxCount = 8;
    [SerializeField] GameObject debugStick;
    [SerializeField] GameObject debugSphere;
    readonly Queue<GameObject> debugObjects = new Queue<GameObject>();

    internal event Action<bool> OnToggleDebug;
    
    void Awake()
    {
        if (Instance != null)
        {
            DebugConsole.Instance.Log("<DebugWorld> already exists!", LogType.Warning);
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        if (!Application.isEditor)
        {
            isEnabled = false;
        }
    }

    void DebugObjectEnqueue(GameObject gameObject)
    {
        debugObjects.Enqueue(gameObject);
        
        if (debugObjects.Count > debugObjectsMaxCount)
        {
            Destroy(debugObjects.Dequeue());
        }
    }
    
    internal void ObjectAtPosition(Vector3 position, Quaternion rotation, DebugObjectType type)
    {
        if (!isEnabled) return;

        GameObject objectToSpawn = type switch
        {
            DebugObjectType.Stick => debugStick,
            DebugObjectType.Sphere => debugSphere,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        GameObject spawnedObject = Instantiate(objectToSpawn, position, rotation, debugObjectsContainer);
        DebugObjectEnqueue(spawnedObject);
    }
    
    internal void DrawSphere(Vector3 position, float size)
    {
        if (!isEnabled) return;

        GameObject sphere = Instantiate(debugSphere, debugObjectsContainer);
        sphere.transform.position = position;
        sphere.transform.localScale = size * Vector3.one;
        
        DebugObjectEnqueue(sphere);
    }

    internal void ClearDebugObjects()
    {
        while (debugObjects.Count > 0)
        {
            Destroy(debugObjects.Dequeue());
        }
    }

    internal void ToggleDebugging(bool value)
    {
        isEnabled = value;
        OnToggleDebug?.Invoke(value);

        if (isEnabled) return;
        
        ClearDebugObjects();
    }

    void OnEnable()
    {
        SceneLoadManager.OnSceneLoadStart += ClearDebugObjects;
    }

    void OnDisable()
    {
        SceneLoadManager.OnSceneLoadStart -= ClearDebugObjects;
    }
}
