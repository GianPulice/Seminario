using UnityEngine;
using UnityEngine.UI;
using System;

public class ConfirmationPanel : MonoBehaviour
{
    [SerializeField] private GameObject[] childElementsInsideContainer; // Dejar de esta forma, porque sino tira error.
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private event Action onConfirm;


    void Awake()
    {
        foreach (GameObject panel in childElementsInsideContainer)
        {
            panel.gameObject.SetActive(false);
        }

        yesButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
            Hide();
        });

        noButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayOneShotSFX("ButtonClickWrong");
            Hide();
        });
    }


    public void Show(Action confirmAction)
    {
        foreach (GameObject panel in childElementsInsideContainer)
        {
            panel.gameObject.SetActive(true);
        }

        onConfirm = confirmAction;
    }

    public void Hide()
    {
        foreach (GameObject panel in childElementsInsideContainer)
        {
            panel.gameObject.SetActive(false);
        }

        onConfirm = null;
    }
}
