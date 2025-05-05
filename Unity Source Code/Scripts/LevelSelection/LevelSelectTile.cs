using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelSelectTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] TMP_Text levelNameText;
    [SerializeField] GameObject lockObject;
    [SerializeField] GameObject rimObject;
    [SerializeField] float flipTime = 1f;
    [SerializeField] float shakeIntensity = 10f;
    [SerializeField] int shakeVibrato = 10;
    
    [Space]
    [Header("Audio")]
    [SerializeField] AudioClipProfile flipSFX;
    [SerializeField] AudioClipProfile hoverSFX;
    [SerializeField] AudioClipProfile clickSFX;
    [SerializeField] float flipSFXPitchIncrementPerTile = 0.01f;

    internal LevelProfile levelProfile;
    internal LevelList levelList;
    internal LevelClearedStatus clearedStatus;

    Tween shakeTween;
    bool isReady;

    void Awake()
    {
        lockObject.SetActive(false);
        rimObject.SetActive(false);
        levelNameText.enabled = false;
    }

    internal void Initialize()
    {
        rimObject.SetActive(true);
        levelNameText.text = levelProfile.levelName;

        if (clearedStatus > LevelClearedStatus.Locked)
        {
            StartCoroutine(Flip(Vector3.zero));
        }
        else
        {
            ShowLevelClearedStatus();
        }
    }

    void ShowLevelClearedStatus()
    {
        switch (clearedStatus)
        {
            case LevelClearedStatus.Locked:
                lockObject.SetActive(true);
                levelNameText.enabled = false;
                break;
            case LevelClearedStatus.Unlocked:
                lockObject.SetActive(false);
                levelNameText.enabled = true;
                break;
            case LevelClearedStatus.Cleared:
                lockObject.SetActive(false);
                levelNameText.enabled = true;
                // enable cleared ring
                break;
            case LevelClearedStatus.Mastered:
                lockObject.SetActive(false);
                levelNameText.enabled = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    IEnumerator Flip(Vector3 pivot)
    {
        isReady = false;
        
        float pitchMultiplier = 1 + levelList.levels.IndexOf(levelProfile) * flipSFXPitchIncrementPerTile;
        AudioManager.Instance.PlaySound(flipSFX, transform, Vector3.zero, pitchMultiplier);
        
        float angle = 0;
        Quaternion startRotation = transform.rotation;
        Tween flipTween = DOTween.To(() => angle, x => angle = x, 360f, flipTime).SetEase(Ease.OutCirc);
        while (flipTween.IsActive())
        {
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.Cross(pivot - transform.position, Vector3.back)) * startRotation;
            
            if (angle > 180f)
            {
                ShowLevelClearedStatus();
            }
            
            yield return null;
        }
        
        isReady = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isReady || levelProfile == null || clearedStatus == LevelClearedStatus.Locked) return;
        
        AudioManager.Instance.PlaySound(hoverSFX, transform);
        
        shakeTween = transform.DOShakeRotation(10f, shakeIntensity, shakeVibrato, 90f, false, ShakeRandomnessMode.Harmonic).SetLoops(-1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!shakeTween.IsActive()) return;
        
        shakeTween.Rewind();
        shakeTween.Kill();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (levelProfile == null || clearedStatus == LevelClearedStatus.Locked) return;
        
        AudioManager.Instance.PlaySound(clickSFX, null, transform.position);
        
        LevelSelectionManager.Instance.LoadLevel(levelProfile, levelList);
    }
}
