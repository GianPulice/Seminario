using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrdersManagerUI : Singleton<OrdersManagerUI>
{
    [Header("Parameters")]
    [SerializeField] private RectTransform ordersContainer; // Donde se instancian los pedidos
    [SerializeField] private GameObject orderUIPrefab;
    [Header("UI Effect")]
    [SerializeField] private Vector3 cookingScale = new Vector3(0.8f, 0.8f, 0.8f);
    [SerializeField] private float cookingOffsetY = -50f;
    [SerializeField] private float animTime = 0.4f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutQuad;

    [SerializeField] private float orderPrefabWidth = 177f;
    private int totalOrdersBeforeTabernOpen = 0;
    private List<OrderItemUI> activeOrders = new List<OrderItemUI>();
    private Vector2 originalAnchoredPos;
    public int TotalOrdersBeforeTabernOpen { get => totalOrdersBeforeTabernOpen; }

    void Awake()
    {
        CreateSingleton(false);
        SubscribeToPlayerViewEvents();
        if (ClientManager.Instance.IsTabernOpen)
        {
            totalOrdersBeforeTabernOpen++;
        }

        originalAnchoredPos = ordersContainer.anchoredPosition;
    }

    private void OnDestroy()
    {
        UnsubscribeToPlayerViewEvents();
    }

    private void SubscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInAdministrationMode += DissapearIfInAdmin;
        PlayerView.OnExitInAdministrationMode += AppearIfOutsideAdmin;
        PlayerView.OnEnterInCookMode += ChangeScaleIfInCooking;
        PlayerView.OnExitInCookMode += ReturnScaleToNormal;
    }
    private void UnsubscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInAdministrationMode -= DissapearIfInAdmin;
        PlayerView.OnExitInAdministrationMode -= AppearIfOutsideAdmin;
        PlayerView.OnEnterInCookMode -= ChangeScaleIfInCooking;
        PlayerView.OnExitInCookMode -= ReturnScaleToNormal;
    }
    public void AddOrder(OrderDataUI newOrderDataUI)
    {
        GameObject obj = Instantiate(orderUIPrefab, ordersContainer);
        OrderItemUI uiItem = obj.GetComponent<OrderItemUI>();
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition3D = Vector3.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        LayoutElement le = obj.GetComponent<LayoutElement>();
        if (le == null) le = obj.AddComponent<LayoutElement>();

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();

        uiItem.SetupOrder(newOrderDataUI);
        activeOrders.Add(uiItem);

        le.preferredWidth = 0f;
        cg.alpha = 0f;

        LeanTween.value(obj, 0f, orderPrefabWidth, animTime)
            .setOnUpdate((float val) =>
            {
                le.preferredWidth = val;
            })
            .setEase(easeType);
        LeanTween.alphaCanvas(cg, 1f, animTime * 0.8f) 
            .setEase(easeType);
    }

    public void RemoveOrder(OrderDataUI orderDataUI)
    {
        OrderItemUI itemToRemove = activeOrders.Find(o => o.CurrentOrder == orderDataUI);

        if (itemToRemove != null)
        {
            activeOrders.Remove(itemToRemove);

            LayoutElement le = itemToRemove.GetComponent<LayoutElement>();
            CanvasGroup cg = itemToRemove.GetComponent<CanvasGroup>();

            if (le == null || cg == null)
            {
                Destroy(itemToRemove.gameObject);
                return;
            }

            LeanTween.value(itemToRemove.gameObject, le.preferredWidth, 0f, animTime)
                .setOnUpdate((float val) =>
                {
                    le.preferredWidth = val;
                })
                .setEase(easeType);

            LeanTween.alphaCanvas(cg, 0f, animTime * 0.8f)
                .setEase(easeType)
                .setOnComplete(() =>
                {
                    Destroy(itemToRemove.gameObject);
                });
        }
    }

    private void DissapearIfInAdmin()
    {
        foreach (var order in activeOrders)
        {
            if (order != null)
            {
                var img = order.GetComponentInChildren<Image>();
                if (img != null)
                    img.gameObject.SetActive(false);
            }
        }
    }
    private void AppearIfOutsideAdmin()
    {
        foreach (var order in activeOrders)
        {
            if (order != null)
            {
                var img = order.GetComponentInChildren<Image>(true);
                if (img != null)
                    img.gameObject.SetActive(true);
            }
        }
    }
    private void ChangeScaleIfInCooking()
    {
        foreach (var order in activeOrders)
        {
            if (order != null)
            {
                LeanTween.scale(order.gameObject, cookingScale, animTime)
                    .setEase(easeType)
                    .setIgnoreTimeScale(true);
            }
        }

        Vector2 target = originalAnchoredPos + new Vector2(0f, cookingOffsetY);

        LeanTween.move(ordersContainer, target, animTime)
            .setEase(easeType)
            .setIgnoreTimeScale(true);
    }
    private void ReturnScaleToNormal()
    {
        foreach (var order in activeOrders)
        {
            if (order != null)
            {
                LeanTween.scale(order.gameObject, Vector3.one, animTime)
                    .setEase(easeType)
                    .setIgnoreTimeScale(true);
            }
        }

        LeanTween.move(ordersContainer, originalAnchoredPos, animTime)
            .setEase(easeType)
            .setIgnoreTimeScale(true);
    }
}
