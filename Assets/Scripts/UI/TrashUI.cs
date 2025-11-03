using UnityEngine;

public class TrashUI : MonoBehaviour
{
    private PlayerModel playerModel;

    [SerializeField] private GameObject panelTrash;


    void Awake()
    {
        SuscribeToTrashEvent();
        GetComponents();
    }


    public void Yes()
    {
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        PlayerController.OnThrowFoodToTrash?.Invoke();
        Trash.OnHidePanelTrash?.Invoke();
        playerModel.IsInTrashPanel = false;
        panelTrash.SetActive(false);
        DeviceManager.Instance.IsUIModeActive = false;
    }

    public void No()
    {
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        Trash.OnHidePanelTrash?.Invoke();
        playerModel.IsInTrashPanel = false;
        panelTrash.SetActive(false);
        DeviceManager.Instance.IsUIModeActive = false;
    }


    private void SuscribeToTrashEvent()
    {
        Trash.OnShowPanelTrash += OnShowPanelTrash;
    }

    private void OnShowPanelTrash()
    {
        DeviceManager.Instance.IsUIModeActive = true;
        panelTrash.SetActive(true);
    }

    private void GetComponents()
    {
        playerModel = FindFirstObjectByType<PlayerModel>();
    }
}
