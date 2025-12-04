using System.Collections;
using TMPro;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private GameObject allUpgradesPanel;
    [SerializeField] private TextMeshProUGUI text;

    [Header("Animation Settings")]
    [Tooltip("Distancia en píxeles que se moverá hacia arriba")]
    [SerializeField] private float verticalOffset = 50f;
    [SerializeField] private float animTime = 0.3f;

    private Coroutine autoHideCoroutine;
    private int idleTweenId = -1;

    private RectTransform messageRect;
    private Vector2 initialPos;
    void Awake()
    {
        messageRect = messagePanel.GetComponent<RectTransform>();
        initialPos = messageRect.anchoredPosition;
    }
    void OnEnable()
    {
        PlayerView.OnEnterInCookMode += HideMessage;
        PlayerView.OnEnterInAdministrationMode += HideMessage;
        PlayerView.OnExitInCookMode += CheckAndShowMessage;
        PlayerView.OnExitInAdministrationMode += CheckAndShowMessage;

        if (UpgradesManager.Exists)
        {
            UpgradesManager.Instance.OnCanPurchaseStatusChanged += HandleUpgradeMessage;
            UpgradesManager.Instance.OnAllUpgradesCompleted += HandleAllUpgradesPanel;
        }
    }

    void OnDisable()
    {
        PlayerView.OnEnterInCookMode -= HideMessage;
        PlayerView.OnEnterInAdministrationMode -= HideMessage;
        PlayerView.OnExitInCookMode -= CheckAndShowMessage;
        PlayerView.OnExitInAdministrationMode -= CheckAndShowMessage;
   
        if (UpgradesManager.Exists)
        {
            UpgradesManager.Instance.OnCanPurchaseStatusChanged -= HandleUpgradeMessage;
            UpgradesManager.Instance.OnAllUpgradesCompleted -= HandleAllUpgradesPanel;
        }
        StopIdleAnim();
        LeanTween.cancel(messagePanel);
    }
    public void CloseButton()
    {
        HideMessage();
    }
    private void CheckAndShowMessage()
    {
        if (UpgradesManager.Exists && UpgradesManager.Instance.reachedMoneyToPurchase)
        {
            ShowMessage();
        }
    }
    private void HandleUpgradeMessage(bool canPurchase)
    {
        if (canPurchase)
            ShowMessage();
        else
            HideMessage();
    }
    private void StartIdleAnim()
    {
        StopIdleAnim();
        idleTweenId = LeanTween.scale(text.rectTransform, Vector3.one * 1.05f, 0.6f)
            .setEaseInOutSine()
            .setLoopPingPong()
            .setIgnoreTimeScale(true)
            .id;
    }
    private void StopIdleAnim()
    {
        if (idleTweenId != -1)
        {
            LeanTween.cancel(idleTweenId);
            text.rectTransform.localScale = Vector3.one;
            idleTweenId = -1;
        }
    }
    private void ShowMessage()
    {
        if (UpgradesManager.Exists && UpgradesManager.Instance.AllUpgradesPurchased) return;
       
        if (autoHideCoroutine != null)
            StopCoroutine(autoHideCoroutine);

        messagePanel.SetActive(true);
        LeanTween.cancel(messagePanel);

        Vector2 startPos = initialPos + new Vector2(0, verticalOffset);
        messageRect.anchoredPosition = startPos;

        messagePanel.transform.localScale = Vector3.one;

        LeanTween.move(messageRect, initialPos, animTime)
            .setEaseOutBack() 
            .setIgnoreTimeScale(true);

        StartIdleAnim();
        autoHideCoroutine = StartCoroutine(AutoHide());
    }

    private void HideMessage()
    {
        if (!messagePanel.activeSelf) return;

        StopIdleAnim();
        if (autoHideCoroutine != null)
            StopCoroutine(autoHideCoroutine);

        LeanTween.cancel(messagePanel);

        Vector2 targetPos = initialPos + new Vector2(0, verticalOffset);

        LeanTween.move(messageRect, targetPos, 0.25f)
            .setEaseInBack() 
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                messagePanel.SetActive(false);
                messageRect.anchoredPosition = initialPos;
            });
    }

    private IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(15f);
        HideMessage();
    }

    private void HandleAllUpgradesPanel()
    {
        allUpgradesPanel.SetActive(true);
        LeanTween.cancel(allUpgradesPanel);
        LeanTween.scale(allUpgradesPanel, Vector3.one, 0.3f)
            .setFrom(Vector3.zero)
            .setEaseOutBack()
            .setIgnoreTimeScale(true);
    }
}
