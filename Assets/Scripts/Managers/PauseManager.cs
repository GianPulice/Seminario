using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseManager : Singleton<PauseManager>
{
    [Header("UI")]
    [SerializeField] private PauseAppear pausePanel;
    [SerializeField] private GameObject pauseOpacity;
    [SerializeField] private GameObject pauseText;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject tutorialPanel;

    [Header("UI Elements")]
    [Tooltip("El GameObject que contiene TODOS los botones principales de pausa (Resume, Settings, etc.)")]
    [SerializeField] private GameObject pauseButtonsContainer;
    [Tooltip("El primer botón que debe seleccionarse al abrir la pausa (Ej: el botón 'Resume')")]
    [SerializeField] private GameObject firstSelectedButton;
    [Tooltip("El botón que debe seleccionarse al volver de Settings (Ej: el botón 'Settings')")]
    [SerializeField] private GameObject settingsSelectedButton;
    [SerializeField] private GameObject tutorialSelectedButton;

    private PlayerModel playerModel;

    // --- Eventos ---
    private static event Action<GameObject> onSetSelectedCurrentGameObject;
    private static event Action onClearSelectedCurrentGameObject;
    private static event Action onButtonSettingsClickToShowCorrectPanel;
    private static event Action onRestoreSelectedGameObject;

    private static event Action onGamePaused;
    private static event Action onGameUnPaused;

    private bool isGamePaused = false;
    private bool ignoreFirstSelectedSound = false;
    private bool wasUiActiveOnPause = false;
    private bool ignorePauseThisFrame = false;

    public static Action<GameObject> OnSetSelectedCurrentGameObject { get => onSetSelectedCurrentGameObject; set => onSetSelectedCurrentGameObject = value; }
    public static Action OnClearSelectedCurrentGameObject { get => onClearSelectedCurrentGameObject; set => onClearSelectedCurrentGameObject = value; }
    public static Action OnButtonSettingsClickToShowCorrectPanel { get => onButtonSettingsClickToShowCorrectPanel; set => onButtonSettingsClickToShowCorrectPanel = value; }
    public static Action OnRestoreSelectedGameObject { get => onRestoreSelectedGameObject; set => onRestoreSelectedGameObject = value; }

    public static Action OnGamePaused { get => onGamePaused; set => onGamePaused = value; }
    public static Action OnGameUnPaused { get => onGameUnPaused; set => onGameUnPaused = value; }

    public bool IsGamePaused { get => isGamePaused; }
    
    
    void Awake()
    {
        CreateSingleton(false);
        SuscribeToUpdateManagerEvent();
        SuscribeToPlayerViewEvents();
        GetComponents();
    }

    void UpdatePauseManager()
    {
        EnabledOrDisabledPausePanel();
    }

    void OnDestroy()
    {
        UnsuscribeToUpdateManagerEvent();
        UnsuscribeToPlayerViewEvents();
    }

    // Funcion asignada a botones en la UI para reproducir el sonido selected
    public void PlayAudioButtonSelectedWhenChangeSelectedGameObjectExceptFirstTime()
    {
        if (!ignoreFirstSelectedSound)
        {
            AudioManager.Instance.PlayOneShotSFX("ButtonSelected");
            return;
        }

        ignoreFirstSelectedSound = false;
    }

    // Funciones asignadas a botones de la UI
    public void ButtonResume()
    {
        AudioManager.Instance.PlayOneShotSFX("Admin/Cook/Pause");
        HidePause();
    }

    public void ButtonSettings()
    {
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        ShowSettings();
    }
    public void ButtonTutorial()
    {
        if (!tutorialSelectedButton.activeSelf) return;
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        ShowTutorial();
    }
    public void ButtonMainMenu()
    {
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        Time.timeScale = 1f;

        SaveLastSceneName();
        GameManager.Instance.GameSessionType = GameSessionType.None;
        string[] additiveScenes = { "MainMenuUI" };
        StartCoroutine(loadSceneAfterSeconds("MainMenu", additiveScenes));
    }

    public void ButtonExit()
    {
        SaveLastSceneName();
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        StartCoroutine(ExitGameAfterSeconds());
    }

    public void ButtonBack()
    {
        ignoreFirstSelectedSound = true;
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        HideSettings();
        HideTutorial();
    }
    public void OnPausePanelShowComplete()
    {
        onSetSelectedCurrentGameObject?.Invoke(firstSelectedButton);
        ignoreFirstSelectedSound = true;
        DeviceManager.Instance.IsUIModeActive = true;
    }

    public void OnPausePanelHideComplete()
    {
        onRestoreSelectedGameObject?.Invoke();
        pauseOpacity.SetActive(false);
    }

    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdatePauseManager;
    }

    private void UnsuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate -= UpdatePauseManager;
    }

    private void SuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInAdministrationMode += OnEnterInUIMode;
        PlayerView.OnExitInAdministrationMode += OnExitInUIMode;

        PlayerView.OnEnterInCookMode += OnEnterInUIMode;
        PlayerView.OnExitInCookMode += OnExitInUIMode;

        PlayerView.OnEnterTutorial += OnEnterInUIMode;
        PlayerView.OnExitTutorial += OnExitInUIMode;

        Trash.OnShowPanelTrash += OnEnterInUIMode;
        Trash.OnHidePanelTrash += OnExitInUIMode;
    }

    private void UnsuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInAdministrationMode -= OnEnterInUIMode;
        PlayerView.OnExitInAdministrationMode -= OnExitInUIMode;

        PlayerView.OnEnterInCookMode -= OnEnterInUIMode;
        PlayerView.OnExitInCookMode -= OnExitInUIMode;

        PlayerView.OnEnterTutorial -= OnEnterInUIMode;
        PlayerView.OnExitTutorial -= OnExitInUIMode;

        Trash.OnShowPanelTrash -= OnEnterInUIMode;
        Trash.OnHidePanelTrash -= OnExitInUIMode;
    }

    private void OnEnterInUIMode()
    {
        ignorePauseThisFrame = true;
    }

    private void OnExitInUIMode()
    {
        //if (!isGamePaused) return;
        
        if (playerModel.IsAdministrating || playerModel.IsCooking || playerModel.IsInTrashPanel || playerModel.IsInTutorial) return;

        StartCoroutine(ExitUIMode());
    }

    private IEnumerator ExitUIMode()
    {
        yield return null;

        ignorePauseThisFrame = false;
    }

    private void GetComponents()
    {
        playerModel = FindFirstObjectByType<PlayerModel>();
    }
    private bool ShouldStayInUIMode()
    {
        if (playerModel == null) return false;

        if (playerModel.IsAdministrating) return true;
        if (playerModel.IsCooking) return true;
        if (playerModel.IsInTrashPanel) return true;
        if (playerModel.IsInTutorial) return true;
       // if (IngredientInventoryManagerUI.OnInventoryOpen) return true;
        

        return false;
    }
    private void ShowPause()
    {
        DeviceManager.Instance.IsUIModeActive = true;

        if(InteractionManagerUI.Exists)
        {
            InteractionManagerUI.instance.ShowOrHideCenterPointUI(false);
            if(InteractionManagerUI.instance.MessageAnimator!= null)
            {
                InteractionManagerUI.instance.MessageAnimator.HideInstantly();
            }
        }
        onGamePaused?.Invoke();
            
        bool allTutorialsSeen = TutorialListener.Instance.AllTutorialsSeen();
        tutorialSelectedButton.SetActive(allTutorialsSeen);
        
        AudioManager.Instance.PauseCurrentMusic();
        StartCoroutine(AudioManager.Instance.PlayMusic("Pause"));
        pauseOpacity.SetActive(true);
        pauseButtonsContainer.SetActive(true);

        Time.timeScale = 0f;
        isGamePaused = true;
       
        pausePanel.AnimateIn();
    }

    private void HidePause()
    {
        onGameUnPaused?.Invoke();
        Time.timeScale = 1f;
        isGamePaused = false;

        bool stayInUi = ShouldStayInUIMode();

        DeviceManager.Instance.IsUIModeActive = stayInUi;

        if (!stayInUi)
        {
            if (InteractionManagerUI.Instance != null)
            {
                InteractionManagerUI.Instance.ForceResetUI();
            }
        }

        //No Borrar nunca
        onRestoreSelectedGameObject?.Invoke();

        pausePanel.AnimateOut();
        
        AudioManager.Instance.StopMusic("Pause");
        AudioManager.Instance.ResumeLastMusic();
        
        if (settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void ShowSettings()
    {
        pauseButtonsContainer.SetActive(false);
        pauseText.SetActive(false);
        settingsPanel.SetActive(true);
        onButtonSettingsClickToShowCorrectPanel?.Invoke();
    }

    private void HideSettings()
    {
        pauseButtonsContainer.SetActive(true);
        pauseText.SetActive(true);
        settingsPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(settingsSelectedButton);
    }
    private void ShowTutorial()
    {
        tutorialPanel.SetActive(true);
    }
    private void HideTutorial()
    {
        tutorialPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(tutorialSelectedButton);
    }

    private void EnabledOrDisabledPausePanel()
    {
        if (isGamePaused)
        {
            if (PlayerInputs.Instance.Pause())
            {
                AudioManager.Instance.PlayOneShotSFX("Admin/Cook/Pause");
                HidePause();
                return;
            }
        }

        if (PlayerInputs.Instance.PauseWithKeyP())
        {
            AudioManager.Instance.PlayOneShotSFX("Admin/Cook/Pause");
            (isGamePaused ? (Action)HidePause : ShowPause)();
            return;
        }

        if (ignorePauseThisFrame) return;

        if (PlayerInputs.Instance.Pause() && !playerModel.IsAdministrating && !playerModel.IsCooking && !playerModel.IsInTrashPanel && !playerModel.IsInTutorial)
        {
            AudioManager.Instance.PlayOneShotSFX("Admin/Cook/Pause");
            (isGamePaused ? (Action)HidePause : ShowPause)();
        }
    }

    private void SaveLastSceneName()
    {
        SaveData data = SaveSystemManager.LoadGame();
        data.lastSceneName = ScenesManager.Instance.CurrentSceneName;
        SaveSystemManager.SaveGame(data);
    }

    private IEnumerator loadSceneAfterSeconds(string sceneName, string[] sceneNameAdditive)
    {
        yield return StartCoroutine(ScenesManager.Instance.LoadScene(sceneName, sceneNameAdditive));
    }

    private IEnumerator ExitGameAfterSeconds()
    {
        yield return StartCoroutine(ScenesManager.Instance.ExitGame());
    }
}
