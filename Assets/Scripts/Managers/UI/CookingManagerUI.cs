using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class CookingManagerUI : MonoBehaviour
{
    [Header("Main References")]
    [SerializeField] private GameObject rootGameObject; // GameObject padre con los botones de hijos
    [SerializeField] private GameObject panelInformation;
    [SerializeField] private GameObject cookingCamera;

    [Header("AnimScript GameObject")]
    [SerializeField] private CookingUIAppear cookingAnim;

    /// <summary>
    /// Agregar ruido de cancelacion si no tiene ingredientes para cocinar una receta
    /// </summary>

    [Header("ButtonContainers")]
    [SerializeField] private GameObject recipeButtonsContainer;
    [SerializeField] private GameObject ingredientButtonsContainer;

    [Header("RecipeUI")]
    [SerializeField] private List<RecipeInformationUI> recipesInformationUI;

    private List<GenericTweenButton> buttonsInformationReciepes = new List<GenericTweenButton>();
    private List<IngredientButtonUI> buttonsIngredients = new List<IngredientButtonUI>();

    // --- Variables de Estado ---
    private GameObject lastSelectedButtonFromCookingPanel;
    private List<IngredientType> selectedIngredients = new List<IngredientType>();
    private bool ignoreFirstButtonSelected = true;

    // --- Eventos Estáticos ---
    private static event Action<string> onButtonGetFood;
    private static event Action onEnterCook, onExitCookRequest;
    private static event Action<GameObject> onSetSelectedCurrentGameObject;
    private static event Action onClearSelectedCurrentGameObject;

    public static Action<string> OnButtonSetFood { get => onButtonGetFood; set => onButtonGetFood = value; }
    public static Action OnExitCook { get => onExitCookRequest; set => onExitCookRequest = value; }
    public static Action<GameObject> OnSetSelectedCurrentGameObject { get => onSetSelectedCurrentGameObject; set => onSetSelectedCurrentGameObject = value; }
    public static Action OnClearSelectedCurrentGameObject { get => onClearSelectedCurrentGameObject; set => onClearSelectedCurrentGameObject = value; }


    void Awake()
    {
        SuscribeToUpdateManagerEvent();
        //InitializeLambdaEvents();
        SuscribeToPlayerViewEvents();
        SuscribeToPauseManagerRestoreSelectedGameObjectEvent();
        GetComponents();

        InitializeAnimatorBindings();
    }
    private void Start()
    {
        ShowInformationRecipe(FoodType.BeastStew.ToString());
    }
    // Simulacion de Update
    void UpdateCookingManagerUI()
    {
        CheckLastSelectedButtonIfCookingPanelIsOpen();
        CheckJoystickInputsToChangeSelection();
    }

    void OnDestroy()
    {
        //ClearLambas();
        UnscribeToUpdateManagerEvent();
        UnSuscribeToPlayerViewEvents();
        UnscribeToPauseManagerRestoreSelectedGameObjectEvent();

        if (cookingAnim != null)
        {
            cookingAnim.OnAnimateInComplete.RemoveListener(SetupAfterAnimationIn);
            cookingAnim.OnAnimateOutComplete.RemoveListener(CleanupAfterAnimationOut);
        }
    }


    /// <summary>
    /// Asignar a OnPointerEnterEvent de CUALQUIER GenericTweenButton.
    /// </summary>
    public void SetSelectedGameObject(GameObject go)
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(go);
        }
    }

    /// <summary>
    /// Asignar a OnPointerExitEvent de CUALQUIER GenericTweenButton.
    /// </summary>
    public void DeselectCurrentGameObject()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // Funcion asignada a botones en la UI para reproducir el sonido selected
    public void PlayAudioButtonSelectedWhenChangeSelectedGameObjectExceptFirstTime()
    {
        if (!ignoreFirstButtonSelected)
        {
            AudioManager.Instance.PlayOneShotSFX("ButtonSelected");
            return;
        }

        ignoreFirstButtonSelected = false;
    }

    public void PlayCancelAudio()
    {
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWrong");
    }

    // Funcion asignada a event trigger de la UI para mostrar la informacion de las recetas
    public void ShowInformationRecipe(string foodTypeName)
    {
        if (!Enum.TryParse(foodTypeName, out FoodType foodType)) return;

        var recipe = IngredientInventoryManager.Instance.GetRecipe(foodType);
        if (recipe == null) return;

        for (int i = 0; i < recipesInformationUI.Count; i++)
        {
            if (i < recipe.Ingridients.Count)
            {
                var ing = recipe.Ingridients[i];
                recipesInformationUI[i].IngredientAmountText.text = ing.Amount.ToString();

                if (IngredientInventoryManager.Instance.IngredientDataDict.TryGetValue(ing.IngredientType, out var data))
                {
                    recipesInformationUI[i].IngredientImage.sprite = data.Sprite;
                }
            }

            else
            {
                recipesInformationUI[i].IngredientAmountText.text = "";
                recipesInformationUI[i].IngredientImage.sprite = null;
            }
        }
    }

    // Funcion asignada a botones ingredientes de la UI para seleccionar o deseleccionar ingredientes
    public void ButtonSelectCurrentIngredient(string ingredientType)
    {
        if (!Enum.TryParse(ingredientType, out IngredientType ingredient)) return;

        GameObject clickedObject = EventSystem.current.currentSelectedGameObject;
        if (clickedObject == null) return;

        GenericTweenButton tweenButton = clickedObject.GetComponent<GenericTweenButton>();
        if (tweenButton == null) return;

        if (selectedIngredients.Contains(ingredient))
        {
            // --- Deseleccionar ---
            AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
            selectedIngredients.Remove(ingredient);
            tweenButton.SetSelected(false);
        }
        else
        {
            // --- Intentar Seleccionar ---
            if (selectedIngredients.Count >= 3)
            {
                // Límite alcanzado
                AudioManager.Instance.PlayOneShotSFX("ButtonCancel"); // Sonido de error
                tweenButton.SetSelected(false); // Asegura que no quede visualmente seleccionado
                return;
            }

            // Añadir
            AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
            selectedIngredients.Add(ingredient);
            tweenButton.SetSelected(true);
        }
    }
    public void CookSelectedIngredients()
    {
        foreach (var recipe in IngredientInventoryManager.Instance.GetAllRecipes())
        {
            var recipeIngredients = recipe.Ingridients.Select(i => i.IngredientType).ToList();

            if (selectedIngredients.Count == recipeIngredients.Count && !selectedIngredients.Except(recipeIngredients).Any())
            {
                bool canCraft = true;
                foreach (var ing in recipe.Ingridients)
                {
                    if (IngredientInventoryManager.Instance.GetStock(ing.IngredientType) < ing.Amount)
                    {
                        canCraft = false;
                        break;
                    }
                }

                if (canCraft)
                {
                    AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
                    onButtonGetFood?.Invoke(recipe.FoodType.ToString());
                    Debug.Log($"Cocinaste {recipe.FoodType}!");
                    UpdateStocksForSelectedIngredients();
                    UpdateStocksForSelectedIngredients();
                    DeselectAllIngredients();
                    return;
                }
            }
        }

        // --- Lógica de fallo ---
        Debug.Log("No hay receta con esos ingredientes o no alcanza el stock.");
        AudioManager.Instance.PlayOneShotSFX("ButtonCancel");
        DeselectAllIngredients();
    }

    public void DeselectAllIngredients()
    {
        selectedIngredients.Clear();

        foreach (var buttonUI in buttonsIngredients)
        {
            GenericTweenButton tweenButton = buttonUI.GetComponent<GenericTweenButton>();
            if (tweenButton != null)
            {
                tweenButton.SetSelected(false);
            }
        }
    }

    private void UpdateAllIngredientStocks()
    {
        if (buttonsIngredients == null || IngredientInventoryManager.Instance == null) return;
        foreach (var btnUI in buttonsIngredients)
        {
            IngredientType type = btnUI.IngredientType;
            int stock = IngredientInventoryManager.Instance.GetStock(type);
            btnUI.UpdateStock(stock);
        }
    }

    private void UpdateStocksForSelectedIngredients()
    {
        if (buttonsIngredients == null || IngredientInventoryManager.Instance == null) return;

        // Evitamos duplicados por si hay repetidos en la lista de selección
        foreach (var type in selectedIngredients.Distinct())
        {
            var btnUI = buttonsIngredients.FirstOrDefault(b => b.IngredientType == type);
            if (btnUI != null)
            {
                int stock = IngredientInventoryManager.Instance.GetStock(type);
                btnUI.UpdateStock(stock);
            }
        }
    }

    public void ButtonExit()
    {
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        onExitCookRequest?.Invoke();
    }

    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdateCookingManagerUI;
    }

    private void UnscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate -= UpdateCookingManagerUI;
    }

    private void InitializeAnimatorBindings()
    {
        if (cookingAnim == null)
        {
            Debug.LogError("CookingUIAppear (cookingAnim) no está asignado en el Inspector!", this);
            return;
        }
        cookingAnim.OnAnimateInComplete.AddListener(SetupAfterAnimationIn);
        cookingAnim.OnAnimateOutComplete.AddListener(CleanupAfterAnimationOut);
    }

    private void SuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInCookMode += HandleEnterCookMode;
        PlayerView.OnExitInCookMode += HandleExitCookMode;
    }

    private void UnSuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInCookMode -= HandleEnterCookMode;
        PlayerView.OnExitInCookMode -= HandleExitCookMode;
    }

    private void SuscribeToPauseManagerRestoreSelectedGameObjectEvent()
    {
        PauseManager.OnRestoreSelectedGameObject += RestoreLastSelectedGameObjectIfGameWasPausedDuringAdministratingUI;
    }

    private void UnscribeToPauseManagerRestoreSelectedGameObjectEvent()
    {
        PauseManager.OnRestoreSelectedGameObject -= RestoreLastSelectedGameObjectIfGameWasPausedDuringAdministratingUI;
    }

    private void GetComponents()
    {
        if (recipeButtonsContainer != null)
        {
            buttonsInformationReciepes = recipeButtonsContainer.GetComponentsInChildren<GenericTweenButton>(true).ToList();
        }
        else
        {
            Debug.LogError("'Recipe Buttons Container' no está asignado en el Inspector.", this);
        }

        if (ingredientButtonsContainer != null)
        {
            buttonsIngredients = ingredientButtonsContainer.GetComponentsInChildren<IngredientButtonUI>(true).ToList();

            if (buttonsIngredients.Count == 0)
            {
                Debug.LogWarning($"No se encontró ningún script 'IngredientButtonUI' en los hijos de {ingredientButtonsContainer.name}", this);
            }
        }
        else
        {
            Debug.LogError("'Ingredient Buttons Container' no está asignado en el Inspector.", this);
        }
    }

    private void HandleEnterCookMode()
    {
        panelInformation.SetActive(true);
        cookingCamera.SetActive(true);

        UpdateAllIngredientStocks();
        // Inicia la animación de los 3 paneles
        if (cookingAnim != null)
            cookingAnim.AnimateIn();
    }

    /// <summary>
    /// Se llama por PlayerView.OnExitInCookMode.
    /// Inicia la animación de salida.
    /// </summary>
    private void HandleExitCookMode()
    {
        UpdateAllIngredientStocks();
        // Inicia la animación de salida
        if (cookingAnim != null)
            cookingAnim.AnimateOut();
    }

    /// <summary>
    /// Se llama cuando cookingAnim.OnAnimateInComplete se dispara.
    /// Configura el estado de UI (modo, foco, etc.).
    /// </summary>
    private void SetupAfterAnimationIn()
    {
        DeviceManager.Instance.IsUIModeActive = true;

        if (buttonsInformationReciepes.Count > 0)
        {
            onSetSelectedCurrentGameObject?.Invoke(buttonsInformationReciepes[0].gameObject);
        }
        else
        {
            Debug.LogWarning("No hay botones de información de recetas para seleccionar.", this);
        }
        ignoreFirstButtonSelected = true;

    }

    /// <summary>
    /// Se llama cuando cookingAnim.OnAnimateOutComplete se dispara.
    /// Limpia el estado de UI.
    /// </summary>
    private void CleanupAfterAnimationOut()
    {
        panelInformation.SetActive(false);
        cookingCamera.SetActive(false);

        // Limpia el estado
        ignoreFirstButtonSelected = true;
        DeviceManager.Instance.IsUIModeActive = false;
        onClearSelectedCurrentGameObject?.Invoke();
        DeselectAllIngredients();
    }

    private void RestoreLastSelectedGameObjectIfGameWasPausedDuringAdministratingUI()
    {
        if (rootGameObject != null && rootGameObject.activeSelf)
        {
            ignoreFirstButtonSelected = true;
            DeviceManager.Instance.IsUIModeActive = true;
            EventSystem.current.SetSelectedGameObject(lastSelectedButtonFromCookingPanel);
        }
    }

    private void CheckLastSelectedButtonIfCookingPanelIsOpen()
    {
        if (EventSystem.current != null && PauseManager.Instance != null && !PauseManager.Instance.IsGamePaused && rootGameObject != null && rootGameObject.activeSelf)
        {
            lastSelectedButtonFromCookingPanel = EventSystem.current.currentSelectedGameObject;
        }
    }

    private void CheckJoystickInputsToChangeSelection()
    {
        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null) return;
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        if (PlayerInputs.Instance.L1() || PlayerInputs.Instance.R1())
        {
            if (buttonsInformationReciepes.Any(b => b.gameObject == currentSelected))
            {
                if (buttonsIngredients.Count > 0)
                    onSetSelectedCurrentGameObject?.Invoke(buttonsIngredients[0].gameObject);
            }

            else if (buttonsIngredients.Any(b => b.gameObject == currentSelected))
            {
                if (buttonsInformationReciepes.Count > 0)
                    onSetSelectedCurrentGameObject?.Invoke(buttonsInformationReciepes[0].gameObject);
            }

        }
    }
}

[Serializable]
public class RecipeInformationUI
{
    [SerializeField] private Image ingredientImage;
    [SerializeField] private TextMeshProUGUI ingredientAmountText;

    public Image IngredientImage { get => ingredientImage; }
    public TextMeshProUGUI IngredientAmountText { get => ingredientAmountText; }
}