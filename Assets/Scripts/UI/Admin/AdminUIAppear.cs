
using UnityEngine;
using UnityEngine.Events;

public class AdminUIAppear : MonoBehaviour
{
    [Header("Animación - Tiempos")]
    [SerializeField] private float showTime = 0.5f;
    [SerializeField] private float hideTime = 0.3f;

    [Header("Animación - Posiciones")]
    [Tooltip("Posición del panel cuando está visible (normalmente 0,0)")]
    [SerializeField] private Vector2 shownPosition = Vector2.zero;
    [Tooltip("Posición del panel cuando está oculto (ej: 0, -1200)")]
    [SerializeField] private Vector2 hiddenPosition = new Vector2(0, -1200);

    [Header("Animación - Easing")]
    [SerializeField] private LeanTweenType showEase = LeanTweenType.easeOutBack;
    [SerializeField] private LeanTweenType hideEase = LeanTweenType.easeInQuad;
    [SerializeField] private AnimationCurve showCurve;
    [SerializeField] private AnimationCurve hideCurve;

    [Header("Eventos")]
    [Tooltip("Se dispara cuando la animación de entrada (aparecer) ha terminado.")]
    public UnityEvent OnAnimateInComplete = new UnityEvent();
    [Tooltip("Se dispara cuando la animación de salida (desaparecer) ha terminado.")]
    public UnityEvent OnAnimateOutComplete = new UnityEvent();

    [Header("Referencias (Opcional)")]
    [Tooltip("El CanvasGroup a animar. Si es nulo, lo buscará en este GameObject.")]
    [SerializeField] private CanvasGroup canvasGroup;
    [Tooltip("El RectTransform a mover. Si es nulo, lo buscará en este GameObject.")]
    [SerializeField] private RectTransform rectTransform;

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

        LeanTween.cancel(gameObject);

        rectTransform.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        gameObject.SetActive(true);

      
        LeanTween.move(rectTransform, shownPosition, showTime)
            .setEase(showEase)
            .setIgnoreTimeScale(true)
            .setOnComplete(OnInComplete);

 
        LeanTween.alphaCanvas(canvasGroup, 1f, showTime)
            .setEase(showEase) 
            .setIgnoreTimeScale(true);
    }
    public void AnimateOut()
    {
        InitializeIfNeeded();
        if (isAnimating) return;
        isAnimating = true;

        LeanTween.cancel(gameObject);

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

  
        LeanTween.move(rectTransform, hiddenPosition, hideTime)
            .setEase(hideEase)
            .setIgnoreTimeScale(true)
            .setOnComplete(OnOutComplete);

        LeanTween.alphaCanvas(canvasGroup, 0f, hideTime)
            .setEase(hideEase)
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
        rectTransform.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
        OnAnimateOutComplete.Invoke();
    }
    private void InitializeIfNeeded()
    {
        if(hasInitialized) return;
        if (rectTransform == null) 
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (canvasGroup == null) 
        {
            canvasGroup = GetComponentInParent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (rectTransform == null)
        {
            Debug.LogError("AdminUIAppear no pudo encontrar un RectTransform.", this);
            return; 
        }

        // Poner el estado inicial
        rectTransform.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);

        hasInitialized = true;
    }

}
