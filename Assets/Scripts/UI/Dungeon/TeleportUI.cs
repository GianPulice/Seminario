
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeleportUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject teleportConfirmUI;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private void Awake()
    {
        SubscribeEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void HandleShowTeleportConfirm(string message)
    {
        messageText.text = $"Do you want to {message.ToLower()}?";
        teleportConfirmUI.SetActive(true);

        DeviceManager.Instance.IsUIModeActive = true;
    }

    private void TeleportToLobby()
    {
        Debug.Log("[TeleportUIManager] Teleporting player to lobby...");

        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.TeleportToLobby();
        }
        else
        {
            Debug.LogWarning("[TeleportUIManager] DungeonManager instance not found!");
        }

        HideConfirmPanel();
    }

    private void HideConfirmPanel()
    {
        teleportConfirmUI.SetActive(false);

        if (DeviceManager.Instance != null)
        {
            DeviceManager.Instance.IsUIModeActive = false;
        }
        PlayerDungeonHUD.OnHideTeleportConfirm?.Invoke();
    }

    private void SubscribeEvents()
    {
        PlayerDungeonHUD.OnShowTeleportConfirm += HandleShowTeleportConfirm;
        confirmButton.onClick.AddListener(TeleportToLobby);
        cancelButton.onClick.AddListener(HideConfirmPanel);
    }

    private void UnsubscribeEvents()
    {
        PlayerDungeonHUD.OnShowTeleportConfirm -= HandleShowTeleportConfirm;
        confirmButton.onClick.RemoveListener(TeleportToLobby);
        cancelButton.onClick.RemoveListener(HideConfirmPanel);
    }
}
