using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

public class AdministratingManagerUI : MonoBehaviour
{
    [Header("Paneles Principales")]
    [SerializeField] private GameObject panelAdministrating;
    [SerializeField] private GameObject panelIngredients;
    [SerializeField] private GameObject panelUpgrades;
    [SerializeField] private AdminUIAppear panelAnimator;

    [Header("Gestión de Pestañas")]
    [Tooltip("Referencia al script TabGroup que gestiona los botones y paneles")]
    [SerializeField] private TabGroup tabGroup;

    [Header("Botones de Lógica")]
    [Tooltip("El botón tipo Switch para Iniciar/Cerrar la taberna")]
    [SerializeField] private SwitchTweenButton startTavernSwitch;

    [Header("Referencias (Panel Ingredients)")]
    [SerializeField] private GameObject ingredientButtonContainer;
    [SerializeField] private GameObject firstUpgradeButtonContainer;
    [SerializeField] private GameObject secondUpgradeButtonContainer;
    [SerializeField] private GameObject thirdUpgradeButtonContainer;

    [Header("Referencias (Panel Upgrades)")]
    [SerializeField] private ConfirmationPanel confirmationPanel;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private Image currentImageUpgrade;
    [SerializeField] private TextMeshProUGUI textPriceCurrentUpgradeUnlock;
    [SerializeField] private TextMeshProUGUI textInformationCurrentUpgrade;

    [Header("Configuración Visual Upgrades")]
    [SerializeField] private Color unlockedUpgradeColor = Color.green;
    [SerializeField] private Color defaultColor = Color.white;

    //--- Listas de botones ---
    private List<IngredientButtonUI> ingredientButtons = new List<IngredientButtonUI>();
    private List<GenericTweenButton> upgradeButtons = new List<GenericTweenButton>();

    private static event Action onEnterAdmin, onExitAdmin;
    private static event Action<GameObject> onSetSelectedCurrentGameObject;
    private static event Action onClearSelectedCurrentGameObject;
    private static event Action onStartTabern, onCloseTabern;
    public static Action OnExitAdmin { get => onExitAdmin; set => onExitAdmin = value; }
    public static Action<GameObject> OnSetSelectedCurrentGameObject { get => onSetSelectedCurrentGameObject; set => onSetSelectedCurrentGameObject = value; }
    public static Action OnClearSelectedCurrentGameObject { get => onClearSelectedCurrentGameObject; set => onClearSelectedCurrentGameObject = value; }
    public static Action OnStartTabern { get => onStartTabern; set => onStartTabern = value; }
    public static Action OnCloseTabern { get => onCloseTabern; set => onCloseTabern = value; }

    // --- Variables de control ---
    private GameObject lastSelectedButtonFromAdminPanel;
    private bool ignoreFirstButtonSelected = true;
    private bool localTavernState = false;

    //--- Variable estaticas ---
    private static int lastTabIndex = -1;

    void Awake()
    {
        GetComponents();
        InitializeAnimatorEventBindings();
        SubscribeToPlayerViewEvents();
        SuscribeToUpdateManagerEvent();
        SuscribeToPauseManagerRestoreSelectedGameObjectEvent();
        SubscribeToRecipeProgressEvents();
        SubscribeToTabernStateEvents();
    }

    void OnDestroy()
    {
        UnsubscribeToPlayerViewEvents();
        UnsuscribeToUpdateManagerEvent();
        UnsuscribeToPauseManagerRestoreSelectedGameObjectEvent();
        UnsubscribeToRecipeProgressEvents();
        UnsubscribeToTabernStateEvents();

        if (panelAnimator != null)
        {
            panelAnimator.OnAnimateInComplete.RemoveListener(SetupInitialTab);
        }
    }

    #region === Actualización y Gestión de Foco ===

    void UpdateAdministratingManagerUI()
    {
        CheckLastSelectedButtonIfAdminPanelIsOpen();
    }

    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdateAdministratingManagerUI;
    }

    private void UnsuscribeToUpdateManagerEvent()
    {
        if (UpdateManager.Instance != null)
            UpdateManager.OnUpdate -= UpdateAdministratingManagerUI;
    }

    private void SuscribeToPauseManagerRestoreSelectedGameObjectEvent()
    {
        PauseManager.OnRestoreSelectedGameObject += RestoreLastSelectedGameObjectIfGameWasPausedDuringAdministratingUI;
    }

    private void UnsuscribeToPauseManagerRestoreSelectedGameObjectEvent()
    {
        if (PauseManager.Instance != null)
            PauseManager.OnRestoreSelectedGameObject -= RestoreLastSelectedGameObjectIfGameWasPausedDuringAdministratingUI;
    }

    private void RestoreLastSelectedGameObjectIfGameWasPausedDuringAdministratingUI()
    {
        if (panelAdministrating == null) return; // Esta linea de codigo se agrego, porque sino cuando se volvio del game al mainmenu y luego se volvio a entrar al game, la pausa no anda y tira un error
        if (panelAdministrating.activeSelf)
        {
            ignoreFirstButtonSelected = true;
            DeviceManager.Instance.IsUIModeActive = true;
            onSetSelectedCurrentGameObject?.Invoke(lastSelectedButtonFromAdminPanel);
        }
    }

    private void CheckLastSelectedButtonIfAdminPanelIsOpen()
    {
        if (EventSystem.current != null && PauseManager.Instance != null && !PauseManager.Instance.IsGamePaused)
        {
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            if (currentSelected != null && currentSelected.transform.IsChildOf(this.transform))
            {
                lastSelectedButtonFromAdminPanel = currentSelected;
            }
        }
    }

    public void PlayAudioButtonSelectedWhenChangeSelectedGameObjectExceptFirstTime()
    {
        if (!ignoreFirstButtonSelected)
        {
            AudioManager.Instance.PlayOneShotSFX("ButtonSelected");
            return;
        }
        ignoreFirstButtonSelected = false;
    }
    #endregion

    #region == Botones de Lógica ===
    public void OnStartTavernSwitchClicked()
    {
        if (startTavernSwitch == null) return;

        bool selected = startTavernSwitch.GetSelectedState();
        if (selected)
        {
            OnStartTabern?.Invoke();
            return;
        }
        if (TabernManager.Instance.IsTabernOpen)
        {
            // Cancel the visual change
            startTavernSwitch.SetSelected(true);

            AudioManager.Instance.PlayOneShotSFX("ButtonClickWrong");

            MessagePopUp.Show("You can't close the tavern right now.");

            return;
        }

        OnCloseTabern?.Invoke();
    }
    private void HandleTavernOpened()
    {
        localTavernState = true;
        startTavernSwitch.SetSelected(true);  
        startTavernSwitch.LockSwitch();        
    }
    private void HandleTavernClosed()
    {
        localTavernState = false;
        startTavernSwitch.SetSelected(false);  
        startTavernSwitch.UnlockSwitch();      
    }
    public void ButtonExit()
    {
        onExitAdmin?.Invoke();
    }

    public void ButtonBuyIngredient(string ingredientName)
    {
        if (Enum.TryParse(ingredientName, out IngredientType ingredient))
        {
            int price = IngredientInventoryManager.Instance.GetPriceOfIngredient(ingredient);
            if (MoneyManager.Instance.CurrentMoney >= price)
            {
                AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
                IngredientInventoryManager.Instance.IncreaseIngredientStock(ingredient);
                MoneyManager.Instance.SubMoney(price);
                TabernManager.Instance.PurchasedIngredientsAmount += price;
                UpdateAllIngredientButtons();
            }
            else
            {
                AudioManager.Instance.PlayOneShotSFX("ButtonClickWrong");
            }
        }
    }
    private void UpdateAllIngredientButtons()
    {
        if (ingredientButtons == null) return;

        foreach (var btnUI in ingredientButtons)
        {
            IngredientType type = btnUI.IngredientType;

            int price = IngredientInventoryManager.Instance.GetPriceOfIngredient(type);
            int stock = IngredientInventoryManager.Instance.GetStock(type);

            btnUI.UpdateUI(price, stock);

            bool isUnlocked = RecipeProgressManager.Instance.IsIngredientUnlocked(type);
            btnUI.gameObject.SetActive(isUnlocked);
        }
    }

    public void ButtonUnlockNewZone(int index)
    {
        var upgrade = UpgradesManager.Instance.GetUpgrade(index);
        if (upgrade == null) return;

        if (!upgrade.CanUpgrade)
            return;

        int price = upgrade.UpgradesData.Cost;

        if (MoneyManager.Instance.CurrentMoney >= price)
        {
            AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
            UpgradesManager.Instance.UnlockUpgrade(index);
            MoneyManager.Instance.SubMoney(price);

            UpdateUpgradeButtonsInteractable();

            int next = GetNextAvailableUpgradeIndex();
            if (next != -1)
                ShowCurrentZoneInformation(next);

            Debug.Log("Yes");
        }
        else
        {
            AudioManager.Instance.PlayOneShotSFX("ButtonClickWrong");
        }
    }

    public void OnUpgradeButtonClicked(int index)
    {
        var upgrade = UpgradesManager.Instance.GetUpgrade(index);
        var data = upgrade.UpgradesData;

        if (!upgrade.CanUpgrade)
            return;

        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        if(confirmationText != null) confirmationText.text = $"Are you sure you want to spend <color=yellow>${data.Cost}</color> to buy this upgrade";

        // Mostrar panel de confirmación y asignar la acción a realizar si presiona YES
        if (confirmationPanel != null)
        {
            confirmationPanel.Show(() => ButtonUnlockNewZone(index));
        }
    }

    public void ShowCurrentZoneInformation(int index)
    {
        var upgrade = UpgradesManager.Instance.GetUpgrade(index);
        if (upgrade == null) return;

        if (!upgrade.CanUpgrade)
            return;

        var data = upgrade.UpgradesData;

        currentImageUpgrade.sprite = data.ImageZoneUnlock;
        textPriceCurrentUpgradeUnlock.text = $"Price: {data.Cost}";
        textInformationCurrentUpgrade.text = data.InformationCurrentZone;
    }
    private void UpdateIngredientButtonsFromUpgrades()
    {
        foreach (var btn in ingredientButtons)
        {
            bool isUnlocked = RecipeProgressManager.Instance.IsIngredientUnlocked(btn.IngredientType);
            btn.gameObject.SetActive(isUnlocked);
        }
    }
    private void UpdateUpgradeButtonsInteractable()
    {
        int totalButtons = upgradeButtons.Count;

        for (int i = 0; i < upgradeButtons.Count; i++)
        {
            bool shouldBeInteractable = false;
            if (i == 0)
            {
                shouldBeInteractable = true;
            }
            else
            {
                var previousUpgrade = UpgradesManager.Instance.GetUpgrade(i - 1);

                if (previousUpgrade != null && !previousUpgrade.CanUpgrade)
                {
                    shouldBeInteractable = true;
                }
            }
            if (shouldBeInteractable)
            {
                upgradeButtons[i].ChangeColor(unlockedUpgradeColor);
            }
            else
            {
                upgradeButtons[i].ChangeColor(defaultColor);
            }
            upgradeButtons[i].SetInteractable(shouldBeInteractable);
        }
    }
    private int GetNextAvailableUpgradeIndex()
    {
        for (int i = 0; i < UpgradesManager.Instance.GetUpgradesCount(); i++)
        {
            var upgrade = UpgradesManager.Instance.GetUpgrade(i);
            if (upgrade.CanUpgrade)
                return i;
        }
        return -1;
    }
    private void OnIngredientUnlocked(IngredientType ingredient)
    {
        UpdateIngredientButtonsFromUpgrades();
    }
    private void PreRefreshUI()
    {
        UpdateAllIngredientButtons();
        UpdateUpgradeButtonsInteractable();

        // Si estás entrando a la UI y la última tab era "Upgrades"
        if (tabGroup != null)
        {
            int indexToSelect =
                (lastTabIndex >= 0 && lastTabIndex < tabGroup.GetTabCount())
                ? lastTabIndex
                : 0;

            // si la tab es la de upgrades, refrescá la info
            if (indexToSelect == 2)
            {
                ShowCurrentZoneInformation(0);
            }
        }
    }
    #endregion

    #region === Animaciones de Entrada / Salida ===

    private void OnEnterInAdminMode()
    {
        HandlePlayerEnterAdmin();
    }

    private void OnExitAdminMode()
    {
        HandlePlayerExitAdmin();
    }

    private void HandlePlayerEnterAdmin()
    {
        AudioManager.Instance.PlayOneShotSFX("Admin/Cook/Pause");
        PrepareInitialUIState();
        PreRefreshUI();
        panelAnimator?.AnimateIn();
    }

    private void HandlePlayerExitAdmin()
    {
        AudioManager.Instance.PlayOneShotSFX("Admin/Cook/Pause");
        DeviceManager.Instance.IsUIModeActive = false;
        onClearSelectedCurrentGameObject?.Invoke();

        if (tabGroup != null)
            lastTabIndex = tabGroup.GetCurrentTabIndex();

        panelAnimator?.AnimateOut();
        confirmationPanel.Hide();
    }



    private void SetupInitialTab()
    {
        ignoreFirstButtonSelected = true;

        UpdateAllIngredientButtons();
        UpdateUpgradeButtonsInteractable();

        if (tabGroup == null) return;

        //Determino la tab a mostrar
        int indexToSelect =
          (lastTabIndex >= 0 && lastTabIndex < tabGroup.GetTabCount())
          ? lastTabIndex
          : 0;

        tabGroup.SelectTabByIndex(indexToSelect);
        tabGroup.ForceShowCurrentTab();

        // Forzar selección visual del botón correcto
        if (tabGroup.CurrentSelectedButton != null)
        {
            onSetSelectedCurrentGameObject?.Invoke(tabGroup.CurrentSelectedButton.gameObject);
        }
        if (startTavernSwitch != null)
        {
            localTavernState = TabernManager.Instance.IsTabernOpen;
            startTavernSwitch.SetSelected(localTavernState);
        }

        if (indexToSelect == 2)
        {
            int nextIndex = GetNextAvailableUpgradeIndex();
            if (nextIndex != -1)
                ShowCurrentZoneInformation(nextIndex);
        }
    }

    private void PrepareInitialUIState()
    {
        DeviceManager.Instance.IsUIModeActive = true;
        int initialTabIndex = tabGroup?.GetButtonIndex(tabGroup.StartingSelectedButton) ?? 0;
        if (initialTabIndex == 2) ShowCurrentZoneInformation(0);
    }
    #endregion

    #region Setup y Suscripciones

    private void InitializeAnimatorEventBindings()
    {
        panelAnimator.OnAnimateInComplete.AddListener(SetupInitialTab);
        panelAnimator.OnAnimateOutStart.AddListener(() =>
        {
            panelIngredients.SetActive(false);
            panelUpgrades.SetActive(false);
        });
    }

    private void SubscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInAdministrationMode += OnEnterInAdminMode;
        PlayerView.OnExitInAdministrationMode += OnExitAdminMode;
    }

    private void UnsubscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInAdministrationMode -= OnEnterInAdminMode;
        PlayerView.OnExitInAdministrationMode -= OnExitAdminMode;
    }

    private void SubscribeToRecipeProgressEvents()
    {
        RecipeProgressManager.Instance.OnIngredientUnlocked += OnIngredientUnlocked;
    }

    private void UnsubscribeToRecipeProgressEvents()
    {
        if (RecipeProgressManager.Instance == null) return;

        RecipeProgressManager.Instance.OnIngredientUnlocked -= OnIngredientUnlocked;
    }

    private void SubscribeToTabernStateEvents()
    {
        OnStartTabern += HandleTavernOpened;
        OnCloseTabern += HandleTavernClosed;
    }
    private void UnsubscribeToTabernStateEvents()
    {
        OnStartTabern -= HandleTavernOpened;
        OnCloseTabern -= HandleTavernClosed;
    }
    private void GetComponents()
    {
        if (panelAnimator == null)
            Debug.LogError("Falta AdminUIAppear", this);
        if (tabGroup == null)
            tabGroup = GetComponent<TabGroup>();

        if (ingredientButtonContainer != null)
        {
            ingredientButtons = ingredientButtonContainer
              .GetComponentsInChildren<IngredientButtonUI>(true).ToList();

            if (ingredientButtons.Count == 0)
            {
                Debug.LogWarning($"No se encontró ningún script 'IngredientButtonUI' en {ingredientButtonContainer.name}", this);
            }
        }
        else
        {
            Debug.LogError("'Ingredient Button Container' no está asignado.", this);
        }

        upgradeButtons = new List<GenericTweenButton>();

        if (firstUpgradeButtonContainer != null)
        {
            var firstBatch = firstUpgradeButtonContainer.GetComponentsInChildren<GenericTweenButton>(true);
            upgradeButtons.AddRange(firstBatch);
        }
        else
        {
            Debug.LogError("'First Upgrade Button Container' no está asignado.", this);
        }

        if (secondUpgradeButtonContainer != null)
        {
            var secondBatch = secondUpgradeButtonContainer.GetComponentsInChildren<GenericTweenButton>(true);
            upgradeButtons.AddRange(secondBatch);
        }
        else
        {
            Debug.LogError("'Second Upgrade Button Container' no está asignado.", this);
        }
        if (thirdUpgradeButtonContainer != null)
        {
            var thirdBatch = thirdUpgradeButtonContainer.GetComponentsInChildren<GenericTweenButton>(true);
            upgradeButtons.AddRange(thirdBatch);
        }
        else
        {
            Debug.LogWarning("'Third Upgrade Button Container' no está asignado (o no es requerido).", this);
        }

        if (upgradeButtons.Count == 0)
        {
            Debug.LogWarning("No se encontraron 'GenericTweenButton' en ninguno de los contenedores de upgrades.", this);
        }

    }

    #endregion
}