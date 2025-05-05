using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleUIAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerDownHandler, IPointerClickHandler
{
    [SerializeField] Transform target;
    [SerializeField] float scaleFactor = 1.2f;
    [SerializeField] float verticalMovement = 100f;
    [SerializeField] float animationSpeed = 0.5f;

    [SerializeField] bool doSFXOnSelect = false;
    [SerializeField] AudioClipProfile selectSFXOverride;
    [SerializeField] bool doSFXOnPress = false;
    [SerializeField] AudioClipProfile pressSFXOverride;
    [SerializeField] bool doSFXOnClick = false;
    [SerializeField] AudioClipProfile clickSFXOverride;

    bool hasInitialized; // flag used to counteract translation caused by LayoutGroup before first frame
    float initialScale;
    float initialPositionY;

    void Start()
    {
        if (target == null) target = transform;
        initialScale = target.localScale.x;
        initialPositionY = target.localPosition.y;
        hasInitialized = true;
    }

    void Animate(bool isSelected)
    {
        if (isSelected)
        {
            target.DOLocalMoveY(initialPositionY + verticalMovement, animationSpeed).SetUpdate(true);
            target.DOScale(initialScale * scaleFactor, animationSpeed).SetUpdate(true);
        }
        else
        {
            target.DOLocalMoveY(initialPositionY, animationSpeed).SetUpdate(true);
            target.DOScale(initialScale, animationSpeed).SetUpdate(true);
        }
    }

    // Triggers when the pointer hovers the object.
    public void OnPointerEnter(PointerEventData eventData)
    {
        // make this object selected in event system, bypassing pointer click
        eventData.selectedObject = gameObject;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        eventData.selectedObject = null;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (doSFXOnSelect)
        {
            AudioManager.Instance.PlaySound(selectSFXOverride ? selectSFXOverride : AudioManager.Instance.UISelectSFX);
        }
        Animate(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        Animate(false);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (doSFXOnPress)
        {
            AudioManager.Instance.PlaySound(pressSFXOverride ? pressSFXOverride : AudioManager.Instance.UIClickSFX);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (doSFXOnClick)
        {
            AudioManager.Instance.PlaySound(clickSFXOverride ? clickSFXOverride : AudioManager.Instance.UIClickSFX);
        }
    }

    void OnDisable()
    {
        if (!hasInitialized) return;
        
        DOTween.Kill(target);
        target.localPosition = new Vector3(target.localPosition.x, initialPositionY, target.localPosition.z);
        target.localScale = new Vector3(initialScale, initialScale, initialScale);
    }
}
