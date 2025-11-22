using UnityEngine;
using UnityEngine.Events;


public class CookingUIAppear : MonoBehaviour
{
    [Header("Panels to animate")]
    [SerializeField] private GameObject panelRecipes;
    [SerializeField] private GameObject panelIngredients;
    [SerializeField] private GameObject panelCauldron;

    [Header("Panel Contents (CanvasGroups)")]
    [Tooltip("El CanvasGroup de los *contenidos* de Recipes (botones, etc.)")]
    [SerializeField] private CanvasGroup recipesContentGroup;
    [Tooltip("El CanvasGroup de los *contenidos* de Ingredients")]
    [SerializeField] private CanvasGroup ingredientsContentGroup;
    [Tooltip("El CanvasGroup de los *contenidos* de Cauldron")]
    [SerializeField] private CanvasGroup cauldronContentGroup;

    [Header("Timers")]
    [SerializeField][Range(0, 2f)] private float showDuration = 0.25f;
    [SerializeField][Range(0, 2f)] private float hideDuration = 0.3f;
    [SerializeField][Range(0, 1f)] private float contentFadeTime = 0.15f;

    [Header("Easing (Animación)")]
    [SerializeField] private LeanTweenType showEase = LeanTweenType.easeOutBack;
    [SerializeField] private LeanTweenType hideEase = LeanTweenType.easeInQuad;

    [Header("Posiciones")]
    [SerializeField] private Vector2 recipesHiddenPos = new Vector2(-1500, 0);
    [SerializeField] private Vector2 shownRecipesPosition = Vector2.zero;
    [SerializeField] private Vector2 ingredientsHiddenPos = new Vector2(1500, 0);
    [SerializeField] private Vector2 shownIngredientsPosition = Vector2.zero;
    [SerializeField] private Vector2 cauldronHiddenPos = new Vector2(0, 1000);
    [SerializeField] private Vector2 shownCauldronPosition = Vector2.zero;

    [Header("Eventos")]
    [Tooltip("Se dispara cuando la animación de entrada (aparecer) ha terminado.")]
    public UnityEvent OnAnimateInComplete = new UnityEvent();
    [Tooltip("Se dispara cuando la animación de salida (desaparecer) ha terminado.")]
    public UnityEvent OnAnimateOutComplete = new UnityEvent();
    public UnityEvent OnAnimateInStart;
    public UnityEvent OnAnimateOutStart;

    // Referencias a RectTransforms
    private RectTransform recipesRect;
    private RectTransform ingredientsRect;
    private RectTransform cauldronRect;

    public bool IsAnimating => LeanTween.isTweening(gameObject) || LeanTween.isTweening(recipesRect.gameObject);
    private bool hasInitialized = false;

    private void Awake()
    {
        InitializeIfNeeded();
    }
    private void InitializeIfNeeded()
    {
        if (hasInitialized) return;

        if (panelRecipes != null) recipesRect = panelRecipes.GetComponent<RectTransform>();
        if (panelIngredients != null) ingredientsRect = panelIngredients.GetComponent<RectTransform>();
        if (panelCauldron != null) cauldronRect = panelCauldron.GetComponent<RectTransform>();

        SetInitialPositions();

        if (panelRecipes != null) panelRecipes.SetActive(false);
        if (panelIngredients != null) panelIngredients.SetActive(false);
        if (panelCauldron != null) panelCauldron.SetActive(false);

        gameObject.SetActive(false);

        hasInitialized = true;
    }

    private void SetInitialPositions()
    {
        if (recipesRect != null) recipesRect.anchoredPosition = recipesHiddenPos;
        if (ingredientsRect != null) ingredientsRect.anchoredPosition = ingredientsHiddenPos;
        if (cauldronRect != null) cauldronRect.anchoredPosition = cauldronHiddenPos;

        if (recipesContentGroup != null) recipesContentGroup.alpha = 0f;
        if (ingredientsContentGroup != null) ingredientsContentGroup.alpha = 0f;
        if (cauldronContentGroup != null) cauldronContentGroup.alpha = 0f;
    }
    private void CancelAllTweens()
    {
        if (recipesRect) LeanTween.cancel(recipesRect.gameObject);
        if (ingredientsRect) LeanTween.cancel(ingredientsRect.gameObject);
        if (cauldronRect) LeanTween.cancel(cauldronRect.gameObject);

        if(recipesContentGroup) LeanTween.cancel(recipesContentGroup.gameObject);
        if(ingredientsContentGroup) LeanTween.cancel(ingredientsContentGroup.gameObject);
        if (cauldronContentGroup) LeanTween.cancel(cauldronContentGroup.gameObject);

        LeanTween.cancel(gameObject);
    }
    public void AnimateIn()
    {
        InitializeIfNeeded();
        
        CancelAllTweens();

        OnAnimateInStart?.Invoke();

        gameObject.SetActive(true);
        panelRecipes.SetActive(true);
        panelIngredients.SetActive(true);
        panelCauldron.SetActive(true);

        SetInteractable(false);

        recipesContentGroup.alpha = 0f;
        ingredientsContentGroup.alpha = 0f;
        cauldronContentGroup.alpha = 0f;

        LeanTween.alphaCanvas(recipesContentGroup, 1f, contentFadeTime).setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(ingredientsContentGroup, 1f, contentFadeTime).setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(cauldronContentGroup, 1f, contentFadeTime).setIgnoreTimeScale(true);

        LeanTween.move(recipesRect, shownRecipesPosition, showDuration)
            .setEase(showEase)
            .setIgnoreTimeScale(true);
        LeanTween.move(ingredientsRect, shownIngredientsPosition, showDuration)
            .setEase(showEase)
            .setIgnoreTimeScale(true);
        LeanTween.move(cauldronRect, shownCauldronPosition, showDuration)
                .setEase(showEase)
                .setIgnoreTimeScale(true)
                .setOnComplete(OnInComplete);
    }

    public void AnimateOut()
    {
        InitializeIfNeeded();
        CancelAllTweens();

       
        OnAnimateOutStart?.Invoke();

        SetInteractable(false);

        LeanTween.alphaCanvas(recipesContentGroup, 0f, contentFadeTime).setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(ingredientsContentGroup, 0f, contentFadeTime).setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(cauldronContentGroup, 0f, contentFadeTime).setIgnoreTimeScale(true);

        LeanTween.move(recipesRect, recipesHiddenPos, hideDuration).setEase(hideEase).setIgnoreTimeScale(true);
        LeanTween.move(ingredientsRect, ingredientsHiddenPos, hideDuration).setEase(hideEase).setIgnoreTimeScale(true);
        LeanTween.move(cauldronRect, cauldronHiddenPos, hideDuration)
            .setEase(hideEase)
            .setIgnoreTimeScale(true)
            .setOnComplete(OnOutComplete);
    }
    private void SetInteractable(bool canInteract)
    {
        if (recipesContentGroup)
        {
            recipesContentGroup.interactable = canInteract;
            recipesContentGroup.blocksRaycasts = canInteract;
        }
        if (ingredientsContentGroup)
        {
            ingredientsContentGroup.interactable = canInteract;
            ingredientsContentGroup.blocksRaycasts = canInteract;
        }
        if (cauldronContentGroup)
        {
            cauldronContentGroup.interactable = canInteract;
            cauldronContentGroup.blocksRaycasts = canInteract;
        }
    }
    private void OnInComplete()
    {
        SetInteractable(true);
        OnAnimateInComplete.Invoke();
    }

    private void OnOutComplete()
    {
        panelRecipes.SetActive(false);
        panelIngredients.SetActive(false);
        panelCauldron.SetActive(false);

        SetInitialPositions();

        gameObject.SetActive(false); 
        OnAnimateOutComplete.Invoke();
    }
}
