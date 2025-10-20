using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class AdministratingManagerUI : MonoBehaviour
{
    [SerializeField] private GameObject panelAdministrating;

    [SerializeField] private GameObject panelTabern;
    [SerializeField] private GameObject panelIngredients;
    [SerializeField] private GameObject panelUpgrades; 

    private List<ZoneUnlock> zoneUnlocks = new List<ZoneUnlock>();
    private Image currentImageZoneUnlock;
    private TextMeshProUGUI textPriceCurrentZoneUnlock;

    private List<Button> buttonsPanelAdministrating = new List<Button>(); // Botones de arriba de todo

    private List<GameObject> buttonsTabern = new List<GameObject>();
    private List<GameObject> buttonsIngredients = new List<GameObject>();
    private List<GameObject> buttonsUpgrades = new List<GameObject>();

    private GameObject lastSelectedButtonFromAdminPanel;

    private event Action onEnterAdmin, onExitAdmin;

    private static event Action<GameObject> onSetSelectedCurrentGameObject;
    private static event Action onClearSelectedCurrentGameObject;

    private static event Action onStartTabern;

    private bool ignoreFirstButtonSelected = true;

    public static Action<GameObject> OnSetSelectedCurrentGameObject { get => onSetSelectedCurrentGameObject; set => onSetSelectedCurrentGameObject = value; }
    public static Action OnClearSelectedCurrentGameObject { get => onClearSelectedCurrentGameObject; set => onClearSelectedCurrentGameObject = value; }

    public static Action OnStartTabern { get => onStartTabern; set => onStartTabern = value; }


    void Awake()
    {
        SuscribeToUpdateManagerEvent();
        InitializeLambdaEvents();
        SuscribeToPlayerViewEvents();
        SuscribeToPauseManagerRestoreSelectedGameObjectEvent();
        GetComponents();
    }

    // Simulacion de Update
    void UpdateAdministratingManagerUI()
    {
        CheckLastSelectedButtonIfAdminPanelIsOpen();
        CheckJoystickInputsToInteractWithPanels();
    }

    void OnDestroy()
    {
        UnsuscribeToUpdateManagerEvent();
        UnsuscribeToPlayerViewEvents();
        UnscribeToPauseManagerRestoreSelectedGameObjectEvent();
    }


    // Funcion asignada a botones en la UI para setear el selected GameObject del EventSystem con Mouse
    public void SetButtonAsSelectedGameObjectIfHasBeenHover(int indexButton)
    {
        if (EventSystem.current != null)
        {
            if (panelTabern.activeSelf) EventSystem.current.SetSelectedGameObject(buttonsTabern[indexButton]);
            if (panelIngredients.activeSelf) EventSystem.current.SetSelectedGameObject(buttonsIngredients[indexButton]);
            if (panelUpgrades.activeSelf) EventSystem.current.SetSelectedGameObject(buttonsUpgrades[indexButton]);
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

    /// <summary>
    /// Funcion hay que ajustarla en un futuro para que funcione con varias zonas de desbloqueo, actualmente funciona desde codigo
    /// </summary>
    public void ShowCurrentZoneInformation(int index)
    {
        currentImageZoneUnlock.sprite = zoneUnlocks[index].ZoneUnlockData.ImageZoneUnlock;
        textPriceCurrentZoneUnlock.text = "Price: " + zoneUnlocks[index].ZoneUnlockData.Cost.ToString();
    }

    // Funcion asignada a boton UI
    public void ButtonStartTabern()
    {
        onStartTabern?.Invoke();
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
    }

    // Funcion asignada a boton UI
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

    // Funcion asignada a boton UI
    public void ButtonUnlockNewZone(int index)
    {
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

    // Funciones asignadas a OnPointerEnter para el mouse y combinadas con los Inputs del joystick
    // El objetivo es que se abran los paneles correctamente y se cierren correctamente
    public void SetPanelTabern()
    {
        ColorBlock color = buttonsPanelAdministrating[0].colors;
        color.normalColor = new Color32(0xFF, 0xFF, 0xFF, 0xFF);

        if (buttonsPanelAdministrating[0].colors == color) 
        {
            onSetSelectedCurrentGameObject?.Invoke(buttonsTabern[0]);

            AudioManager.Instance.PlayOneShotSFX("ButtonSelected");
            SetButtonNormalColorInWhite(buttonsPanelAdministrating[1]);
            SetButtonNormalColorInWhite(buttonsPanelAdministrating[2]);
            panelIngredients.SetActive(false);
            panelUpgrades.SetActive(false);

            SetButtonNormalColorInGreen(buttonsPanelAdministrating[0]);
            panelTabern.SetActive(true);
        }
    }

    public void SetPanelIngredients()
    {
        // Color blanco
        ColorBlock color = buttonsPanelAdministrating[1].colors;
        color.normalColor = new Color32(0xFF, 0xFF, 0xFF, 0xFF);

        if (buttonsPanelAdministrating[1].colors == color)
        {
            // Aca se podria agregar que selecione el ultimo que tenia antes
            onSetSelectedCurrentGameObject?.Invoke(buttonsIngredients[0]);

            AudioManager.Instance.PlayOneShotSFX("ButtonSelected");

            SetButtonNormalColorInWhite(buttonsPanelAdministrating[0]);
            SetButtonNormalColorInWhite(buttonsPanelAdministrating[2]);
            panelTabern.SetActive(false);
            panelUpgrades.SetActive(false);

            SetButtonNormalColorInGreen(buttonsPanelAdministrating[1]);
            panelIngredients.SetActive(true);
        }
    }

    public void SetPanelUpgrades()
    {
        // Color blanco
        ColorBlock color = buttonsPanelAdministrating[2].colors;
        color.normalColor = new Color32(0xFF, 0xFF, 0xFF, 0xFF);

        if (buttonsPanelAdministrating[2].colors == color)
        {
            // Aca se podria agregar que selecione el ultimo que tenia antes
            onSetSelectedCurrentGameObject?.Invoke(buttonsUpgrades[0]);
            ShowCurrentZoneInformation(0);

            AudioManager.Instance.PlayOneShotSFX("ButtonSelected");

            SetButtonNormalColorInWhite(buttonsPanelAdministrating[0]);
            SetButtonNormalColorInWhite(buttonsPanelAdministrating[1]);
            panelTabern.SetActive(false);
            panelIngredients.SetActive(false);

            SetButtonNormalColorInGreen(buttonsPanelAdministrating[2]);
            panelUpgrades.SetActive(true);
        }
    }


    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdateAdministratingManagerUI;
    }

    private void UnsuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate -= UpdateAdministratingManagerUI;
    }

    private void InitializeLambdaEvents()
    {
        onEnterAdmin += () => ActiveOrDeactivatePanel(true);
        onExitAdmin += () => ActiveOrDeactivatePanel(false);
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
        foreach (Transform childs in panelAdministrating.transform)
        {
            buttonsPanelAdministrating.Add(childs.GetComponent<Button>());
        }

        FindGameObjectsReferences(panelTabern, buttonsTabern);
        FindGameObjectsReferences(panelIngredients, buttonsIngredients);
        FindGameObjectsReferences(panelUpgrades, buttonsUpgrades);

        GameObject ZonesToUnlockFather = GameObject.Find("ZonesToUnlock");
        foreach (Transform childs in ZonesToUnlockFather.transform)
        {
            zoneUnlocks.Add(childs.GetComponent<ZoneUnlock>());
        }

        currentImageZoneUnlock = panelUpgrades.transform.transform.Find("ImageBorderCurrentZone").transform.Find("ImageCurrentZone").GetComponent<Image>();
        textPriceCurrentZoneUnlock = panelUpgrades.transform.transform.Find("ImageBorderCurrentZone").transform.Find("TextPriceCurrentZone").GetComponent<TextMeshProUGUI>();
    }

    private void FindGameObjectsReferences(GameObject panel, List<GameObject> buttons)
    {
        foreach (Transform childs in panel.transform)
        {
            if (childs.GetComponent<Button>())
            {
                buttons.Add(childs.gameObject);
            }
        }
    }

    private void ActiveOrDeactivatePanel(bool state)
    {
        panelAdministrating.SetActive(state);

        if (state)
        {
            DeviceManager.Instance.IsUIModeActive = true;
            SetButtonNormalColorInGreen(buttonsPanelAdministrating[0]);

            panelTabern.SetActive(state);

            onSetSelectedCurrentGameObject?.Invoke(buttonsTabern[0]);
        }

        else
        {
            DeviceManager.Instance.IsUIModeActive = false;
            SetButtonNormalColorInWhite(buttonsPanelAdministrating[0]);
            SetButtonNormalColorInWhite(buttonsPanelAdministrating[1]);
            SetButtonNormalColorInWhite(buttonsPanelAdministrating[2]);

            panelTabern.SetActive(state);
            panelIngredients.SetActive(state);
            panelUpgrades.SetActive(state);

            ignoreFirstButtonSelected = true;
            onClearSelectedCurrentGameObject?.Invoke();
        }
    }

    private void RestoreLastSelectedGameObjectIfGameWasPausedDuringAdministratingUI()
    {
        if (panelAdministrating.activeSelf)
        {
            ignoreFirstButtonSelected = true;
            DeviceManager.Instance.IsUIModeActive = true;
            EventSystem.current.SetSelectedGameObject(lastSelectedButtonFromAdminPanel);
        }
    }

    private void CheckLastSelectedButtonIfAdminPanelIsOpen()
    {
        if (EventSystem.current != null && PauseManager.Instance != null && !PauseManager.Instance.IsGamePaused && panelAdministrating.activeSelf)
        {
            lastSelectedButtonFromAdminPanel = EventSystem.current.currentSelectedGameObject;
        }
    }

    private void SetButtonNormalColorInWhite(Button currentButton)
    {
        ColorBlock color = currentButton.colors;
        color.normalColor = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
        currentButton.colors = color;
    }

    private void SetButtonNormalColorInGreen(Button currentButton)
    {
        ColorBlock color = currentButton.colors;
        color.normalColor = new Color32(0x28, 0xFF, 0x00, 0xFF);
        currentButton.colors = color;
    }

    private void CheckJoystickInputsToInteractWithPanels()
    {
        if (panelAdministrating.activeSelf)
        {
            if (PlayerInputs.Instance != null)
            {
                if (PlayerInputs.Instance.R1())
                {
                    SetNexPanelUsingJoystickR1();
                }

                if (PlayerInputs.Instance.L1())
                {
                    SetNexPanelUsingJoystickL1();
                }
            }
        }
    }

    private void SetNexPanelUsingJoystickR1()
    {
        if (panelTabern.activeSelf)
        {
            SetPanelIngredients();
            return;
        }

        if (panelIngredients.activeSelf)
        {
            SetPanelUpgrades();
            return;
        }

        if (panelUpgrades.activeSelf)
        {
            SetPanelTabern();
            return;
        }
    }

    private void SetNexPanelUsingJoystickL1()
    {
        if (panelTabern.activeSelf)
        {
            SetPanelUpgrades();
            return;
        }

        if (panelIngredients.activeSelf)
        {
            SetPanelTabern();
            return;
        }

        if (panelUpgrades.activeSelf)
        {
            SetPanelIngredients();
            return;
        }
    }
}
