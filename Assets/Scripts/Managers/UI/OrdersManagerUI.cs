using System.Collections.Generic;
using UnityEngine;

public class OrdersManagerUI : Singleton<OrdersManagerUI>
{
    [SerializeField] private RectTransform ordersContainer; // Donde se instancian los pedidos
    [SerializeField] private GameObject orderUIPrefab;

    private List<OrderItemUI> activeOrders = new List<OrderItemUI>();

    private int totalOrdersBeforeTabernOpen = 0;

    public int TotalOrdersBeforeTabernOpen { get => totalOrdersBeforeTabernOpen; }


    void Awake()
    {
        CreateSingleton(false);
    }


    public void AddOrder(OrderDataUI newOrderDataUI)
    {
        GameObject obj = Instantiate(orderUIPrefab, ordersContainer);
        OrderItemUI uiItem = obj.GetComponent<OrderItemUI>();
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition3D = Vector3.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        uiItem.SetupOrder(newOrderDataUI);
        activeOrders.Add(uiItem);

        if (ClientManager.Instance.IsTabernOpen)
        {
            totalOrdersBeforeTabernOpen++;
        }
    }

    public void RemoveOrder(OrderDataUI orderDataUI)
    {
        OrderItemUI itemToRemove = activeOrders.Find(o => o.CurrentOrder == orderDataUI);

        if (itemToRemove != null)
        {
            activeOrders.Remove(itemToRemove);
            Destroy(itemToRemove.gameObject);
        }
    }

    public void RemoveTotalOrdersWhenCloseTabern()
    {
        totalOrdersBeforeTabernOpen = 0;
    }
}
