using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class CookingManagerUI : MonoBehaviour
{
    [SerializeField] private GameObject rootGameObject; // GameObject padre con los botones de hijos
    [SerializeField] private GameObject panelInformation;
    [SerializeField] private GameObject cookingCamera;

    /// <summary>
    /// Agregar ruido de cancelacion si no tiene ingredientes para cocinar una receta
    /// </summary>

    [SerializeField] private List<RecipeInformationUI> recipesInformationUI;

    private List<GenericTweenButton> buttonsInformationReciepes = new List<GenericTweenButton>();
    private List<GenericTweenButton> buttonsIngredients = new List<GenericTweenButton>();

    private GameObject lastSelectedButtonFromCookingPanel;

    private static event Action<string> onButtonGetFood;

    private static event Action onEnterCook, onExitCook;

    private static event Action<GameObject> onSetSelectedCurrentGameObject;
    private static event Action onClearSelectedCurrentGameObject;

    private List<IngredientType> selectedIngredients = new List<IngredientType>();

    private bool ignoreFirstButtonSelected = true;

    public static Action<string> OnButtonSetFood { get => onButtonGetFood; set => onButtonGetFood = value; }

    public static Action OnExitCook { get => onExitCook; set => onExitCook = value; }

    public static Action<GameObject> OnSetSelectedCurrentGameObject { get => onSetSelectedCurrentGameObject; set => onSetSelectedCurrentGameObject = value; }
    public static Action OnClearSelectedCurrentGameObject { get => onClearSelectedCurrentGameObject; set => onClearSelectedCurrentGameObject = value; }


    void Awake()
    {
        SuscribeToUpdateManagerEvent();
        //InitializeLambdaEvents();
        SuscribeToPlayerViewEvents();
        SuscribeToPauseManagerRestoreSelectedGameObjectEvent();
        GetComponents();
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
                    DeselectAllIngredients();
                    return;
                }
            }
        }

        // --- Lógica de fallo ---
        Debug.Log("No hay receta con esos ingredientes o no alcanza el stock.");
        AudioManager.Instance.PlayOneShotSFX("ButtonCancel"); // <-- RUIDO DE CANCELACIÓN AÑADIDO
        DeselectAllIngredients();
    }
    
    public void DeselectAllIngredients()
    {
        selectedIngredients.Clear();

        foreach (var button in buttonsIngredients)
        {
            button.SetSelected(false);
        }
    }

    public void ButtonExit()
    {
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        onExitCook?.Invoke();
    }

    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdateCookingManagerUI;
    }

    private void UnscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate -= UpdateCookingManagerUI;
    }

    private void OnEnterInCookMode()
    {
        ActiveOrDeactivateRootGameObject(true);
    }

    private void OnExitInCookMode()
    {
        ActiveOrDeactivateRootGameObject(false);
    }

    private void SuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInCookMode += OnEnterInCookMode;
        PlayerView.OnExitInCookMode += OnExitInCookMode;
    }

    private void UnSuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInCookMode -= OnEnterInCookMode;
        PlayerView.OnExitInCookMode -= OnExitInCookMode;
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
        Transform rootButtonsInformationReciepes = rootGameObject.transform.Find("ButtonsInformationReciepes");

        foreach (Transform childs in rootButtonsInformationReciepes.transform)
        {
            GenericTweenButton button = childs.GetComponent<GenericTweenButton>();
            if (button != null) buttonsInformationReciepes.Add(button);
        }

        Transform rootButtonsIngredients = rootGameObject.transform.Find("ButtonsIngredients");

        foreach (Transform childs in rootButtonsIngredients.transform)
        {
            GenericTweenButton button = childs.GetComponent<GenericTweenButton>();
            if (button != null) buttonsIngredients.Add(button);
        }
    }

    private void ActiveOrDeactivateRootGameObject(bool state)
    {
        rootGameObject.SetActive(state);
        panelInformation.SetActive(state);
        cookingCamera.SetActive(state);

        if (state)
        {
            DeviceManager.Instance.IsUIModeActive = true;
            onSetSelectedCurrentGameObject?.Invoke(buttonsInformationReciepes[0].gameObject);
        }
        else
        {
            ignoreFirstButtonSelected = true;
            DeviceManager.Instance.IsUIModeActive = false;
            onClearSelectedCurrentGameObject?.Invoke();
            DeselectAllIngredients();
        }
    }

    private void RestoreLastSelectedGameObjectIfGameWasPausedDuringAdministratingUI()
    {
        if (rootGameObject.activeSelf)
        {
            ignoreFirstButtonSelected = true;
            DeviceManager.Instance.IsUIModeActive = true;
            EventSystem.current.SetSelectedGameObject(lastSelectedButtonFromCookingPanel);
        }
    }

    private void CheckLastSelectedButtonIfCookingPanelIsOpen()
    {
        if (EventSystem.current != null && PauseManager.Instance != null && !PauseManager.Instance.IsGamePaused && rootGameObject.activeSelf)
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
                onSetSelectedCurrentGameObject?.Invoke(buttonsIngredients[0].gameObject);
            }

            else if (buttonsIngredients.Any(b => b.gameObject == currentSelected))
            {
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