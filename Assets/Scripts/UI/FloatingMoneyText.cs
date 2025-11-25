using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloatingMoneyText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textAmount;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private LeanTweenType moveXEase;
    [SerializeField] private LeanTweenType moveYEase;
    [SerializeField] private LeanTweenType scaleEase;
    [SerializeField] private float targetX = 150.4f;
    [SerializeField] private float targetScale = 0.125f;

    private float maxTimeToReturnObjectToPool = 1f;
    public float MaxTimeToReturnObjectToPool { get => maxTimeToReturnObjectToPool; }
    void Awake()
    {
        FetchComponents();
        ResetVisualState();
    }
    private void OnEnable()
    {
        Animate();
    }
    void OnDisable()
    {
        LeanTween.cancel(gameObject);
    }
    public void Initialize(string _text, Color _color)
    {
        if (textAmount == null) FetchComponents();

        textAmount.text = _text;
        textAmount.color = _color;
        
        textAmount.ForceMeshUpdate();
    }

    private void Animate()
    {
        // Animación Y
        LeanTween.moveLocalY(gameObject, 50f, maxTimeToReturnObjectToPool)
            .setEase(moveYEase);

        // Animación X
        LeanTween.moveLocalX(gameObject, targetX, maxTimeToReturnObjectToPool)
            .setEase(moveXEase);

        // Animación Escala
        LeanTween.scale(gameObject, new Vector3(targetScale, targetScale, targetScale), maxTimeToReturnObjectToPool)
            .setEase(scaleEase);

        // Animación Alpha (Fade out)
        if (canvasGroup != null)
        {
            LeanTween.alphaCanvas(canvasGroup, 0f, 0.3f)
                     .setDelay(maxTimeToReturnObjectToPool - 0.3f);
        }
    }
    private void ResetVisualState()
    {
        transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
        transform.localPosition = Vector3.zero;
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }
    private void FetchComponents()
    {
        if (textAmount == null) textAmount = GetComponentInChildren<TextMeshProUGUI>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }
   
}
