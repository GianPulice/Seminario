using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CloseButton : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler, IPointerUpHandler
{
    [Header("Animación")]
    [SerializeField] private float hoverScale = 1.15f; 
    [SerializeField] private float pressScale = 0.9f; 
    [SerializeField] private float animationDuration = 0.1f; 
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutQuad;

    [Header("Acción")]
    [Tooltip("El evento que se dispara cuando se hace clic en el botón.")]
    [SerializeField] private UnityEvent onClick;

    private Vector3 originalScale;
    private bool isPressed = false; // Para saber si el botón está presionado
    private bool isHovering = false; // Para saber si el mouse está encima
    public UnityEvent OnClickEvent => onClick;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnDisable()
    {
        LeanTween.cancel(gameObject);
        transform.localScale = originalScale;
        isPressed = false;
        isHovering = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        // No animar si ya está presionado
        if (!isPressed)
        {
            AnimateScale(originalScale * hoverScale);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        // No animar si ya está presionado
        if (!isPressed)
        {
            AnimateScale(originalScale);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        AnimateScale(originalScale * pressScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;

        if (isHovering)
        {
             AnimateScale(originalScale * hoverScale);

            Debug.Log("Button Clicked - Invoking Event");
            onClick?.Invoke();
        }
        else
        {
           AnimateScale(originalScale);
        }
    }

    private void AnimateScale(Vector3 targetScale)
    {
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, targetScale, animationDuration)
            .setEase(easeType)
            .setIgnoreTimeScale(true);
    }
}
