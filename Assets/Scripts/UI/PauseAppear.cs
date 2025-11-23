
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
    public UnityEvent OnAnimateInStart;
    public UnityEvent OnAnimateOutStart;

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
            rectTransform.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;
        }

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        LeanTween.alphaCanvas(canvasGroup, 1f, animationDuration)
               .setEase(easeType)
               .setIgnoreTimeScale(true);

        LeanTween.scale(gameObject, Vector3.one, animationDuration)
                .setEase(easeType)
                .setIgnoreTimeScale(true)
                .setOnComplete(OnInComplete);
    }
    public void AnimateOut()
    {
        InitializeIfNeeded();

        LeanTween.cancel(gameObject);

        if (InteractionManagerUI.Instance != null)
        {
            InteractionManagerUI.Instance.ForceResetUI();
        }

        OnAnimateOutStart?.Invoke();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        LeanTween.alphaCanvas(canvasGroup, 0f, animationDuration)
              .setEase(easeType)
              .setIgnoreTimeScale(true);

        LeanTween.scale(gameObject, Vector3.zero, animationDuration)
              .setEase(easeType)
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
        rectTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        OnAnimateOutComplete.Invoke();
    }
    private void InitializeIfNeeded()
    {
        if (rectTransform != null && canvasGroup != null) return;
        
        canvasGroup = GetComponentInParent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        // Poner el estado inicial oculto
        rectTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}
