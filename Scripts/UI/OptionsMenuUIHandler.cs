using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class OptionsMenuUIHandler : MenuUIHandler
{
    [SerializeField] Slider gameSpeedSlider;
    [SerializeField] Slider graphicsSlider;
    [SerializeField] Slider BGMVolumeSlider;
    [SerializeField] Slider SFXVolumeSlider;
    [SerializeField] TMP_Text gameSpeedText;
    [SerializeField] TMP_Text GraphicsText;
    [SerializeField] TMP_Text BGMVolumeText;
    [SerializeField] TMP_Text SFXVolumeText;
    [SerializeField] Toggle highRefreshRateToggle;
    [SerializeField] Toggle showAimButtonsToggle;

    [SerializeField] GameObject highRefreshRatePanel;
    [SerializeField] GameObject showAimButtonsPanel;

    void Awake()
    {
        SetActive(false);
    }

    void Start()
    {
        int gameSpeedSliderValue = (int) SaveDataManager.preferencesData.gameSpeed;
        gameSpeedSlider.SetValueWithoutNotify(gameSpeedSliderValue);
        OnGameSpeedChange(gameSpeedSliderValue);
        
        int graphicsSliderValue = (int) SaveDataManager.preferencesData.graphicQuality;
        graphicsSlider.SetValueWithoutNotify(graphicsSliderValue);
        OnGraphicsChange(graphicsSliderValue);
        
        float BGMVolumeSliderValue = SaveDataManager.preferencesData.BGMVolume;
        BGMVolumeSlider.SetValueWithoutNotify(BGMVolumeSliderValue);
        OnBGMVolumeChange(BGMVolumeSliderValue);
        
        float SFXVolumeSliderValue = SaveDataManager.preferencesData.SFXVolume;
        SFXVolumeSlider.SetValueWithoutNotify(SFXVolumeSliderValue);
        OnSFXVolumeChange(SFXVolumeSliderValue);

        bool doHighRefreshRate = SaveDataManager.preferencesData.doHighRefreshRate;
        highRefreshRateToggle.SetIsOnWithoutNotify(doHighRefreshRate);
        OnHighRefreshRateChange(doHighRefreshRate);

        bool doShowAimButtons = SaveDataManager.preferencesData.doShowAimButtons;
        showAimButtonsToggle.SetIsOnWithoutNotify(doShowAimButtons);
        OnShowAimButtonsChange(doShowAimButtons);

        int refreshRate = Mathf.RoundToInt((float) Screen.currentResolution.refreshRateRatio.value);
        if (refreshRate <= 60 && refreshRate != 0)
        {
            highRefreshRatePanel.SetActive(false);
        }

        if (!InputManager.isTouchScreenSupported)
        {
            showAimButtonsPanel.SetActive(false);
        }
    }

    void OnGameSpeedChange(float value)
    {
        GameSpeed gameSpeed = (GameSpeed) value;

        gameSpeedText.text = gameSpeed.ToString();
        GameManager.Instance.SetGameSpeed(gameSpeed);
        SaveDataManager.preferencesData.gameSpeed = gameSpeed;
    }
    void OnBGMVolumeChange(float volume)
    {
        BGMVolumeText.text = $"{volume:0}";
        AudioManager.Instance.SetVolume(AudioType.BGM, volume);
        SaveDataManager.preferencesData.BGMVolume = volume;
    }
    
    void OnSFXVolumeChange(float volume)
    {
        SFXVolumeText.text = $"{volume:0}";
        AudioManager.Instance.SetVolume(AudioType.SFX, volume);
        SaveDataManager.preferencesData.SFXVolume = volume;
    }
    
    void OnGraphicsChange(float value)
    {
        GraphicQuality graphicQuality = (GraphicQuality) value;

        GraphicsText.text = graphicQuality.ToString();
        GameManager.Instance.SetQuality(graphicQuality);
        SaveDataManager.preferencesData.graphicQuality = graphicQuality;
    }
    void OnHighRefreshRateChange(bool value)
    {
        GameManager.Instance.SetHighRefreshRate(value);
        SaveDataManager.preferencesData.doHighRefreshRate = value;
    }
    void OnShowAimButtonsChange(bool value)
    {
        TouchScreenUIHandler.Instance?.ToggleAimButtons(value);
        SaveDataManager.preferencesData.doShowAimButtons = value;
    }

    #region UI Callback

    public void OnGameSpeed(float value)
    {
        AudioManager.Instance.PlaySound(AudioManager.Instance.UISelectSFX);
        OnGameSpeedChange(value);
    }

    public void OnBGMVolume(float value)
    {
        AudioManager.Instance.PlaySound(AudioManager.Instance.UISelectSFX);
        OnBGMVolumeChange(value);
    }
    
    public void OnSFXVolume(float value)
    {
        AudioManager.Instance.PlaySound(AudioManager.Instance.UISelectSFX);
        OnSFXVolumeChange(value);
    }

    public void OnGraphics(float value)
    {
        AudioManager.Instance.PlaySound(AudioManager.Instance.UISelectSFX);
        OnGraphicsChange(value);
    }

    public void OnHighRefreshRate(bool value)
    {
        OnHighRefreshRateChange(value);
    }
    
    public void OnShowAimButtons(bool value)
    {
        OnShowAimButtonsChange(value);
    }

    public void OnBackButton()
    {
        SaveDataManager.Save();
        SetActive(false);

        previousMenu?.SetActive(true);
    }

    #endregion
}
