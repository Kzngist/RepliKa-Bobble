using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class GamePlayUIHandler : MonoBehaviour
{
    [SerializeField] PauseMenuUIHandler pauseMenu;
    [SerializeField] TMP_Text scoreText1;
    [SerializeField] TMP_Text scoreText2;
    [SerializeField] GameObject newRecordTagGameObject;
    [SerializeField] TMP_Text levelNameText;

    void Start()
    {
        ToggleNewRecordTag(false);
    }

    public void OnMenuButton()
    {
        pauseMenu.SetActive(true);
        GameManager.Instance.TogglePause(true);
    }

    internal void SetScoreText1(string text)
    {
        if (scoreText1 == null) return;

        scoreText1.text = text;
    }
    
    internal void SetScoreText2(string text)
    {
        scoreText2.text = text;
    }

    internal void ToggleNewRecordTag(bool toggle)
    {
        newRecordTagGameObject.SetActive(toggle);
    }

    internal void SetLevelNameText(string text)
    {
        levelNameText.text = "Level " + text;
    }
}
