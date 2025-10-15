using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseManager : Singleton<PauseManager>
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private List<Button> buttonsPause;

    private static event Action<GameObject> onSetSelectedCurrentGameObject;
    private static event Action onClearSelectedCurrentGameObject;
    private static event Action onButtonSettingsClickToShowCorrectPanel;

    private static event Action onRestoreSelectedGameObject; // Este evento es generico y sirve para todos los paneles de UI que esten abiertos cuando se pausa el juego

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

    // Simulacion de Update
    void UpdatePauseManager()
    {
        EnabledOrDisabledPausePanel();
    }

    void OnDestroy()
    {
        UnsuscribeToUpdateManagerEvent();
    }


    // Funcion asignada a botones en la UI para setear el selected GameObject del EventSystem con Mouse
    public void SetButtonAsSelectedGameObjectIfHasBeenHover(int indexButton)
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(buttonsPause[indexButton].gameObject);
        }
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
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        StartCoroutine(ExitGameAfterSeconds());
    }

    public void ButtonBack()
    {
        ignoreFirstSelectedSound = true;
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        HideSettings();
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
        foreach (var button in buttonsPause)
        {
            button.gameObject.SetActive(true);
        }

        onSetSelectedCurrentGameObject?.Invoke(buttonsPause[0].gameObject);
        Time.timeScale = 0f;
        isGamePaused = true;
        pausePanel.SetActive(true);
        DeviceManager.Instance.IsUIModeActive = true;
    }

    private void HidePause()
    {
        onClearSelectedCurrentGameObject?.Invoke();
        Time.timeScale = 1f;
        isGamePaused = false;
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        DeviceManager.Instance.IsUIModeActive = false;

        onRestoreSelectedGameObject?.Invoke();
    }

    private void ShowSettings()
    {
        foreach (var button in buttonsPause)
        {
            button.gameObject.SetActive(false);
        }

        settingsPanel.SetActive(true);
        onButtonSettingsClickToShowCorrectPanel?.Invoke();
    }

    private void HideSettings()
    {
        foreach (var button in buttonsPause)
        {
            button.gameObject.SetActive(true);
        }

        settingsPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(buttonsPause[1].gameObject);
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
