using UnityEngine;
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
 
}
