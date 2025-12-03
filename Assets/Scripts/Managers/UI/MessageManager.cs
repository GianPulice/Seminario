using System.Collections;
using TMPro;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private GameObject allUpgradesPanel;
    [SerializeField] private TextMeshProUGUI text;

    private Coroutine autoHideCoroutine;
    private int idleTweenId = -1;
    void OnEnable()
    {
        PlayerView.OnEnterInCookMode += HideMessage;
        PlayerView.OnEnterInAdministrationMode += HideMessage;
   
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

        if (UpgradesManager.Exists)
        {
            UpgradesManager.Instance.OnCanPurchaseStatusChanged -= HandleUpgradeMessage;
            UpgradesManager.Instance.OnAllUpgradesCompleted -= HandleAllUpgradesPanel;
        }
        StopIdleAnim();
    }
    public void CloseButton()
    {
        HideMessage();
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
        if (autoHideCoroutine != null)
            StopCoroutine(autoHideCoroutine);

        messagePanel.SetActive(true);
        LeanTween.scale(messagePanel, Vector3.zero, 0.25f)
             .setIgnoreTimeScale(true)
             .setEase(LeanTweenType.easeOutQuad);
        StartIdleAnim();
        autoHideCoroutine = StartCoroutine(AutoHide());
    }

    private void HideMessage()
    {
        if (!messagePanel.activeSelf) return;

        StopIdleAnim();
        LeanTween.cancel(messagePanel);
        LeanTween.scale(messagePanel, Vector3.zero, 0.25f)
              .setIgnoreTimeScale(true)
              .setEase(LeanTweenType.easeOutQuad) 
              .setOnComplete(() => messagePanel.SetActive(false));

        if (autoHideCoroutine != null)
            StopCoroutine(autoHideCoroutine);
    }

    private IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(15f);
        HideMessage();
    }

    private void HandleAllUpgradesPanel()
    {
        allUpgradesPanel.SetActive(true);
        LeanTween.scale(allUpgradesPanel, Vector3.one, 0.3f).setFrom(Vector3.zero);
    }
}
