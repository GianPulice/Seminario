using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class EventSystemMainMenu : MonoBehaviour
{
    private EventSystem eventSystem;

    [SerializeField] private List<GameObject> buttonsMainMenu = new List<GameObject>();


    void Awake()
    {
        SuscribeToMainMenuEvent();
        GetComponents();
        StartCoroutine(InitializeFirstSelectedButtonByDefault());
    }


    void OnDestroy()
    {
        UnsuscribeToMainMenuEvent();
    }


    private void SuscribeToMainMenuEvent()
    {
        MainMenu.OnSendButtonsToEventSystem += GetButtonsMainMenuFromEvent;
    }

    private void UnsuscribeToMainMenuEvent()
    {
        MainMenu.OnSendButtonsToEventSystem -= GetButtonsMainMenuFromEvent;
    }

    private void GetComponents()
    {
        eventSystem = GetComponent<EventSystem>();
    }

    private void GetButtonsMainMenuFromEvent(List<GameObject> buttonsMainMenu)
    {
        this.buttonsMainMenu = buttonsMainMenu;
    }

    private IEnumerator InitializeFirstSelectedButtonByDefault()
    {
        yield return new WaitUntil(() => SceneManager.GetSceneByName("MainMenuUI").isLoaded);

        yield return new WaitUntil(() => buttonsMainMenu != null && buttonsMainMenu.Count > 0);

        eventSystem.SetSelectedGameObject(buttonsMainMenu[0]);
    }
}
