using TMPro;
using UnityEngine;

public class InteractionOnActive : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    private RectTransform rectTransform;

    [Header("Animation positions")]
    [SerializeField] private Vector2 hiddenPosition = new Vector2(400, -600);
    [SerializeField] private Vector2 showPosition = new Vector2(400, -400);

    [Header("Animation Timings")]
    [SerializeField][Range(0,1)] private float showTime = 0.5f;
    [SerializeField][Range(0, 1)] private float hideTime = 0.25f;
    [SerializeField] private LeanTweenType showEase = LeanTweenType.easeOutBack;
    [SerializeField] private LeanTweenType hideEase = LeanTweenType.easeInBack;

    [Header("Idle Settings")]
    [Tooltip("Qué tanto crece (1.05 = 5%)")]
    [SerializeField] private float idleScaleAmount = 1.05f;
    [Tooltip("Duración de un ciclo de \"respiración\"")
        ]
    [SerializeField] private float idleTime = 1.0f;
    [SerializeField] private LeanTweenType idleEase = LeanTweenType.easeInOutSine;

    private int currentMoveTweenId = -1;
    private int currentIdleTweenId = -1;
    private bool isShown = false;
    private Vector3 initialScale;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (messageText == null)
        {
            messageText = GetComponentInChildren<TextMeshProUGUI>();

        }
        initialScale = rectTransform.localScale;

        rectTransform.anchoredPosition = hiddenPosition;
        gameObject.SetActive(false);
        isShown = false;
    }

    public void Show(string message)
    {
        messageText.text = message;

        if (isShown) return;
        isShown = true;

        CancelAllTweens();

        gameObject.SetActive(true);

        currentMoveTweenId = LeanTween.move(rectTransform, showPosition, showTime)
            .setEase(showEase)
            .setOnComplete(() =>
            {
                // Cuando termina de moverse, resetea el ID y empieza el "idle"
                currentMoveTweenId = -1;
                StartIdleAnimation();
            })
            .id;

    }

    public void Hide()
    {
        if (!isShown)
        {
            return;
        }

        isShown = false;

        CancelAllTweens();

        // Resetea la escala a 1 por si lo interrumpimos a mitad del "idle"
        rectTransform.localScale = initialScale;

        // Si el objeto no está activo, no puede animarse, así que salimos.
        if (!gameObject.activeInHierarchy) return;

        currentMoveTweenId = LeanTween.move(rectTransform, hiddenPosition, hideTime)
            .setEase(hideEase)
            .setOnComplete(() =>
            {
                currentMoveTweenId = -1;
                messageText.text = string.Empty;
                gameObject.SetActive(false);
            })
            .id;

    }

    private void StartIdleAnimation()
    {
        // Comprobación: Si Hide() fue llamado MIENTRAS Show() se animaba, no debemos empezar el idle.
        if (!isShown) return;

        rectTransform.localScale = initialScale;
        Vector3 targetIdleScale = initialScale * idleScaleAmount;

        currentIdleTweenId = LeanTween.scale(rectTransform, targetIdleScale, idleTime)
            .setEase(idleEase)
            .setLoopPingPong() // Causa que vaya de 1 a 1.05 y vuelva a 1, infinitamente
            .id;
    }
    private void CancelAllTweens()
    {
        if (currentMoveTweenId != -1)
        {
            LeanTween.cancel(currentMoveTweenId);
            currentMoveTweenId = -1;
        }
        if (currentIdleTweenId != -1)
        {
            LeanTween.cancel(currentIdleTweenId);
            currentIdleTweenId = -1;
        }
    }
}
