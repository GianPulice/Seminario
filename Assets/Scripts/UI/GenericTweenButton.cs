using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ButtonBehavior
{
    Bounce,
    ToggleSelect,
    HoverOnly
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

    [Tooltip("La Imagen a la que se le cambiará el color. Si es nulo, buscará una en este GameObject.")]
    [SerializeField] private Image targetImage;

    [Header("Configuración de Animación")]
    [Tooltip("Tiempo que tardan las animaciones (en segundos).")]
    [SerializeField][Range(0.1f, 1)] private float animTime = 0.15f;
    [Tooltip("La escala cuando el cursor está encima (ej: 1.1 = 110%).")]
    [SerializeField] private float hoverScale = 1.1f;
    [Tooltip("La escala cuando se presiona el botón (ej: 0.9 = 90%).")]
    [SerializeField] private float pressScale = 0.9f;
    [Tooltip("La escala cuando está en modo 'Toggle' y seleccionado (ej: 1.2 = 120%).")]
    [SerializeField] private float selectedScale = 1.2f;
    [Tooltip("El tipo de 'easing' para las animaciones.")]
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutQuad;

    [Header("Configuración de Color (Toggle)")]
    [Tooltip("Color del botón cuando el cursor está encima.")]
    [SerializeField] private Color hoverColor = new Color(0.95f, 0.95f, 0.95f, 1f);
    [Tooltip("Color del botón cuando está en modo 'Toggle' y seleccionado.")]
    [SerializeField] private Color selectedColor = new Color(1f, 0.9f, 0.5f);
    private Color normalColor;

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

    private static GenericTweenButton currentHoverOnlyButton;
    private void Awake()
    {
        if (targetTransform == null)
        {
            targetTransform = GetComponent<RectTransform>();
        }

        if (targetImage == null)
        {
            targetImage = targetTransform.GetComponent<Image>();
        }

        initialScale = targetTransform.localScale;

        if (targetImage != null)
        {
            normalColor = targetImage.color;
        }

    }
    private void OnDisable()
    {
        LeanTween.cancel(targetTransform.gameObject);

        targetTransform.localScale = initialScale;
        if (targetImage != null)
        {
            targetImage.color = normalColor;
        }
        if (currentHoverOnlyButton == this)
            currentHoverOnlyButton = null;

        isPointerOver = false;
        isPointerDown = false;
        isSelected = false;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;

        if (behavior == ButtonBehavior.HoverOnly)
        {
            GenericTweenButton oldButton = currentHoverOnlyButton;

            currentHoverOnlyButton = this;

            if (oldButton != null && oldButton != this)
            {
                oldButton.UpdateVisuals();
            }
        }
        UpdateVisuals();
        OnPointerEnterEvent.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        UpdateVisuals();
        OnPointerExitEvent.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (behavior == ButtonBehavior.HoverOnly) return;
        isPointerDown = true;
        UpdateVisuals();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (behavior == ButtonBehavior.HoverOnly) return;
        isPointerDown = false;
        UpdateVisuals();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (behavior == ButtonBehavior.HoverOnly) return;
        if (behavior == ButtonBehavior.ToggleSelect)
        {
            isSelected = !isSelected;
        }
        if (behavior == ButtonBehavior.ToggleSelect)
        {
            UpdateVisuals();
        }
        OnClick?.Invoke();
    }

    private void UpdateVisuals()
    {
        LeanTween.cancel(targetTransform.gameObject);

        Vector3 targetScale = GetTargetScale();

        currentTweenId = LeanTween.scale(targetTransform, targetScale, animTime)
            .setEase(easeType)
            .setIgnoreTimeScale(true)
            .id;
        if (targetImage != null)
        {
            Color targetColor = GetTargetColor();
            LeanTween.color(targetImage.rectTransform, targetColor, animTime)
                .setEase(easeType)
                .setIgnoreTimeScale(true);
        }

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

        // Estado normal
        return initialScale;
    }

    private Color GetTargetColor()
    {
        if (behavior == ButtonBehavior.ToggleSelect && isSelected)
        {
            return selectedColor;
        }

        if (behavior == ButtonBehavior.HoverOnly && currentHoverOnlyButton == this)
        {
            return hoverColor;
        }
        if (isPointerOver && behavior == ButtonBehavior.ToggleSelect)
        {
            return hoverColor;
        }

        // Estado normal
        return normalColor;
    }

    public void SetSelected(bool selected)
    {
        if (behavior != ButtonBehavior.ToggleSelect) return;

        if (isSelected == selected) return; // No hacer nada si ya está en ese estado

        isSelected = selected;
        UpdateVisuals();

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
