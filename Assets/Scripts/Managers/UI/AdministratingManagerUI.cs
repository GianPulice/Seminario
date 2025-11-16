using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections;

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

    [Header("Referencias (Panel Upgrades)")]
    [SerializeField] private ConfirmationPanel confirmationPanel;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private Image currentImageUpgrade;
    [SerializeField] private TextMeshProUGUI textPriceCurrentUpgradeUnlock;
    [SerializeField] private TextMeshProUGUI textInformationCurrentUpgrade;

    private List<IngredientButtonUI> ingredientButtons = new List<IngredientButtonUI>();

    private static event Action onEnterAdmin, onExitAdmin;
    private static event Action<GameObject> onSetSelectedCurrentGameObject;
    private static event Action onClearSelectedCurrentGameObject;
    private static event Action onStartTabern,onCloseTabern;

    public static Action OnExitAdmin { get => onExitAdmin; set => onExitAdmin = value; } 
    public static Action<GameObject> OnSetSelectedCurrentGameObject { get => onSetSelectedCurrentGameObject; set => onSetSelectedCurrentGameObject = value; }
    public static Action OnClearSelectedCurrentGameObject { get => onClearSelectedCurrentGameObject; set => onClearSelectedCurrentGameObject = value; }
    public static Action OnStartTabern { get => onStartTabern; set => onStartTabern = value; }
    public static Action OnCloseTabern { get => onCloseTabern; set => onCloseTabern = value; }

    // --- Variables de control ---
    private GameObject lastSelectedButtonFromAdminPanel;
    private bool ignoreFirstButtonSelected = true;
    private int currentActiveTabIndex = 0;
    private bool localTavernState = false;

    //--- Variable estaticas ---
    private static int lastTabIndex = -1;
    
    void Awake()
    {
        GetComponents();
        InitializeAnimatorEventBindings();
        SuscribeToPlayerViewEvents();
        SuscribeToUpdateManagerEvent();
        SuscribeToPauseManagerRestoreSelectedGameObjectEvent();
    }

    void OnDestroy()
    {
        UnsuscribeToPlayerViewEvents();
        UnsuscribeToUpdateManagerEvent();
        UnscribeToPauseManagerRestoreSelectedGameObjectEvent();

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

    private void UnscribeToPauseManagerRestoreSelectedGameObjectEvent()
    {
        if (PauseManager.Instance != null)
            PauseManager.OnRestoreSelectedGameObject -= RestoreLastSelectedGameObjectIfGameWasPausedDuringAdministratingUI;
    }

    private void RestoreLastSelectedGameObjectIfGameWasPausedDuringAdministratingUI()
    {
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

        bool isTavernOn = startTavernSwitch.GetSelectedState();

        if (isTavernOn)
        {
            Debug.Log("¡Taberna ABIERTA!");
            onStartTabern?.Invoke();
            localTavernState = true;
            AudioManager.Instance.PlayOneShotSFX("ButtonClickWell"); // sonido "Switch_On"
        }
        if(!isTavernOn && ClientManager.Instance.CanCloseTabern)
        {
            Debug.Log("¡Taberna CERRADA!");
            onCloseTabern?.Invoke();
            localTavernState = false;
            AudioManager.Instance.PlayOneShotSFX("ButtonClickWell"); // sonido "Switch_Off"
        }
    }
    public void ButtonExit()
    {
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
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
        if (ingredientButtons == null || IngredientInventoryManager.Instance == null)
            return;

        foreach (var btnUI in ingredientButtons)
        {
            IngredientType type = btnUI.IngredientType;

            int price = IngredientInventoryManager.Instance.GetPriceOfIngredient(type);
            int stock = IngredientInventoryManager.Instance.GetStock(type); 

            btnUI.UpdateUI(price, stock);
        }
    }

    public void ButtonUnlockNewZone(int index)
    {
        var upgrade = UpgradesManager.Instance.GetUpgrade(index);
        if (upgrade == null) return;

        // Si ya está desbloqueado, no hacemos nada
        if (!upgrade.CanUpgrade)
        {
            //AudioManager.Instance.PlayOneShotSFX("ButtonClickWrong");
            return;
        }

        int price = upgrade.UpgradesData.Cost;

        if (MoneyManager.Instance.CurrentMoney >= price)
        {
            AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
            UpgradesManager.Instance.UnlockUpgrade(index);
            MoneyManager.Instance.SubMoney(price);
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
        confirmationText.text = $"Are you sure you want to spend <color=yellow>${data.Cost}</color> to buy this upgrade";

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
        {
            /// Agregar un sprite generico que muestre que ya tenes la zona desbloqueada
            return;
        }

        var data = upgrade.UpgradesData;

        currentImageUpgrade.sprite = data.ImageZoneUnlock;
        textPriceCurrentUpgradeUnlock.text = $"Price: {data.Cost}";
        textInformationCurrentUpgrade.text = data.InformationCurrentZone;
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
        PrepareInitialUIState();
        if (panelAnimator != null) panelAnimator.AnimateIn();
    }

    private void HandlePlayerExitAdmin()
    {
        DeviceManager.Instance.IsUIModeActive = false;
        onClearSelectedCurrentGameObject?.Invoke();

        if (tabGroup != null)
        {
            lastTabIndex = tabGroup.GetCurrentTabIndex();
        }
     
        if (panelAnimator != null)
            panelAnimator.AnimateOut();

        confirmationPanel.Hide();
    }

    private void SetupInitialTab()
    {
        ignoreFirstButtonSelected = true;

        UpdateAllIngredientButtons();

        if (tabGroup == null) return;

        //Determino la tab a mostrar
        int indexToSelect = (lastTabIndex >= 0 && lastTabIndex < tabGroup.GetTabCount())
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
            startTavernSwitch.SetSelected(localTavernState); 
        }
        // Actualizar información si está en el tab de Upgrades
        if (indexToSelect == 2 && UpgradesManager.Instance != null && UpgradesManager.Instance.GetUpgrade(0) != null)
        {
            ShowCurrentZoneInformation(0);
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

    private void SuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInAdministrationMode += OnEnterInAdminMode;
        PlayerView.OnExitInAdministrationMode += OnExitAdminMode;
    }

    private void UnsuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInAdministrationMode -= OnEnterInAdminMode;
        PlayerView.OnExitInAdministrationMode -= OnExitAdminMode;
    }

    private void GetComponents()
    {
        if (panelAnimator == null)
            Debug.LogError("Falta AdminUIAppear", this);
        if (tabGroup == null)
            tabGroup = GetComponent<TabGroup>();
        
        if (ingredientButtonContainer != null)
        {
            // Busca recursivamente en todos los hijos del contenedor
            // El 'true' incluye los objetos inactivos, lo cual es bueno
            ingredientButtons = ingredientButtonContainer.GetComponentsInChildren<IngredientButtonUI>(true).ToList();

            if (ingredientButtons.Count == 0)
            {
                Debug.LogWarning($"No se encontró ningún script 'IngredientButtonUI' en los hijos de {ingredientButtonContainer.name}", this);
            }
        }
        else
        {
            Debug.LogError("'Ingredient Button Container' no está asignado en el Inspector de AdministratingManagerUI.", this);
        }
        startTavernSwitch.OnTryEnableCondition = () => ClientManager.Instance.CanCloseTabern;
    }

    #endregion
}