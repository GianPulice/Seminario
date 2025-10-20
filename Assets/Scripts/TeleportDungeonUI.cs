using System;
using TMPro;
using UnityEngine;
using System.Collections;

public class TeleportDungeonUI : MonoBehaviour, IInteractable
{
    private PlayerModel playerModel;

    private GameObject leftDoor;
    private GameObject rightDoor;

    private static event Action onShowTeleportPanel;

    public static Action OnShowTeleportPanel { get => onShowTeleportPanel; set => onShowTeleportPanel = value; }

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
        onShowTeleportPanel?.Invoke();
        playerModel.StopVelocity();
        playerModel.IsInTeleportPanel = true;
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

    public void ShowMessage(TextMeshProUGUI interactionManagerUIText)
    {
        string keyText = $"<color=yellow> {PlayerInputs.Instance.GetInteractInput()} </color>";
        interactionManagerUIText.text = $"Press" + keyText + "to Teleport";
    }

    public void HideMessage(TextMeshProUGUI interactionManagerUIText)
    {
        interactionManagerUIText.text = string.Empty;
    }


    private void GetComponents()
    {
        playerModel = FindFirstObjectByType<PlayerModel>();
        leftDoor = transform.Find("LeftDoor").gameObject;
        rightDoor = transform.Find("RightDoor").gameObject;
    }

    private IEnumerator RegisterOutline()
    {
        yield return new WaitUntil(() => OutlineManager.Instance != null);

        OutlineManager.Instance.Register(gameObject);
    }
}
