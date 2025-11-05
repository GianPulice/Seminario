using UnityEngine;
using UnityEngine.EventSystems;

public class TrashUI : MonoBehaviour
{
    private PlayerModel playerModel;

    [SerializeField] private GameObject panelTrash;
    [SerializeField] private GameObject contentTrash;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutBack;
    [SerializeField] private float timeToScale = 0.2f;

    private bool isAnimating = false;


    void Awake()
    {
        panelTrash.SetActive(false);
        SuscribeToTrashEvent();
        GetComponents();
    }


    public void Yes()
    {
        if (isAnimating) return;

        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        PlayerController.OnThrowFoodToTrash?.Invoke();
        Trash.OnHidePanelTrash?.Invoke();
    }

    public void No()
    {
        if (isAnimating) return;

        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        Trash.OnHidePanelTrash?.Invoke();
    }


    private void SuscribeToTrashEvent()
    {
        Trash.OnShowPanelTrash += OnShowPanelTrash;
        Trash.OnHidePanelTrash += OnHidePanelTrash;
    }

    private void OnShowPanelTrash()
    {
        if (isAnimating || panelTrash.activeSelf) return;

        isAnimating = true;
        DeviceManager.Instance.IsUIModeActive = true;
        panelTrash.SetActive(true);

        contentTrash.transform.localScale = Vector3.zero;

        LeanTween.scale(contentTrash, Vector3.one, timeToScale)
            .setEase(easeType)
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                isAnimating = false;
            });
    }
    private void OnHidePanelTrash()
    {
        if (isAnimating || !panelTrash.activeSelf) return;

        isAnimating = true;

        LeanTween.scale(contentTrash, Vector3.zero, timeToScale)
            .setEase(easeType)
            .setIgnoreTimeScale(true)
            .setOnComplete(OnHideAnimationComplete);
    }
    private void OnHideAnimationComplete()
    {
        isAnimating = false;
        playerModel.IsInTrashPanel = false;
        panelTrash.SetActive(false);
        DeviceManager.Instance.IsUIModeActive = false;
        EventSystem.current.SetSelectedGameObject(null);
    }
    private void GetComponents()
    {
        playerModel = FindFirstObjectByType<PlayerModel>();

    }
    private void OnDestroy()
    {
        Trash.OnShowPanelTrash -= OnShowPanelTrash;
        Trash.OnHidePanelTrash -= OnHidePanelTrash;
    }
}
