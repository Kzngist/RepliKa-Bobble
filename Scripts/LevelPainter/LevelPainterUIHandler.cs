using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelPainterUIHandler : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] [Range(0f, 1f)] float cameraSmoothTime = 0.3f;
    
    [SerializeField] Toggle[] spawnableBobbleColourToggles;
    [SerializeField] Slider trayWidthSlider;
    [SerializeField] Slider ceilingDropIntervalSlider;
    [SerializeField] Slider ceilingDropCounterSlider;
    [SerializeField] Slider ceilingDropAmountSlider;
    [SerializeField] Slider shotsSlider;

    BobbleColour[] spawnableBobbleColourIndexer;
    
    void Start()
    {
        CurrentLevelManager.Instance.bobbleTray.OnInitializationComplete += UpdateUI;

        spawnableBobbleColourIndexer = new BobbleColour[spawnableBobbleColourToggles.Length];
        for (int i = 0; i < spawnableBobbleColourToggles.Length; i++)
        {
            spawnableBobbleColourIndexer[i] = spawnableBobbleColourToggles[i].GetComponent<LevelPainterSelectable>().bobbleColour;
        }
    }

    void UpdateUI()
    {
        for (int i = 0; i < spawnableBobbleColourToggles.Length; i++)
        {
            BobbleColour colour = spawnableBobbleColourIndexer[i];
            spawnableBobbleColourToggles[i].SetIsOnWithoutNotify(CurrentLevelManager.Instance.bobbleTray.spawnableBobbleColours.Contains(colour));
        }
        
        trayWidthSlider.value = CurrentLevelManager.Instance.bobbleTray.trayWidth;
        ceilingDropIntervalSlider.value = CurrentLevelManager.Instance.bobbleTray.ceilingDropInterval;
        ceilingDropCounterSlider.value = CurrentLevelManager.Instance.bobbleTray.ceilingDropCounter;
        ceilingDropAmountSlider.value = CurrentLevelManager.Instance.bobbleTray.ceilingDropAmount;
        // shotsSlider.value = CurrentLevelManager.Instance.theArrow.shotsLeft;
    }
    
    public void OnSpawnableBobbleColoursChange()
    {
        List<BobbleColour> bobbleColours = new List<BobbleColour>();

        for (int i = 0; i < spawnableBobbleColourToggles.Length; i++)
        {
            if (spawnableBobbleColourToggles[i].isOn)
            {
                bobbleColours.Add(spawnableBobbleColourIndexer[i]);
            }

            CurrentLevelManager.Instance.bobbleTray.spawnableBobbleColours = bobbleColours;
        }
    }
    
    public void OnTrayWidthChange(float value)
    {
        CurrentLevelManager.Instance.bobbleTray.SetTrayWidth((int) value);
    }

    public void OnCeilingDropIntervalChange(float value)
    {
        CurrentLevelManager.Instance.bobbleTray.ceilingDropInterval = (int) value;
    }

    public void OnCeilingDropCounterChange(float value)
    {
        CurrentLevelManager.Instance.bobbleTray.ceilingDropCounter = (int) value;
    }
    
    public void OnCeilingDropAmountChange(float value)
    {
        value = MathF.Round(value, 1);
        CurrentLevelManager.Instance.bobbleTray.ceilingDropAmount = value;
    }
    
    public void OnShotsChange(float value)
    {
        CurrentLevelManager.Instance.theArrow.shotsLeft = (int) value;
    }

    public void OnCameraHeightChange(float value)
    {
        cam.transform.DOMoveY(value, cameraSmoothTime);
    }

    void OnDisable()
    {
        if (CurrentLevelManager.Instance)
        {
            CurrentLevelManager.Instance.bobbleTray.OnInitializationComplete -= UpdateUI;
        }
    }
}
