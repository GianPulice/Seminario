using UnityEngine;
using System.Collections;
using System;

public class Trash : MonoBehaviour, IInteractable
{
    private PlayerController playerController;

    private static event Action onShowPanelTrash;
    private static event Action onHidePanelTrash;

    public InteractionMode InteractionMode { get => InteractionMode.Press; }

    public static Action OnShowPanelTrash { get => onShowPanelTrash; set => onShowPanelTrash = value; }
    public static Action OnHidePanelTrash { get => onHidePanelTrash; set => onHidePanelTrash = value; }


    void Awake()
    {
        GetComponents();
        StartCoroutine(RegisterOutline());
    }

    void OnDestroy()
    {
        OutlineManager.Instance.Unregister(gameObject);
    }


    public void Interact(bool isPressed)
    {
        playerController.PlayerModel.IsInTrashPanel = true;
        onShowPanelTrash?.Invoke();
    }

    public void ShowOutline()
    {
        foreach (Transform child in playerController.PlayerView.Dish.transform)
        {
            // Verifica que las posiciones de la bandeja tengan hijos (COMIDAS)
            if (child.childCount > 0)
            {
                OutlineManager.Instance.ShowWithDefaultColor(gameObject);
                InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Interactive);
            }
        }
    }

    public void HideOutline()
    {
        OutlineManager.Instance.Hide(gameObject);
        InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Normal);
    }

    public bool TryGetInteractionMessage(out string message)
    {
        foreach (Transform child in playerController.PlayerView.Dish.transform)
        {
            if (child.childCount > 0)
            {
                string keyText = $"<color=yellow> {PlayerInputs.Instance.GetInteractInput()} </color>";
                message = $"Press" + keyText + "to throw food in the trash";
                return true;
            }
        }
        message = string.Empty;
        return false;
    }

    private void GetComponents()
    {
        playerController = FindFirstObjectByType<PlayerController>();
    }

    private IEnumerator RegisterOutline()
    {
        yield return new WaitUntil(() => OutlineManager.Instance != null);

        OutlineManager.Instance.Register(gameObject);
    }
}
