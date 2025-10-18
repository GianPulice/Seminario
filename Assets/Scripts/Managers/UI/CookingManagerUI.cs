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

    private List<Button> buttonsInformationReciepes  = new List<Button>();
    private List<Button> buttonsIngredients = new List<Button>();

    private GameObject lastSelectedButtonFromCookingPanel;

    private static event Action<string> onButtonGetFood;

    private event Action onEnterCook, onExitCook;

    private static event Action<GameObject> onSetSelectedCurrentGameObject;
    private static event Action onClearSelectedCurrentGameObject;

    private List<IngredientType> selectedIngredients = new List<IngredientType>();

    private bool ignoreFirstButtonSelected = true;

    public static Action<string> OnButtonSetFood { get => onButtonGetFood; set => onButtonGetFood = value; }

    public static Action<GameObject> OnSetSelectedCurrentGameObject { get => onSetSelectedCurrentGameObject; set => onSetSelectedCurrentGameObject = value; }
    public static Action OnClearSelectedCurrentGameObject { get => onClearSelectedCurrentGameObject; set => onClearSelectedCurrentGameObject = value; }


    void Awake()
    {
        SuscribeToUpdateManagerEvent();
        InitializeLambdaEvents();
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
        UnscribeToUpdateManagerEvent();
        UnSuscribeToPlayerViewEvents();
        UnscribeToPauseManagerRestoreSelectedGameObjectEvent();
    }


    // Funcion asignada a botones en la UI para setear el selected GameObject del EventSystem con Mouse en los botones de recetas
    public void SetButtonAsSelectedGameObjectIfHasBeenHover(int indexButton)
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(buttonsInformationReciepes[indexButton].gameObject);
        }
    }

    // Funcion asignada a botones en la UI para setear el selected GameObject del EventSystem con Mouse en los botones de ingredientes
    public void SetButtonIngredientAsSelectedGameObjectIfHasBeenHover(int indexButton)
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(buttonsIngredients[indexButton].gameObject);
        }
    }

    // Funcion asignada a botones en la UI para reproducir el sonido selected
    public void PlayAudioButtonSelectedWhenChangeSelectedGameObjectExceptFirstTime()
    {
        if (!ignoreFirstButtonSelected)
        {
            AudioManager.Instance.PlaySFX("ButtonSelected");
            return;
        }

        ignoreFirstButtonSelected = false;
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

        AudioManager.Instance.PlaySFX("ButtonClickWell");

        if (selectedIngredients.Contains(ingredient))
        {
            foreach (var button in buttonsIngredients)
            {
                if (EventSystem.current.currentSelectedGameObject == button.gameObject)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    selectedIngredients.Remove(ingredient);
                    SetButtonNormalColorInWhite(button);
                    break;
                }
            }
        }

        else
        {
            foreach (var button in buttonsIngredients)
            {
                if (EventSystem.current.currentSelectedGameObject == button.gameObject)
                {
                    selectedIngredients.Add(ingredient);
                    SetButtonNormalColorInGreen(button);
                    break;
                }
            }
        }
    }

    public void CookSelectedIngredients()
    {
        foreach (var recipe in IngredientInventoryManager.Instance.GetAllRecipes())
        {
            // Ingredientes de la receta
            var recipeIngredients = recipe.Ingridients.Select(i => i.IngredientType).ToList();

            // Verificamos que los seleccionados coincidan exactamente con los de la receta
            if (selectedIngredients.Count == recipeIngredients.Count && !selectedIngredients.Except(recipeIngredients).Any())
            {
                // Ahora verificamos stock
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
                    // Cocinar
                    AudioManager.Instance.PlaySFX("ButtonClickWell");
                    onButtonGetFood?.Invoke(recipe.FoodType.ToString());

                    Debug.Log($"Cocinaste {recipe.FoodType}!");
                    DeselectAllIngredients();
                    return;
                }
            }
        }

        Debug.Log("No hay receta con esos ingredientes o no alcanza el stock.");
        DeselectAllIngredients();
    }

    public void DeselectAllIngredients()
    {
        selectedIngredients.Clear();

        foreach (var button in buttonsIngredients)
        {
            SetButtonNormalColorInWhite(button);
        }
    }


    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdateCookingManagerUI;
    }

    private void UnscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate -= UpdateCookingManagerUI;
    }

    private void InitializeLambdaEvents()
    {
        onEnterCook += () => ActiveOrDeactivateRootGameObject(true);
        onExitCook += () => ActiveOrDeactivateRootGameObject(false);
    }

    private void SuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInCookMode += onEnterCook;
        PlayerView.OnExitInCookMode += onExitCook;
    }

    private void UnSuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInCookMode -= onEnterCook;
        PlayerView.OnExitInCookMode -= onExitCook;
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
            Button button = childs.GetComponent<Button>();
            if (button != null) buttonsInformationReciepes.Add(button);
        }

        Transform rootButtonsIngredients = rootGameObject.transform.Find("ButtonsIngredients");

        foreach (Transform childs in rootButtonsIngredients.transform)
        {
            Button button = childs.GetComponent<Button>();
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

    private void SetButtonNormalColorInGreen(Button currentButton)
    {
        ColorBlock colorWhite = currentButton.colors;
        colorWhite.normalColor = new Color32(0x28, 0xFF, 0x00, 0xFF);
        currentButton.colors = colorWhite;
    }

    private void SetButtonNormalColorInWhite(Button currentButton)
    {
        ColorBlock colorWhite = currentButton.colors;
        colorWhite.normalColor = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
        currentButton.colors = colorWhite;
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