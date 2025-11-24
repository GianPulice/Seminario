using TMPro;
using UnityEngine;

public class InteractionOnActive : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    [Header("Animation positions")]
    [SerializeField] private Vector2 hiddenPosition = new Vector2(400, -600);
    [SerializeField] private Vector2 showPosition = new Vector2(400, -400);

    [Header("Animation Timings")]
    [SerializeField][Range(0, 1)] private float showTime = 0.5f;
    [SerializeField][Range(0, 1)] private float hideTime = 0.25f;
    [SerializeField] private LeanTweenType showEase = LeanTweenType.easeOutBack;
    [SerializeField] private LeanTweenType hideEase = LeanTweenType.easeInBack;

    [Header("Idle Settings")]
    [Tooltip("Qué tanto crece (1.05 = 5%)")]
    [SerializeField] private float idleScaleAmount = 1.05f;
    [Tooltip("Duración de un ciclo de \"respiración\"")]
    [SerializeField] private float idleTime = 1.0f;
    [SerializeField] private LeanTweenType idleEase = LeanTweenType.easeInOutSine;

    private bool isShown = false;
    private Vector3 initialScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (messageText == null)
        {
            messageText = GetComponentInChildren<TextMeshProUGUI>();
        }
        initialScale = rectTransform.localScale;

        ForceHideState();
    }
    private void OnDisable()
    {
        isShown = false;
        LeanTween.cancel(gameObject);
        rectTransform.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
        rectTransform.localScale = initialScale;

    }
    private void ForceHideState()
    {
        rectTransform.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
        isShown = false;
    }
    public void Show(string message)
    {
        messageText.text = message;

        if (isShown) return;
        isShown = true;

        LeanTween.cancel(gameObject);

        gameObject.SetActive(true);

        rectTransform.localScale = initialScale;

        LeanTween.alphaCanvas(canvasGroup, 1f, showTime)
            .setEase(showEase)
            .setIgnoreTimeScale(true);
        LeanTween.move(rectTransform, showPosition, showTime)
            .setEase(showEase)
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                StartIdleAnimation();
            });
    }

    public void Hide()
    {
        isShown = false;

        LeanTween.cancel(gameObject);

        rectTransform.localScale = initialScale;

        if (!gameObject.activeInHierarchy) return;

        LeanTween.move(rectTransform, hiddenPosition, hideTime)
            .setEase(hideEase)
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                messageText.text = string.Empty;
                gameObject.SetActive(false);
            });
        LeanTween.alphaCanvas(canvasGroup, 0f, hideTime / 2)
            .setEase(hideEase)
            .setIgnoreTimeScale(true);
    }
    public void HideInstantly()
    {
        if (!isShown && !gameObject.activeSelf) return;

        LeanTween.cancel(gameObject);
        messageText.text = string.Empty;
        ForceHideState();
    }
    private void StartIdleAnimation()
    {
        if (!isShown) return;
        LeanTween.scale(rectTransform, initialScale * idleScaleAmount, idleTime)
             .setEase(idleEase)
             .setLoopPingPong()
             .setIgnoreTimeScale(true);
    }
}
