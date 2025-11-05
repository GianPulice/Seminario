using System.Collections.Generic;
using UnityEngine;

public class OrdersManagerUI : Singleton<OrdersManagerUI>
{
    [SerializeField] private RectTransform ordersContainer; // Donde se instancian los pedidos
    [SerializeField] private GameObject orderUIPrefab;

    [SerializeField] private List<OrderItemUI> activeOrders = new List<OrderItemUI>();


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
}
