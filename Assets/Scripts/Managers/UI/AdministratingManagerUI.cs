using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class AdministratingManagerUI : MonoBehaviour
{
    [Header("Paneles Principales")]
    [SerializeField] private GameObject panelAdministrating;
    [SerializeField] private GameObject panelTabern;
    [SerializeField] private GameObject panelIngredients;
    [SerializeField] private GameObject panelUpgrades;
    [SerializeField] private AdminUIAppear panelAnimator;

    [Header("Gestión de Pestañas")]
    [Tooltip("Referencia al script TabGroup que gestiona los botones y paneles")]
    [SerializeField] private TabGroup tabGroup;

    [Header("Referencias (Panel Upgrades)")]
    [SerializeField] private List<ZoneUnlock> zoneUnlocks = new List<ZoneUnlock>();
    [SerializeField] private Image currentImageZoneUnlock;
    [SerializeField] private TextMeshProUGUI textPriceCurrentZoneUnlock;


    private event Action onEnterAdmin, onExitAdmin;
    private static event Action<GameObject> onSetSelectedCurrentGameObject;
    private static event Action onClearSelectedCurrentGameObject;
    private static event Action onStartTabern;

    public static Action<GameObject> OnSetSelectedCurrentGameObject { get => onSetSelectedCurrentGameObject; set => onSetSelectedCurrentGameObject = value; }
    public static Action OnClearSelectedCurrentGameObject { get => onClearSelectedCurrentGameObject; set => onClearSelectedCurrentGameObject = value; }
    public static Action OnStartTabern { get => onStartTabern; set => onStartTabern = value; }

    // --- Variables de control ---
    private PlayerModel playerModel;
    private GameObject lastSelectedButtonFromAdminPanel;
    private bool ignoreFirstButtonSelected = true;
    private int currentActiveTabIndex = 0;

    // --- Constantes ---
    private const int TOTAL_TABS = 3;
    void Awake()
    {
        GetComponents();
        InitializeLambdaEvents();
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
            panelAnimator.OnAnimateOutComplete.RemoveListener(CleanupAfterAnimation);
        }
    }

    #region === Actualización y Gestión de Foco ===

    void UpdateAdministratingManagerUI()
    {
        CheckLastSelectedButtonIfAdminPanelIsOpen();
        CheckJoystickInputsToInteractWithPanels();
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
        if (playerModel != null && playerModel.IsAdministrating)
        {
            ignoreFirstButtonSelected = true;
            DeviceManager.Instance.IsUIModeActive = true;
            onSetSelectedCurrentGameObject?.Invoke(lastSelectedButtonFromAdminPanel);
        }
    }

    private void CheckLastSelectedButtonIfAdminPanelIsOpen()
    {
        if (EventSystem.current != null && PauseManager.Instance != null && !PauseManager.Instance.IsGamePaused &&
            playerModel != null && playerModel.IsAdministrating)
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

    #region === Navegación por Joystick ===

    private void CheckJoystickInputsToInteractWithPanels()
    {
        if (panelAdministrating.activeSelf)
        {
            if (PlayerInputs.Instance != null && tabGroup != null)
            {
                if (PlayerInputs.Instance.R1()) SetNexPanelUsingJoystickR1();
                if (PlayerInputs.Instance.L1()) SetNexPanelUsingJoystickL1();
            }
        }
    }

    private void SetNexPanelUsingJoystickR1()
    {
        currentActiveTabIndex = tabGroup.GetCurrentTabIndex();
        currentActiveTabIndex = (currentActiveTabIndex + 1) % tabGroup.GetTabCount();
        tabGroup.SelectTabByIndex(currentActiveTabIndex);
        AudioManager.Instance.PlayOneShotSFX("ButtonSelected");
    }

    private void SetNexPanelUsingJoystickL1()
    {
        currentActiveTabIndex = tabGroup.GetCurrentTabIndex();
        currentActiveTabIndex = (currentActiveTabIndex - 1 + tabGroup.GetTabCount()) % tabGroup.GetTabCount();
        tabGroup.SelectTabByIndex(currentActiveTabIndex);
        AudioManager.Instance.PlayOneShotSFX("ButtonSelected");
    }

    #endregion

    #region == Botones de Lógica ===
    public void ButtonStartTabern()
    {
        onStartTabern?.Invoke();
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
    }
    public void ButtonExit()
    {
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        if (playerModel != null && playerModel.IsAdministrating)
        {
            playerModel.IsAdministrating = false;
            PlayerView.OnExitInAdministrationMode?.Invoke();
        }
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
            }
            else
            {
                AudioManager.Instance.PlayOneShotSFX("ButtonClickWrong");
            }
        }
    }

    public void ButtonUnlockNewZone(int index)
    {
        if (zoneUnlocks == null || index < 0 || index >= zoneUnlocks.Count) return;
        if (zoneUnlocks[index].IsUnlocked) return;
        
        int price = zoneUnlocks[index].ZoneUnlockData.Cost;
        if (MoneyManager.Instance.CurrentMoney >= price)
        {
            AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
            zoneUnlocks[index].UnlockZone();
            MoneyManager.Instance.SubMoney(price);
        }
        else
        {
            AudioManager.Instance.PlayOneShotSFX("ButtonClickWrong");
        }
    }

    public void ShowCurrentZoneInformation(int index)
    {
        if (zoneUnlocks == null || index < 0 || index >= zoneUnlocks.Count) return;
        currentImageZoneUnlock.sprite = zoneUnlocks[index].ZoneUnlockData.ImageZoneUnlock;
        textPriceCurrentZoneUnlock.text = "Price: " + zoneUnlocks[index].ZoneUnlockData.Cost.ToString();
    }

    #endregion

    #region === Animaciones de Entrada / Salida ===
    private void InitializeLambdaEvents()
    {
        onEnterAdmin += () => HandlePlayerEnterAdmin();
        onExitAdmin += () => HandlePlayerExitAdmin();
    }
    private void HandlePlayerEnterAdmin()
    {
        if(playerModel!=null) playerModel.IsUITransitioning = true;

        PrepareInitialUIState();
        if (panelAnimator != null) panelAnimator.AnimateIn();
    }

    private void HandlePlayerExitAdmin()
    {
        if (playerModel != null) playerModel.IsUITransitioning = true;

        DeviceManager.Instance.IsUIModeActive = false;
        onClearSelectedCurrentGameObject?.Invoke();
        if (panelAnimator != null) panelAnimator.AnimateOut();
    }

    private void SetupInitialTab()
    {
        if (playerModel != null) playerModel.IsUITransitioning = false;

        DeviceManager.Instance.IsUIModeActive = true;
        ignoreFirstButtonSelected = true;

        if (tabGroup != null && tabGroup.StartingSelectedButton != null)
        {
            onSetSelectedCurrentGameObject?.Invoke(tabGroup.StartingSelectedButton.gameObject);
        }
    }

    private void CleanupAfterAnimation()
    {
        if (playerModel != null) playerModel.IsUITransitioning = false;

        panelTabern.SetActive(false);
        panelIngredients.SetActive(false);
        panelUpgrades.SetActive(false);
    }

    private void PrepareInitialUIState()
    {
        int initialTabIndex = tabGroup?.GetButtonIndex(tabGroup.StartingSelectedButton) ?? 0;
        if (initialTabIndex == 2) ShowCurrentZoneInformation(0);
    }
    #endregion

    #region Setup y Suscripciones

    private void InitializeAnimatorEventBindings()
    {
        panelAnimator.OnAnimateInComplete.AddListener(SetupInitialTab);
        panelAnimator.OnAnimateOutComplete.AddListener(CleanupAfterAnimation);
    }

    private void SuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInAdministrationMode += onEnterAdmin;
        PlayerView.OnExitInAdministrationMode += onExitAdmin;
    }

    private void UnsuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInAdministrationMode -= onEnterAdmin;
        PlayerView.OnExitInAdministrationMode -= onExitAdmin;
    }

    private void GetComponents()
    {
        playerModel = FindFirstObjectByType<PlayerModel>();
        if (panelAnimator == null)
            Debug.LogError("Falta AdminUIAppear", this);
        if (tabGroup == null)
            tabGroup = GetComponent<TabGroup>();

        GameObject ZonesToUnlockFather = GameObject.Find("ZonesToUnlock");
        if (ZonesToUnlockFather != null)
        {
            foreach (Transform childs in ZonesToUnlockFather.transform)
            {
                zoneUnlocks.Add(childs.GetComponent<ZoneUnlock>());
            }
        }
    }

    #endregion
}