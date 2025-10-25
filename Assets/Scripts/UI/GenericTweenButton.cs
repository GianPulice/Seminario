using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum ButtonBehavior
{
    Bounce,
    ToggleSelect,
}

public class GenericTweenButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler
{
    [Header("Comportamiento")]
    [Tooltip("Define cómo reacciona el botón al hacer click.")]
    [SerializeField] private ButtonBehavior behavior = ButtonBehavior.Bounce;

    [Tooltip("El RectTransform que se animará. Si es nulo, usará el de este GameObject.")]
    [SerializeField] private RectTransform targetTransform;

    [Header("Configuración de Animación")]
    [Tooltip("Tiempo que tardan las animaciones (en segundos).")]
    [SerializeField][Range(0.1f,1)] private float animTime = 0.15f;
    [Tooltip("La escala cuando el cursor está encima (ej: 1.1 = 110%).")]
    [SerializeField] private float hoverScale = 1.1f;
    [Tooltip("La escala cuando se presiona el botón (ej: 0.9 = 90%).")]
    [SerializeField] private float pressScale = 0.9f;
    [Tooltip("La escala cuando está en modo 'Toggle' y seleccionado (ej: 1.2 = 120%).")]
    [SerializeField] private float selectedScale = 1.2f;
    [Tooltip("El tipo de 'easing' para las animaciones.")]
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutQuad;

    // --- UnityEvents ---
    [Header("Eventos del Botón")]
    [Tooltip("Se dispara al hacer click.")]
    public UnityEvent OnClick = new UnityEvent();
    [Tooltip("Se dispara cuando el cursor entra en el área.")]
    public UnityEvent OnPointerEnterEvent = new UnityEvent();
    [Tooltip("Se dispara cuando el cursor sale del área.")]
    public UnityEvent OnPointerExitEvent = new UnityEvent();

    // --- Variables de Estado ---
    private Vector3 initialScale;
    private bool isSelected = false;  // Solo se usa para el modo ToggleSelect
    private bool isPointerOver = false; // ¿Está el cursor encima?
    private bool isPointerDown = false; // ¿Está el botón presionado?

    private int currentTweenId = -1; // Para cancelar animaciones anteriores

    private void Awake()
    {
        if (targetTransform == null)
        {
            targetTransform = GetComponent<RectTransform>();
        }
        initialScale = targetTransform.localScale;
    }
    private void OnDisable()
    {
        // Resetea el estado si el botón se desactiva
        if (currentTweenId != -1)
        {
            LeanTween.cancel(targetTransform.gameObject, currentTweenId);
        }
        targetTransform.localScale = initialScale;
        isPointerOver = false;
        isPointerDown = false;
        isSelected = false; 
    }
        public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        AnimateButton();
        OnPointerEnterEvent.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        isPointerDown = false;
        AnimateButton();
        OnPointerExitEvent.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        AnimateButton();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        AnimateButton();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (behavior == ButtonBehavior.ToggleSelect)
        {
            isSelected = !isSelected;
            AnimateButton();
        }
        OnClick?.Invoke();
    }

    private void AnimateButton()
    {
        if (currentTweenId != -1)
        {
            LeanTween.cancel(targetTransform.gameObject, currentTweenId);
        }

        Vector3 targetScale = GetTargetScale();

        currentTweenId = LeanTween.scale(targetTransform, targetScale, animTime)
            .setEase(easeType)
            .setIgnoreTimeScale(true)
            .id;
    }

    private Vector3 GetTargetScale()
    {
        if (isPointerDown)
        {
            return initialScale * pressScale;
        }
        if (behavior == ButtonBehavior.ToggleSelect)
        {
            if (isSelected)
            {
                if (isPointerOver)
                {
                    return initialScale * selectedScale * 1.05f; // ej. 1.2 * 1.05
                }
                return initialScale * selectedScale;
            }
        }
        if (isPointerOver)
        {
            return initialScale * hoverScale;
        }
        return initialScale;
    }

    public void SetSelected(bool selected)
    {
        if (behavior != ButtonBehavior.ToggleSelect) return;

        if (isSelected == selected) return; // No hacer nada si ya está en ese estado

        isSelected = selected;
        AnimateButton();

        if (selected)
        {
            OnPointerEnterEvent.Invoke();
        }
        else
        {
            OnPointerExitEvent.Invoke();
        }
    }
}
