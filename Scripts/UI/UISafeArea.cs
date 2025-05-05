using UnityEngine;

public class UISafeArea : MonoBehaviour
{
    RectTransform rectTransform;
    [SerializeField] [Range(0, 1)] float strengthX = 1;
    [SerializeField] [Range(0, 1)] float strengthY = 1;

    void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();

        UpdatePosition();
    }

    void UpdatePosition()
    {
        Rect safeArea = Screen.safeArea;
        if (Mathf.Approximately(safeArea.width, Screen.width) && Mathf.Approximately(safeArea.height, Screen.height)) return;
        
        rectTransform.anchorMin = new Vector2(safeArea.xMin / (Screen.currentResolution.width - 1) * strengthX, safeArea.yMin / (Screen.currentResolution.height - 1) * strengthY);
        rectTransform.anchorMax = Vector2.one - new Vector2((1 - safeArea.xMax / (Screen.currentResolution.width - 1)) * strengthX, (1 - safeArea.yMax / (Screen.currentResolution.height - 1)) * strengthY);
    }
}
