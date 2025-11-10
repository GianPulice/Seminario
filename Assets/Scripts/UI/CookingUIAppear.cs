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

    // Referencias a RectTransforms
    private RectTransform recipesRect;
    private RectTransform ingredientsRect;
    private RectTransform cauldronRect;

    private bool isAnimating = false;
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

        if (recipesRect != null) recipesRect.anchoredPosition = recipesHiddenPos;
        if (ingredientsRect != null) ingredientsRect.anchoredPosition = ingredientsHiddenPos;
        if (cauldronRect != null) cauldronRect.anchoredPosition = cauldronHiddenPos;

        if (recipesContentGroup != null) recipesContentGroup.alpha = 0f;
        if (ingredientsContentGroup != null) ingredientsContentGroup.alpha = 0f;
        if (cauldronContentGroup != null) cauldronContentGroup.alpha = 0f;

        if (panelRecipes != null) panelRecipes.SetActive(false);
        if (panelIngredients != null) panelIngredients.SetActive(false);
        if (panelCauldron != null) panelCauldron.SetActive(false);

        gameObject.SetActive(false);

        hasInitialized = true;
    }

    public void AnimateIn()
    {
        InitializeIfNeeded();
        if (isAnimating) return;
        isAnimating = true;

        gameObject.SetActive(true);

        panelRecipes.SetActive(true);
        panelIngredients.SetActive(true);
        panelCauldron.SetActive(true);

        recipesRect.anchoredPosition = recipesHiddenPos;
        ingredientsRect.anchoredPosition = ingredientsHiddenPos;
        cauldronRect.anchoredPosition = cauldronHiddenPos;

        recipesContentGroup.alpha = 0f;
        ingredientsContentGroup.alpha = 0f;
        cauldronContentGroup.alpha = 0f;

        AnimateIn_Contents();


        LeanTween.move(recipesRect, shownRecipesPosition, showDuration)
            .setEase(showEase)
            .setIgnoreTimeScale(true);
        LeanTween.move(ingredientsRect, shownIngredientsPosition, showDuration)
            .setEase(showEase)
            .setIgnoreTimeScale(true);
        LeanTween.move(cauldronRect, shownCauldronPosition, showDuration)
            .setEase(showEase)
            .setIgnoreTimeScale(true);
    }

    private void AnimateIn_Contents()
    {
        LeanTween.alphaCanvas(recipesContentGroup, 1f, contentFadeTime).setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(ingredientsContentGroup, 1f, contentFadeTime).setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(cauldronContentGroup, 1f, contentFadeTime)
            .setIgnoreTimeScale(true)
            .setOnComplete(OnInComplete);
    }

    public void AnimateOut()
    {
        InitializeIfNeeded();
        if (isAnimating) return;
        isAnimating = true;

        AnimateOut_Panels();
        
        LeanTween.alphaCanvas(recipesContentGroup, 0f, contentFadeTime).setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(ingredientsContentGroup, 0f, contentFadeTime).setIgnoreTimeScale(true);
        LeanTween.alphaCanvas(cauldronContentGroup, 0f, contentFadeTime)
            .setIgnoreTimeScale(true)
            .setOnComplete(()=>gameObject.SetActive(false));
        
    }

    private void AnimateOut_Panels()
    {
        LeanTween.move(recipesRect, recipesHiddenPos, hideDuration)
            .setEase(hideEase)
            .setIgnoreTimeScale(true);
        LeanTween.move(ingredientsRect, ingredientsHiddenPos, hideDuration)
            .setEase(hideEase)
            .setIgnoreTimeScale(true);
        LeanTween.move(cauldronRect, cauldronHiddenPos, hideDuration)
            .setEase(hideEase)
            .setIgnoreTimeScale(true)
            .setOnComplete(OnOutComplete);
    }

    private void OnInComplete()
    {
        isAnimating = false;

        recipesContentGroup.interactable = true;
        ingredientsContentGroup.interactable = true;
        cauldronContentGroup.interactable = true;

        OnAnimateInComplete.Invoke(); // Dispara el evento
    }

    private void OnOutComplete()
    {
        isAnimating = false;

        panelRecipes.SetActive(false);
        panelIngredients.SetActive(false);
        panelCauldron.SetActive(false);

        OnAnimateOutComplete.Invoke();
    }
}
