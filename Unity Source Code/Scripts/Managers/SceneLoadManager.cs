using System;
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoadManager : MonoBehaviour
{
    internal static SceneLoadManager Instance;

    [SerializeField] RectTransform loadingScreen;
    [SerializeField] Slider loadingBar;

    [SerializeField] float loadingScreenAnimationDuration = 0.5f;

    [Space]
    [Header("Audio")]
    [SerializeField] AudioClipProfile sceneLoadInSFX;
    [SerializeField] AudioClipProfile sceneLoadOutSFX;
    
    bool isLoading = false;
    
    internal static event Action OnSceneLoadStart;
    internal static event Action OnSceneLoadComplete;
    
    void Awake()
    {
        #region Singleton
        if (Instance != null)
        {
            DebugConsole.Instance.Log("<SceneLoadManager> already exists!", LogType.Warning);
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        #endregion

        SceneManager.LoadScene("MainMenu");
    }

    internal void LoadScene(string sceneName)
    {
        if (isLoading) return;
        
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    internal void LoadScene(int index)
    {
        LoadScene(System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(index)));
    }
    
    internal void ReloadScene()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator LoadSceneCoroutine(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName)) yield break;
        
        isLoading = true;
        OnSceneLoadStart?.Invoke();
        
        loadingScreen.anchoredPosition = new Vector2(5120f, 0f);
        loadingScreen.gameObject.SetActive(true);
        AudioManager.Instance.PlaySound(sceneLoadInSFX);
        yield return loadingScreen.DOAnchorPos(new Vector2(0f, 0f), loadingScreenAnimationDuration).SetUpdate(true).SetEase(Ease.OutCirc).WaitForCompletion();
        
        yield return LoadSceneProgress(sceneName);
        
        AudioManager.Instance.PlaySound(sceneLoadOutSFX);
        yield return loadingScreen.DOAnchorPos(new Vector2(-5120f, 0f), loadingScreenAnimationDuration).SetUpdate(true).SetEase(Ease.OutCirc).WaitForCompletion();
        loadingScreen.gameObject.SetActive(false);
        
        OnSceneLoadComplete?.Invoke();
        isLoading = false;
    }

    IEnumerator LoadSceneProgress(string sceneName)
    {
        loadingBar.gameObject.SetActive(true);

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncOperation.isDone)
        {
            loadingBar.value = asyncOperation.progress / 0.9f;
            
            yield return null;
        }

        loadingBar.value = 1;
        yield return null;

        loadingBar.gameObject.SetActive(false);
    }
}
