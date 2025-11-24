using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoneyManagerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private RectTransform rectTransformToMove;
    
    [Header("Floating Text")]
    [SerializeField] private ObjectPooler floatingMoneyTextPool;
    [SerializeField] private Color tipColor = Color.green;
    [SerializeField] private Color deductionColor = Color.red;

    [Header("Animación de Posición")]
    [SerializeField] private Vector2 normalPosition = new Vector2(-200, 100);
    [SerializeField] private Vector2 adminPosition = new Vector2(0, 0);
    [SerializeField] private float animTime = 0.4f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutQuad;

    void Awake()
    {
        if (rectTransformToMove != null)
            rectTransformToMove.anchoredPosition = normalPosition;
        if (MoneyManager.Exists)
        {
            MoneyManager.Instance.OnMoneyUpdated += UpdateMoneyText;
            MoneyManager.Instance.OnMoneyTransaction += ShowFloatingMoneyText;

            UpdateMoneyText(MoneyManager.Instance.CurrentMoney);
        }
        SuscribeToPlayerViewEvents();
    }
    void OnDestroy()
    {
        UnsuscribeToPlayerViewEvents();

        if (MoneyManager.Exists)
        {
            MoneyManager.Instance.OnMoneyUpdated -= UpdateMoneyText;
            MoneyManager.Instance.OnMoneyTransaction -= ShowFloatingMoneyText;
        }
    }
    private void UpdateMoneyText(float currentMoney)
    {
        if (moneyText != null)
            moneyText.text = currentMoney.ToString("N0");
    }
    private void ShowFloatingMoneyText(float amount, bool positive)
    {
        if (floatingMoneyTextPool == null) return;

        FloatingMoneyText obj = floatingMoneyTextPool.GetObjectFromPool<FloatingMoneyText>();

        if (obj != null)
        {
            obj.transform.SetParent(moneyText.transform, false);

            string sign = positive ? "+" : "-";
            Color targetColor = positive ? tipColor : deductionColor;

            obj.Initialize(sign + amount.ToString(), targetColor);

            StartCoroutine(floatingMoneyTextPool.ReturnObjectToPool(obj, obj.MaxTimeToReturnObjectToPool));
        }
    }
    private void SuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInAdministrationMode += HandleEnterAdminMode;
        PlayerView.OnExitInAdministrationMode += HandleExitAdminMode;

        PlayerView.OnEnterInCookMode += HandleEnterCookMode;
        PlayerView.OnExitInCookMode += HandleExitCookMode;
    }
    private void UnsuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInAdministrationMode -= HandleEnterAdminMode;
        PlayerView.OnExitInAdministrationMode -= HandleExitAdminMode;

        PlayerView.OnEnterInCookMode -= HandleEnterCookMode;
        PlayerView.OnExitInCookMode -= HandleExitCookMode;
    }

    private void HandleEnterAdminMode()
    {
        if (rectTransformToMove == null) return;

        LeanTween.cancel(rectTransformToMove.gameObject); // Cancelar animación anterior
        LeanTween.move(rectTransformToMove, adminPosition, animTime)
            .setEase(easeType)
            .setIgnoreTimeScale(true); 

    }

    private void HandleExitAdminMode()
    {
        if (rectTransformToMove == null) return;

        LeanTween.cancel(rectTransformToMove.gameObject);
        LeanTween.move(rectTransformToMove, normalPosition, animTime)
            .setEase(easeType)
            .setIgnoreTimeScale(true);
    
    }
    private void HandleEnterCookMode()
    {
        rectTransformToMove.gameObject.SetActive(false);
    }
    private void HandleExitCookMode()
    {
        rectTransformToMove.gameObject.SetActive(true);
    }
}
