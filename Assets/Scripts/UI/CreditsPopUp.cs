
using UnityEngine;

public class CreditsPopUp : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutBounce;
    [SerializeField] private AnimationCurve animationCurve;
    [SerializeField] private float animationDuration = 0.7f;

    [Tooltip("La posición X oculta a la izquierda.")]
    [SerializeField] private float hiddenXPosition = -1500f;

    private Vector2 centerPosition = Vector2.zero;
    private Vector2 hiddenPositionLeft;
    
    private RectTransform rectTransform;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        hiddenPositionLeft = new Vector2(hiddenXPosition, rectTransform.anchoredPosition.y);

        gameObject.SetActive(false);
    }

    public void AnimateIn()
    {
        LeanTween.cancel(gameObject);
        gameObject.SetActive(true);

        rectTransform.anchoredPosition = hiddenPositionLeft;
        transform.localScale = Vector3.zero;

        var scaleTween = LeanTween.scale(gameObject, Vector3.one, animationDuration / 2); // Aparecer rápido
        SetEase(scaleTween);
        
        var moveTween = LeanTween.move(rectTransform, centerPosition, animationDuration)
            .setDelay(0.1f);
        SetEase(moveTween);
    }

    /// <summary>
    /// Animación de Salida: Derecha a Izquierda (de 0 a -1500)
    /// </summary>
    public void AnimateOut()
    {
        LeanTween.cancel(gameObject);

        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        var moveTween = LeanTween.move(rectTransform, hiddenPositionLeft, animationDuration);
        SetEase(moveTween);
       
        var scaleTween = LeanTween.scale(gameObject, Vector3.zero, animationDuration / 2)
            .setDelay(animationDuration / 2); 

        SetEase(scaleTween);

        moveTween.setOnComplete(() => gameObject.SetActive(false));
    }

    private void SetEase(LTDescr tween)
    {
        if (easeType == LeanTweenType.animationCurve && animationCurve != null)
        {
            tween.setEase(animationCurve);
        }
        else
        {
            tween.setEase(easeType);
        }
    }
}
