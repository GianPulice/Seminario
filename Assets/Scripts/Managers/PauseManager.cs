using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseManager : Singleton<PauseManager>
{
    [Header("UI")]
    [SerializeField] private PauseAppear pausePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("UI Elements")]
    [Tooltip("El GameObject que contiene TODOS los botones principales de pausa (Resume, Settings, etc.)")]
    [SerializeField] private GameObject pauseButtonsContainer;
    [Tooltip("El primer botón que debe seleccionarse al abrir la pausa (Ej: el botón 'Resume')")]
    [SerializeField] private GameObject firstSelectedButton;
    [Tooltip("El botón que debe seleccionarse al volver de Settings (Ej: el botón 'Settings')")]
    [SerializeField] private GameObject settingsSelectedButton;


    // --- Eventos (sin cambios) ---
    private static event Action<GameObject> onSetSelectedCurrentGameObject;
    private static event Action onClearSelectedCurrentGameObject;
    private static event Action onButtonSettingsClickToShowCorrectPanel;
    private static event Action onRestoreSelectedGameObject;

    private bool isGamePaused = false;
    private bool ignoreFirstSelectedSound = false;

    public static Action<GameObject> OnSetSelectedCurrentGameObject { get => onSetSelectedCurrentGameObject; set => onSetSelectedCurrentGameObject = value; }
    public static Action OnClearSelectedCurrentGameObject { get => onClearSelectedCurrentGameObject; set => onClearSelectedCurrentGameObject = value; }
    public static Action OnButtonSettingsClickToShowCorrectPanel { get => onButtonSettingsClickToShowCorrectPanel; set => onButtonSettingsClickToShowCorrectPanel = value; }
    public static Action OnRestoreSelectedGameObject { get => onRestoreSelectedGameObject; set => onRestoreSelectedGameObject = value; }

    public bool IsGamePaused { get => isGamePaused; }
    void Awake()
    {
        CreateSingleton(false);
        SuscribeToUpdateManagerEvent();
    }

    void UpdatePauseManager()
    {
        if (pausePanel == null)
        {
            Debug.LogWarning("PausePanel no está asignado en el PauseManager.");
            return;
        }
        if (pausePanel.IsAnimating)
        {
            return;
        }
        EnabledOrDisabledPausePanel();
    }

    void OnDestroy()
    {
        UnsuscribeToUpdateManagerEvent();
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
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        HidePause();
    }

    public void ButtonSettings()
    {
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        ShowSettings();
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
    }

    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdatePauseManager;
    }

    private void UnsuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate -= UpdatePauseManager;
    }

    private void ShowPause()
    {
        AudioManager.Instance.PauseCurrentMusic();
        StartCoroutine(AudioManager.Instance.PlayMusic("Pause"));
        pauseButtonsContainer.SetActive(true);

        Time.timeScale = 0f;
        isGamePaused = true;
       
        pausePanel.AnimateIn();
    }

    private void HidePause()
    {
        Time.timeScale = 1f;
        isGamePaused = false;
        DeviceManager.Instance.IsUIModeActive = false;
        onRestoreSelectedGameObject?.Invoke();

        onClearSelectedCurrentGameObject?.Invoke();
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
        settingsPanel.SetActive(true);
        onButtonSettingsClickToShowCorrectPanel?.Invoke();
    }

    private void HideSettings()
    {
        pauseButtonsContainer.SetActive(true);
        settingsPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(settingsSelectedButton);
    }

    private void EnabledOrDisabledPausePanel()
    {
        if (PlayerInputs.Instance.Pause())
        {
            AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
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
