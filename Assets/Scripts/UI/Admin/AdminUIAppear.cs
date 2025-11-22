
using UnityEngine;
using UnityEngine.Events;

public class AdminUIAppear : MonoBehaviour
{
    [Header("Animación - Tiempos")]
    [SerializeField] private float showTime = 0.5f;
    [SerializeField] private float hideTime = 0.3f;

    [Header("Animación - Posiciones")]
    [SerializeField] private Vector2 shownPosition = Vector2.zero;
    [SerializeField] private Vector2 hiddenPosition = new Vector2(0, -1200);

    [Header("Animación - Easing")]
    [SerializeField] private LeanTweenType showEase = LeanTweenType.easeOutBack;
    [SerializeField] private LeanTweenType hideEase = LeanTweenType.easeInQuad;

    [Header("Eventos")]
    public UnityEvent OnAnimateInComplete = new UnityEvent();
    public UnityEvent OnAnimateOutComplete = new UnityEvent();
    public UnityEvent OnAnimateInStart;
    public UnityEvent OnAnimateOutStart;

    [Header("Referencias (Opcional)")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;

    public bool IsVisible => gameObject.activeSelf && canvasGroup.alpha > 0.01f;

    private void Awake()
    {
        InitializeIfNeeded();
    }

    public void AnimateIn()
    {
        InitializeIfNeeded();

        LeanTween.cancel(gameObject);

        OnAnimateInStart?.Invoke();

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            rectTransform.anchoredPosition = hiddenPosition;
            canvasGroup.alpha = 0f;
        }

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        LeanTween.alphaCanvas(canvasGroup, 1f, showTime)
            .setEase(showEase)
            .setIgnoreTimeScale(true);

        LeanTween.move(rectTransform, shownPosition, showTime)
            .setEase(showEase)
            .setIgnoreTimeScale(true)
            .setOnComplete(OnInComplete);
    }
    public void AnimateOut()
    {
        InitializeIfNeeded();

        LeanTween.cancel(gameObject);

        OnAnimateOutStart?.Invoke();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        LeanTween.alphaCanvas(canvasGroup, 0f, hideTime)
            .setEase(hideEase)
            .setIgnoreTimeScale(true);

        LeanTween.move(rectTransform, hiddenPosition, hideTime)
            .setEase(hideEase)
            .setIgnoreTimeScale(true)
            .setOnComplete(OnOutComplete);
    }

    private void OnInComplete()
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        OnAnimateInComplete.Invoke();
    }

    private void OnOutComplete()
    {
        gameObject.SetActive(false);
        rectTransform.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
        OnAnimateOutComplete.Invoke();
    }
    private void InitializeIfNeeded()
    {
        if (rectTransform != null && canvasGroup != null) return;

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Estado inicial forzado
        rectTransform.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

}
