using System.Collections;
using System.IO;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ScorePopUp : MonoBehaviour
{
    [SerializeField] TMP_Text scoreText;

    [SerializeField] float movementAmount = 2;
    [SerializeField] float duration = 1f;
    
    internal IEnumerator PopScore(int value)
    {
        transform.rotation = GameManager.Instance.mainCamera.transform.rotation;
        scoreText.text = value.ToString();
        scoreText.enabled = true;
        yield return transform.DOMove(movementAmount * Vector3.up, duration).SetRelative(true).WaitForCompletion();
        Destroy(gameObject);
    }
}
