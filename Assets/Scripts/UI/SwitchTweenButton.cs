using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SwitchTweenButton : GenericTweenButton
{
    [SerializeField] private RectTransform handleRect;
    [SerializeField] private RectTransform bgRect;
    private Image bgImage;

    [Header("Off")]
    [SerializeField] private Vector2 handleOffPosition = new Vector2(-50,0);
    [SerializeField] private Color handleOffColor = Color.red;
    [Header("On")]
    [SerializeField] private Vector2 handleOnPosition= new Vector2(50,0);
    [SerializeField] private Color handleOnColor = Color.green;
    [Header("EaseType")]
    [SerializeField] private LeanTweenType handleEaseType = LeanTweenType.easeOutCirc;

    public Func<bool> OnTryEnableCondition;
    public bool IsLocked { get;private set; } = false;
    protected override void Awake()
    {
        base.Awake();
        behavior = ButtonBehavior.ToggleSelect;
        targetImage = null;
        if (bgRect != null)
        {
            bgImage = bgRect.GetComponent<Image>();
        }
        if (handleRect != null)
        {
            handleRect.anchoredPosition = isSelected ? handleOnPosition : handleOffPosition;
        }
        if (bgImage != null)
        {
            bgImage.color = isSelected ? handleOnColor : handleOffColor;
        }
    }
    protected override void UpdateVisuals()
    {
        base.UpdateVisuals();
        if (handleRect != null)
        {
            Vector2 targetPosition = isSelected ? handleOnPosition : handleOffPosition;
            LeanTween.move(handleRect, targetPosition, animTime)
                .setEase(handleEaseType)
                .setIgnoreTimeScale(true);
        }
        if (bgImage != null)
        {
            Color targetColor = isSelected ? handleOnColor : handleOffColor;

            LeanTween.value(bgImage.gameObject, bgImage.color, targetColor, animTime)
                .setEase(handleEaseType)
                .setIgnoreTimeScale(true)
                .setOnUpdate((Color val) =>
                {
                    if (bgImage != null) bgImage.color = val;
                });
        }
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (IsLocked)
        {
            PlayRejectAnimation();
            AudioManager.Instance?.PlayOneShotSFX("ButtonClickWrong");
            MessagePopUp.Show("Tavern is closed. You need to go to Bed");
            return;
        }

        if (isSelected && OnTryEnableCondition != null && !OnTryEnableCondition.Invoke())
        {
            PlayRejectAnimation();
            AudioManager.Instance?.PlayOneShotSFX("ButtonClickWrong");
            MessagePopUp.Show("Can't close until 24:00");
            return;
        }
        base.OnPointerClick(eventData);
    }
    public void LockSwitch()
    {
        IsLocked = true;
        SetInteractable(false);
    }
    public void UnlockSwitch()
    {
        IsLocked = false;
        SetInteractable(true);
    }
    private void PlayRejectAnimation()
    {
        if (handleRect == null) return;

        LeanTween.cancel(handleRect);

        Vector2 originalPos = handleRect.anchoredPosition;

        float shakeDistance = 12f; // cuánto se mueve
        float shakeTime = 0.25f;

        LeanTween.move(handleRect, originalPos + Vector2.right * shakeDistance, shakeTime * 0.3f)
            .setEase(LeanTweenType.easeOutCubic)
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                LeanTween.move(handleRect, originalPos, shakeTime * 0.7f)
                    .setEase(LeanTweenType.easeOutElastic)
                    .setIgnoreTimeScale(true);
            });
    }
}
