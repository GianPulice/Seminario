using UnityEngine;
using UnityEngine.UI;


public class DeathUI : MonoBehaviour
{
    public Button retryButton;
    public Button mainMenuButton;
    public Button exitButton;

    private void Start()
    {

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryClicked);
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitClicked);
        }
    }

    private void OnDestroy()
    {
        if (retryButton != null) retryButton.onClick.RemoveAllListeners();
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveAllListeners();
        if (exitButton != null) exitButton.onClick.RemoveAllListeners();
    }

    // Métodos que se ejecutarán al hacer clic en los botones.
    private void OnRetryClicked()
    {
        gameObject.SetActive(false);

        //PlayerDungeonHUD.OnRetryGame?.Invoke();

        Debug.Log("Botón Reintentar presionado. Evento PlayerDungeonHUD.OnRetryGame invocado.");

    }

    private void OnMainMenuClicked()
    {
        Debug.Log("Botón Menú Principal presionado.");
        PauseManager.Instance.ButtonMainMenu();
    }

    private void OnExitClicked()
    {
        Debug.Log("Botón Salir presionado.");
        PauseManager.Instance.ButtonExit();
    }
}
