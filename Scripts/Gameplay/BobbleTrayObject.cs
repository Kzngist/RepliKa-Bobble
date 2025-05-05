using System.Collections;
using TMPro;
using UnityEngine;

public enum BobbleTrayObjectType
{
    Bobble,
    Ceiling,
    Wall,
    Other
}

[SelectionBase]
public class BobbleTrayObject : MonoBehaviour
{
    [SerializeField] internal BobbleTrayObjectType type;
    [SerializeField] internal BobbleTrayObjectVariant variant;
    [SerializeField] internal Coordinates coordinates;
    [SerializeField] internal Direction orientation;
    
    [SerializeField] Canvas canvas;
    [SerializeField] TMP_Text coordinatesText;

    IEnumerator Start()
    {
        // wait for other objects to finish instantiation, in case multiple objects are generated in the same frame
        yield return null;
        
        SetUp();
    }

    internal virtual void SetUp()
    {
        UpdateCoordinates();
        UpdateOrientation();
        UpdateCanvas(DebugWorld.Instance.isEnabled);
    }

    void UpdateCoordinates()
    {
        coordinates = Coordinates.FromPosition(transform.localPosition);
    }

    void UpdateOrientation()
    {
        orientation = transform.localRotation.ToDirection();
    }

    void UpdateCanvas(bool isEnabled)
    {
        canvas.enabled = isEnabled;
        canvas.transform.rotation = GameManager.Instance.mainCamera.transform.rotation;
        coordinatesText.text = coordinates.ToString();
    }
    
    internal virtual BobbleTrayObjectState GetState()
    {
        return new BobbleTrayObjectState(type, coordinates, orientation, variant);
    }

    internal void Destroy()
    {
        Destroy(gameObject);
    }

    void OnEnable()
    {
        DebugWorld.Instance.OnToggleDebug += UpdateCanvas;
    }

    void OnDisable()
    {
        DebugWorld.Instance.OnToggleDebug -= UpdateCanvas;
    }
}
