
using UnityEngine;
using UnityEngine.Events;

public class PauseAppear : MonoBehaviour
{
    [Header("Animación")]
    [SerializeField] private float animationDuration = 1.2f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutBack;

    [Header("Eventos")]
    [Tooltip("Se dispara cuando la animación de entrada (aparecer) ha terminado.")]
    public UnityEvent OnAnimateInComplete = new UnityEvent();
    [Tooltip("Se dispara cuando la animación de salida (desaparecer) ha terminado.")]
    public UnityEvent OnAnimateOutComplete = new UnityEvent();

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private bool isAnimating = false;
     public bool IsAnimating => isAnimating;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        rectTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
    public void AnimateIn()
    {
        LeanTween.cancel(gameObject);
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
        LeanTween.cancel(gameObject);

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
        OnAnimateOutComplete.Invoke();
    }
}
