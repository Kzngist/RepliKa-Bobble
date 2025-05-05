using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugScreen : MonoBehaviour
{
    internal static DebugScreen Instance;
    [SerializeField] bool doDebugText = true;
    [SerializeField] bool doFPSText = true;
    
    [SerializeField] TextMeshProUGUI fpsText;
    [SerializeField] TextMeshProUGUI[] debugTexts;

    [SerializeField] int fpsSmoothing = 300; // how many frames used in fps calculation

    ControlScheme currentControlScheme;

    Coroutine updateFPSTextCoroutine;

    void Awake()
    {
        if (Instance != null)
        {
            DebugConsole.Instance.Log("<DebugScreen> already exists!", LogType.Warning);
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
            doDebugText = false;
            doFPSText = false;
        }
    }

    void Start()
    {
        fpsText.text = "";
        foreach (var debugText in debugTexts) debugText.text = "";
    }

    void SetText(TMP_Text textGUI, string text, bool doReplace = false)
    {
        // replace all
        if (doReplace)
        {
            textGUI.text = text;
        }
        // append
        else
        {
            textGUI.text += "\n" + text;
        }
    }

    IEnumerator UpdateFPSTextCoroutine()
    {
        Queue<float> pastFPSs = new Queue<float>();
        
        while (gameObject.activeSelf)
        {
            if (pastFPSs.Count >= fpsSmoothing)
            {
                pastFPSs.Dequeue();
            }
            pastFPSs.Enqueue(1 / Time.unscaledDeltaTime);

            int averageFPS = (int) pastFPSs.Average();
            int lowestFPS = (int) pastFPSs.OrderBy(i => i).Take(fpsSmoothing / 10).Average();

            if (doFPSText)
            {
                SetText(fpsText, $"FPS: {averageFPS:000}({lowestFPS:000}) | Frame Time: {1f / averageFPS * 1000:000}({1f / lowestFPS * 1000:000})ms", true);
            }
            else
            {
                SetText(fpsText, "", true);
            }

            yield return null;
        }
    }
    
    internal void UpdateDebugText(int index, string text, bool doReplace = true)
    {
        if (!doDebugText) return;

        SetText(debugTexts[index], text, doReplace);
    }

    internal void ToggleFPSText(bool doFPSText)
    {
        this.doFPSText = doFPSText;
    }

    internal void ToggleDebugText(bool doToggle)
    {
        this.doDebugText = doToggle;
    }
    
    void OnEnable()
    {
        updateFPSTextCoroutine = StartCoroutine(UpdateFPSTextCoroutine());
    }

    void OnDisable()
    {
        StopCoroutine(updateFPSTextCoroutine);
    }
}
