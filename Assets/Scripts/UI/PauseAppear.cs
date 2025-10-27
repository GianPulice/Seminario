
using UnityEngine;
using UnityEngine.Events;

public class PauseAppear : MonoBehaviour
{
    [Header("Animación")]
    [SerializeField] private float animationDuration = 1.2f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutBack;

    [Header("Eventos")]
    public UnityEvent OnAnimateInComplete = new UnityEvent();
    public UnityEvent OnAnimateOutComplete = new UnityEvent();

    [SerializeField] private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private bool isAnimating = false;
    private bool hasInitialized = false;
    public bool IsAnimating => isAnimating;
    private void Awake()
    {
        InitializeIfNeeded();
    }
    public void AnimateIn()
    {
        InitializeIfNeeded();

        if (isAnimating) return;
        isAnimating = true;

        gameObject.SetActive(true);

        rectTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;


        LeanTween.cancel(gameObject);

        LeanTween.scale(gameObject, Vector3.one, animationDuration)
                .setEase(easeType)
                .setIgnoreTimeScale(true)
                .setOnComplete(OnInComplete);

        LeanTween.alphaCanvas(canvasGroup, 1f, animationDuration)
               .setEase(easeType)
               .setIgnoreTimeScale(true);
    }
    public void AnimateOut()
    {
        InitializeIfNeeded();

        if (isAnimating) return;
        isAnimating = true;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        LeanTween.cancel(gameObject);

        LeanTween.scale(gameObject, Vector3.zero, animationDuration)
              .setEase(easeType)
              .setIgnoreTimeScale(true)
              .setOnComplete(OnOutComplete);

        LeanTween.alphaCanvas(canvasGroup, 0f, animationDuration)
              .setEase(easeType)
              .setIgnoreTimeScale(true);
    }
    private void OnInComplete()
    {
        isAnimating = false;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        OnAnimateInComplete.Invoke();
    }
    private void OnOutComplete()
    {
        isAnimating = false;
        gameObject.SetActive(false);
        rectTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        OnAnimateOutComplete.Invoke();
    }
    private void InitializeIfNeeded()
    {
        if (hasInitialized) return;

        canvasGroup = GetComponentInParent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        // Poner el estado inicial oculto
        rectTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);

        hasInitialized = true;
    }
}
