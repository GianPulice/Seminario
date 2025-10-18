using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TeleportDungeonManagerUI : MonoBehaviour
{
    private PlayerModel playerModel;

    [SerializeField] private GameObject panelTeleport;
    [SerializeField] private List<GameObject> buttonsTeleportPanel;

    private GameObject lastSelectedButtonFromAdminPanel;

    private static event Action<GameObject> onSetSelectedCurrentGameObject;
    private static event Action onClearSelectedCurrentGameObject;

    private bool ignoreFirstButtonSelected = true;

    public static Action<GameObject> OnSetSelectedCurrentGameObject { get => onSetSelectedCurrentGameObject; set => onSetSelectedCurrentGameObject = value; }
    public static Action OnClearSelectedCurrentGameObject { get => onClearSelectedCurrentGameObject; set => onClearSelectedCurrentGameObject = value; }


    void Awake()
    {
        SuscribeToUpdateManagerEvent();
        SuscribeToTeleportDungeonUIEvents();
        SuscribeToPauseManagerRestoreSelectedGameObjectEvent();
        GetComponents();
    }

    // Simulacion de Update
    void UpdateTeleportDungeonManagerUI()
    {
        CheckLastSelectedButtonIfAdminPanelIsOpen();
    }

    void OnDestroy()
    {
        UnsuscribeToUpdateManagerEvent();
        UnsuscribeToTeleportDungeonUIEvents();
        UnscribeToPauseManagerRestoreSelectedGameObjectEvent();
    }


    // Funcion asignada a botones en la UI para setear el selected GameObject del EventSystem con Mouse
    public void SetButtonAsSelectedGameObjectIfHasBeenHover(int indexButton)
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(buttonsTeleportPanel[indexButton]);
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

    // Funcion asignada a boton en la UI
    public void ButtonYes()
    {
        DeviceManager.Instance.IsUIModeActive = false;
        string[] additiveScenes = { "DungeonUI", "CompartidoUI" };
        StartCoroutine(ScenesManager.Instance.LoadScene("Dungeon", additiveScenes));
    }

    // Funcion asignada a boton en la UI
    public void ButtonNo()
    {
        onClearSelectedCurrentGameObject?.Invoke();
        ignoreFirstButtonSelected = true;
        playerModel.IsInTeleportPanel = false;
        DeviceManager.Instance.IsUIModeActive = false;
        panelTeleport.SetActive(false);
    }


    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdateTeleportDungeonManagerUI;
    }

    private void UnsuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate -= UpdateTeleportDungeonManagerUI;
    }

    private void SuscribeToTeleportDungeonUIEvents()
    {
        TeleportDungeonUI.OnShowTeleportPanel += OnShowPanel;
    }

    private void UnsuscribeToTeleportDungeonUIEvents()
    {
        TeleportDungeonUI.OnShowTeleportPanel -= OnShowPanel;
    }

    private void SuscribeToPauseManagerRestoreSelectedGameObjectEvent()
    {
        PauseManager.OnRestoreSelectedGameObject += RestoreLastSelectedGameObjectIfGameWasPausedDuringAdministratingUI;
    }

    private void UnscribeToPauseManagerRestoreSelectedGameObjectEvent()
    {
        PauseManager.OnRestoreSelectedGameObject -= RestoreLastSelectedGameObjectIfGameWasPausedDuringAdministratingUI;
    }

    private void OnShowPanel()
    {
        DeviceManager.Instance.IsUIModeActive = true;
        panelTeleport.SetActive(true);
        onSetSelectedCurrentGameObject?.Invoke(buttonsTeleportPanel[0]);
    }

    private void RestoreLastSelectedGameObjectIfGameWasPausedDuringAdministratingUI()
    {
        if (panelTeleport.activeSelf)
        {
            ignoreFirstButtonSelected = true;
            DeviceManager.Instance.IsUIModeActive = true;
            EventSystem.current.SetSelectedGameObject(lastSelectedButtonFromAdminPanel);
        }
    }

    private void GetComponents()
    {
        playerModel = FindFirstObjectByType<PlayerModel>();
    }

    private void CheckLastSelectedButtonIfAdminPanelIsOpen()
    {
        if (EventSystem.current != null && PauseManager.Instance != null && !PauseManager.Instance.IsGamePaused && panelTeleport.activeSelf)
        {
            lastSelectedButtonFromAdminPanel = EventSystem.current.currentSelectedGameObject;
        }
    }
}
