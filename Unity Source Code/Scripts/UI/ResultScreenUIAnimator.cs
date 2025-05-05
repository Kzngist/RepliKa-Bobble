using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultScreenUIAnimator : MonoBehaviour
{
    internal static ResultScreenUIAnimator Instance;
    
    [SerializeField] Image panelBackground;
    [SerializeField] TMP_Text resultText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] GameObject newRecordTagGameObject;
    [SerializeField] Button quitButton;
    [SerializeField] Button retryButton;
    [SerializeField] Button nextButton;
    CanvasGroup canvasGroup;
    
    [SerializeField] float fadeDuration = 0.5f;
    
    [SerializeField] float resultTextAnimationDuration = 0.3f;
    [SerializeField] float resultTextHighlightDuration = 0.6f;
    [SerializeField] float resultTextVerticalMovement = 256f;
    [SerializeField] float resultTextScaleFactor = 0.2f;
    
    [SerializeField] float actionButtonAnimationDuration = 0.2f;
    [SerializeField] float actionButtonScaleFactor = 0.1f;
    
    [Space]
    [Header("Audio")]
    [SerializeField] AudioClipProfile winningSFX;
    [SerializeField] AudioClipProfile losingSFX;
    
    void Awake()
    {
        if (Instance != null)
        {
            DebugConsole.Instance.Log("<ResultScreenUIHandler> already exists!", LogType.Warning);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        
        gameObject.SetActive(false);
        SetScore(0);
        ToggleNewRecordTag(false);
    }

    internal void SetScore(int score)
    {
        scoreText.text = $"{"SCORE: ",7}{score}";
    }

    internal void ToggleNewRecordTag(bool toggle)
    {
        newRecordTagGameObject.SetActive(toggle);
    }

    internal IEnumerator ShowResult(bool isWin)
    {
        resultText.text = isWin ? "CONGRATULATIONS" : "GAME OVER";

        AudioManager.Instance.PlaySound(isWin ? winningSFX : losingSFX);

        switch (LevelSelectionManager.Instance.currentGameMode)
        {
            case GameMode.Adventure:
                // activate next button only if not at the end of the level list and next level is unlocked
                if (!LevelSelectionManager.Instance.HasNextLevel() || !LevelSelectionManager.Instance.IsNextLevelUnlocked())
                {
                    nextButton.interactable = false;
                }
                break;
            case GameMode.Arcade:
                if (!isWin || !LevelSelectionManager.Instance.HasNextLevel())
                {
                    nextButton.interactable = false;
                }
                break;
        }
        
        // animate score text
        scoreText?.transform.Rotate(new Vector3(0, 0, -1), Space.Self);
        scoreText?.transform.DOLocalRotate(new Vector3(0, 0, 2), 1f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        
        // fade in panel
        gameObject.SetActive(true);
        canvasGroup.alpha = 0;
        yield return canvasGroup.DOFade(1f, fadeDuration).SetUpdate(true).WaitForCompletion();
        
        // result text animation
        yield return resultText.transform.DOScale(resultTextScaleFactor, resultTextAnimationDuration).SetRelative().SetUpdate(true).WaitForCompletion();
        yield return new WaitForSecondsRealtime(resultTextHighlightDuration);
        yield return resultText.transform.DOLocalMoveY(resultTextVerticalMovement, resultTextAnimationDuration).SetRelative().SetUpdate(true).WaitForCompletion();
        
        // button animations
        quitButton.transform.DOScale(actionButtonScaleFactor, actionButtonAnimationDuration).SetRelative().SetUpdate(true);
        retryButton?.transform.DOScale(actionButtonScaleFactor, actionButtonAnimationDuration).SetRelative().SetUpdate(true);
        yield return nextButton.transform.DOScale(actionButtonScaleFactor, actionButtonAnimationDuration).SetRelative().SetUpdate(true).WaitForCompletion();
    }
}
