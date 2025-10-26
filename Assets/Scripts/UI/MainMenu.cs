using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private List<GenericTweenButton> buttonsMainMenu;

    //Cambiar aca despues de hacer el nuevo sistema de tabs.
    [SerializeField] private GameObject panelSettings;

    private static event Action<List<GameObject>> onSendButtonsToEventSystem;
    private static event Action onButtonSettingsClickToShowCorrectPanel;

    private bool ignoreFirstButtonSelected = true;

    public static Action<List<GameObject>> OnSendButtonsToEventSystem { get => onSendButtonsToEventSystem; set => onSendButtonsToEventSystem = value; }
    public static Action OnButtonSettingsClickToShowCorrectPanel { get => onButtonSettingsClickToShowCorrectPanel; set => onButtonSettingsClickToShowCorrectPanel = value; }


    void Awake()
    {
        InvokeEventToSendButtonsReferences();
    }

    void Start()
    {
        InitializeLoadGameButtonIfLoadDataExists();
        StartCoroutine(PlayMainMenuMusic());
    }

    public void SetSelectedGameObject(GameObject go)
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(go);
        }
    }
    public void DeselectCurrentGameObject()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // Funcion asignada a botones en la UI para reproducir el sonido selected excepto la primero vez
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
    public void ButtonNewGame()
    {
        SaveSystemManager.DeleteAllData();
        GameManager.Instance.GameSessionType = GameSessionType.New;
        GameManager.Instance.OnGameSessionStarted?.Invoke();
        DeviceManager.Instance.IsUIModeActive = false;
        StartCoroutine(LoadSceneAfterButtonClick());
    }

    // Funcion asignada a boton en la UI
    public void ButtonLoadGame()
    {
        GameManager.Instance.GameSessionType = GameSessionType.Load;
        GameManager.Instance.OnGameSessionStarted?.Invoke();
        DeviceManager.Instance.IsUIModeActive = false;
        StartCoroutine(LoadSceneAfterButtonClick());
    }

    // Funcion asignada a boton en la UI
    public void ButtonSettings()
    {
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");

        foreach (var button in buttonsMainMenu)
        {
            button.gameObject.SetActive(false);
        }

        panelSettings.SetActive(true);
        onButtonSettingsClickToShowCorrectPanel?.Invoke();
    }

    public void ButtonCredits()
    {
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
    }

    public void ButtonExit()
    {
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        DeviceManager.Instance.IsUIModeActive = false;
        StartCoroutine(CloseGameAfterClickButton());
    }

    public void ButtonBack()
    {
        ignoreFirstButtonSelected = true;
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");

        for (int i = 0; i < buttonsMainMenu.Count; i++)
        {
            if (i != 1) 
            {
                buttonsMainMenu[i].gameObject.SetActive(true);
            }
            else
            {
                if (SaveSystemManager.SaveExists())
                {
                    buttonsMainMenu[i].gameObject.SetActive(true);
                }
            }
        }

        panelSettings.SetActive(false);
        EventSystem.current.SetSelectedGameObject(buttonsMainMenu[2].gameObject);
    }


    private void InvokeEventToSendButtonsReferences()
    {
        onSendButtonsToEventSystem?.Invoke(buttonsMainMenu.ConvertAll(b => b.gameObject));
    }

    private void InitializeLoadGameButtonIfLoadDataExists()
    {
        if(!SaveSystemManager.SaveExists())
        {
            buttonsMainMenu[1].gameObject.SetActive(false);
        }
    }

    private IEnumerator LoadSceneAfterButtonClick()
    {
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");

        if (GameManager.Instance.GameSessionType == GameSessionType.Load && SaveSystemManager.SaveExists())
        {
            SaveData data = SaveSystemManager.LoadGame();
            string[] additiveScenes = { data.lastSceneName + "UI", "CompartidoUI" };
            yield return StartCoroutine(ScenesManager.Instance.LoadScene(data.lastSceneName, additiveScenes));
        }

        else
        {
            string[] additiveScenes = { "TabernUI", "CompartidoUI" };
            yield return StartCoroutine(ScenesManager.Instance.LoadScene("Tabern", additiveScenes));
        }
    }

    private IEnumerator CloseGameAfterClickButton()
    {
        yield return StartCoroutine(ScenesManager.Instance.ExitGame());
    }

    private IEnumerator PlayMainMenuMusic()
    {
        yield return new WaitUntil(() => AudioManager.Instance != null);

        StartCoroutine(AudioManager.Instance.PlayMusic("MainMenu"));
    }
}
