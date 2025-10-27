using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemTabern : MonoBehaviour
{
    void Awake()
    {
        SuscribeToUIEvents();
    }

    void OnDestroy()
    {
        UnsuscribeToUIEvents();
    }


    private void SuscribeToUIEvents()
    {
        PauseManager.OnSetSelectedCurrentGameObject += SetSelectedCurrentGameObject;
        PauseManager.OnClearSelectedCurrentGameObject += ClearCurrentSelectedGameObject;

        CookingManagerUI.OnSetSelectedCurrentGameObject += SetSelectedCurrentGameObject;
        CookingManagerUI.OnClearSelectedCurrentGameObject += ClearCurrentSelectedGameObject;

        AdministratingManagerUI.OnSetSelectedCurrentGameObject += SetSelectedCurrentGameObject;
        AdministratingManagerUI.OnClearSelectedCurrentGameObject += ClearCurrentSelectedGameObject;

        TeleportDungeonManagerUI.OnSetSelectedCurrentGameObject += SetSelectedCurrentGameObject;
        TeleportDungeonManagerUI.OnClearSelectedCurrentGameObject += ClearCurrentSelectedGameObject;
    }

    private void UnsuscribeToUIEvents()
    {
        PauseManager.OnSetSelectedCurrentGameObject -= SetSelectedCurrentGameObject;
        PauseManager.OnClearSelectedCurrentGameObject -= ClearCurrentSelectedGameObject;

        CookingManagerUI.OnSetSelectedCurrentGameObject -= SetSelectedCurrentGameObject;
        CookingManagerUI.OnClearSelectedCurrentGameObject -= ClearCurrentSelectedGameObject;

        AdministratingManagerUI.OnSetSelectedCurrentGameObject -= SetSelectedCurrentGameObject;
        AdministratingManagerUI.OnClearSelectedCurrentGameObject -= ClearCurrentSelectedGameObject;

        TeleportDungeonManagerUI.OnSetSelectedCurrentGameObject -= SetSelectedCurrentGameObject;
        TeleportDungeonManagerUI.OnClearSelectedCurrentGameObject -= ClearCurrentSelectedGameObject;
    }

    // Sirve para selecionar un GameObject
    private void SetSelectedCurrentGameObject(GameObject currentGameObject)
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(currentGameObject);
        }
    }

    // Sirve para limpiar el GameObject seleccionado, util para cuando salimos de modo UI
    private void ClearCurrentSelectedGameObject()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
