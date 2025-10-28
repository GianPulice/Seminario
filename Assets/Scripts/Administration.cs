using System.Collections;
using TMPro;
using UnityEngine;

public class Administration : MonoBehaviour, IInteractable
{
    private PlayerController playerController;

    public InteractionMode InteractionMode { get => InteractionMode.Press; }


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
        SetAdministratingTrue();
    }

    public void ShowOutline()
    {
        OutlineManager.Instance.ShowWithDefaultColor(gameObject);
        InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Interactive);
    }

    public void HideOutline()
    {
        OutlineManager.Instance.Hide(gameObject);
        InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Normal);
    }

   
    public bool TryGetInteractionMessage(out string message)
    {
        string keyText = $"<color=yellow> {PlayerInputs.Instance.GetInteractInput()} </color>";
        message = $"Press {keyText} to enter administration";

        return true;
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
    private void SetAdministratingTrue()
    {
        playerController.PlayerModel.IsAdministrating = true;
    }
    private void SetAdministratingFalse()
    {
    playerController.PlayerModel.IsAdministrating = false;
    }
}
