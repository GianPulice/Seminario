using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private List<Button> buttonsMainMenu;

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
    }


    // Funcion asignada a botones en la UI para setear el selected GameObject del EventSystem con Mouse
    public void SetButtonAsSelectedGameObjectIfHasBeenHover(int indexButton)
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(buttonsMainMenu[indexButton].gameObject);
        }
    }

    // Funcion asignada a botones en la UI para reproducir el sonido selected excepto la primero vez
    public void PlayAudioButtonSelectedWhenChangeSelectedGameObjectExceptFirstTime()
    {
        if (!ignoreFirstButtonSelected)
        {
            AudioManager.Instance.PlaySFX("ButtonSelected");
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
        AudioManager.Instance.PlaySFX("ButtonClickWell");

        foreach (var button in buttonsMainMenu)
        {
            button.gameObject.SetActive(false);
        }

        panelSettings.SetActive(true);
        onButtonSettingsClickToShowCorrectPanel?.Invoke();
    }

    // Funcion asignada a boton en la UI
    public void ButtonCredits()
    {
        AudioManager.Instance.PlaySFX("ButtonClickWell");
    }

    // Funcion asignada a boton en la UI
    public void ButtonExit()
    {
        DeviceManager.Instance.IsUIModeActive = false;
        StartCoroutine(CloseGameAfterClickButton());
    }

    // Funcion asignada a boton en la UI
    public void ButtonBack()
    {
        ignoreFirstButtonSelected = true;

        AudioManager.Instance.PlaySFX("ButtonClickWell");

        foreach (var button in buttonsMainMenu)
        {
            button.gameObject.SetActive(true);
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
        if (!SaveSystemManager.SaveExists())
        {
            buttonsMainMenu[1].gameObject.SetActive(false);

            Navigation nav0 = buttonsMainMenu[0].navigation;
            nav0.mode = Navigation.Mode.Explicit;
            nav0.selectOnDown = buttonsMainMenu[2];
            buttonsMainMenu[0].navigation = nav0;

            // Ajusto la navegación de Settings (índice 2) para que vuelva hacia NewGame (índice 0)
            Navigation nav2 = buttonsMainMenu[2].navigation;
            nav2.mode = Navigation.Mode.Explicit;
            nav2.selectOnUp = buttonsMainMenu[0];
            buttonsMainMenu[2].navigation = nav2;
        }
    }

    private IEnumerator LoadSceneAfterButtonClick()
    {
        AudioManager.Instance.PlaySFX("ButtonClickWell");

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
}
