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
    // (Estas referencias son de lógica de negocio, están bien aquí)
    [SerializeField] private List<ZoneUnlock> zoneUnlocks = new List<ZoneUnlock>();
    [SerializeField] private Image currentImageZoneUnlock;
    [SerializeField] private TextMeshProUGUI textPriceCurrentZoneUnlock;


    // --- Eventos de Lógica de Negocio ---
    private static event Action onStartTabern, onCloseTabern;
    public static Action OnStartTabern { get => onStartTabern; set => onStartTabern = value; }
    public static Action OnCloseTabern { get => onCloseTabern; set => onCloseTabern = value; }
    
    private static event Action<GameObject> onSetSelectedCurrentGameObject;
    private static event Action onClearSelectedCurrentGameObject;
    public static Action<GameObject> OnSetSelectedCurrentGameObject { get => onSetSelectedCurrentGameObject; set => onSetSelectedCurrentGameObject = value; }
    public static Action OnClearSelectedCurrentGameObject { get => onClearSelectedCurrentGameObject; set => onClearSelectedCurrentGameObject = value; }

    private PlayerModel playerModel;
    private GameObject lastSelectedButtonFromAdminPanel;
    private bool ignoreFirstButtonSelected = true;
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
        if (panelAnimator != null)
        {
            panelAnimator.OnAnimateInComplete.RemoveListener(SetupInitialTab);
            panelAnimator.OnAnimateOutComplete.RemoveListener(CleanupAfterAnimation);
        }
        UnsuscribeToUpdateManagerEvent();
        UnscribeToPauseManagerRestoreSelectedGameObjectEvent();
    }

    #region Gestión de Foco (Pausa y Sonido)

    // Añade esta simulación de Update de vuelta
    void UpdateAdministratingManagerUI()
    {
        CheckLastSelectedButtonIfAdminPanelIsOpen();
    }

    // Función para reproducir sonido de selección
    public void PlayAudioButtonSelectedWhenChangeSelectedGameObjectExceptFirstTime()
    {
        if (!ignoreFirstButtonSelected)
        {
            AudioManager.Instance.PlayOneShotSFX("ButtonSelected");
            return;
        }
        ignoreFirstButtonSelected = false;
    }

    // Suscripción al Update
    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdateAdministratingManagerUI;
    }
    private void UnsuscribeToUpdateManagerEvent()
    {
        if (UpdateManager.Instance != null)
            UpdateManager.OnUpdate -= UpdateAdministratingManagerUI;
    }

    // Suscripción al evento de Pausa
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
        // Usamos el PlayerModel como la "fuente de verdad" del estado
        if (playerModel != null && playerModel.IsAdministrating)
        {
            ignoreFirstButtonSelected = true;
            DeviceManager.Instance.IsUIModeActive = true; // Re-confirma el modo UI
            onSetSelectedCurrentGameObject?.Invoke(lastSelectedButtonFromAdminPanel);
        }
    }

    private void CheckLastSelectedButtonIfAdminPanelIsOpen()
    {
        if (EventSystem.current != null && PauseManager.Instance != null && !PauseManager.Instance.IsGamePaused &&
            playerModel != null && playerModel.IsAdministrating)
        {
            // Solo guardar si el botón actual es parte de este panel
            if (EventSystem.current.currentSelectedGameObject != null &&
                EventSystem.current.currentSelectedGameObject.transform.IsChildOf(this.transform))
            {
                lastSelectedButtonFromAdminPanel = EventSystem.current.currentSelectedGameObject;
            }
        }
    }
    #endregion

    #region Funciones de Lógica de Negocio (Asignadas a Botones)
    public void ButtonStartTabern()
    {
        onStartTabern?.Invoke();
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
    }
    public void ButtonCloseTabern()
    {
        onCloseTabern?.Invoke();
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
    }

    public void ButtonExit()
    {
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        HandlePlayerExitAdmin();
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

    #region Gestión de Apertura/Cierre del Panel

    private void SetupInitialTab()
    {
        DeviceManager.Instance.IsUIModeActive = true;
        ignoreFirstButtonSelected = true;
        if (tabGroup != null)
        {
            if (tabGroup.StartingSelectedButton != null)
            {
                tabGroup.OnTabSelected(tabGroup.StartingSelectedButton);
            }
            else
            {
                tabGroup.SelectTabByIndex(0);
            }
        }
    }
    private void CleanupAfterAnimation()
    {
        DeviceManager.Instance.IsUIModeActive = false;
        onClearSelectedCurrentGameObject?.Invoke();

        panelTabern.SetActive(false);
        panelIngredients.SetActive(false);
        panelUpgrades.SetActive(false);
    }
    private void PrepareInitialUIState()
    {
        int initialTabIndex = 0;
        TabTweenButton initialButton = tabGroup?.StartingSelectedButton;
        if (tabGroup != null && initialButton != null)
        {
            initialTabIndex = tabGroup.GetButtonIndex(initialButton);
            if (initialTabIndex < 0) initialTabIndex = 0;
        }

        int upgradesButtonIndex = 2;
        if (initialTabIndex == upgradesButtonIndex)
        {
            ShowCurrentZoneInformation(0);
        }
    }
    private void HandlePlayerEnterAdmin()
    {
        PrepareInitialUIState();
        if (panelAnimator != null)
        {
            panelAnimator.AnimateIn();
        }
    }
    private void HandlePlayerExitAdmin()
    {
        if (panelAnimator != null)
        {
            panelAnimator.AnimateOut();
        }
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
        PlayerView.OnEnterInAdministrationMode += HandlePlayerEnterAdmin;
        PlayerView.OnExitInAdministrationMode += HandlePlayerExitAdmin;
    }

    private void UnsuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInAdministrationMode -= HandlePlayerEnterAdmin;
        PlayerView.OnExitInAdministrationMode -= HandlePlayerExitAdmin;
    }

    private void GetComponents()
    {
        if (panelAnimator == null)
            Debug.LogError("No se ha asignado el AdminUIAppear en el AdministratingManagerUI", this);
        if (tabGroup == null)
        {
            tabGroup = GetComponent<TabGroup>();
            if (tabGroup == null) Debug.LogError("No se encontró el TabGroup", this);
        }

        GameObject ZonesToUnlockFather = GameObject.Find("ZonesToUnlock");
        if (ZonesToUnlockFather != null)
        {
            foreach (Transform childs in ZonesToUnlockFather.transform)
            {
                zoneUnlocks.Add(childs.GetComponent<ZoneUnlock>());
            }
        }
        playerModel = FindFirstObjectByType<PlayerModel>();
    }

    #endregion
}