using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OrderItemUI : MonoBehaviour
{
    [SerializeField] private RectTransform containerRect;
    [SerializeField] private Image clientImage;
    [SerializeField] private Image orderImage;
    [SerializeField] private TMP_Text timeText;

    private OrderDataUI currentOrderDataUI;
    private Coroutine timerCoroutine;
    private Coroutine alertCoroutine;

    private Vector3 originalPos;

    private float initialTime;

    public OrderDataUI CurrentOrder { get => currentOrderDataUI; }


    public void SetupOrder(OrderDataUI order)
    {
        currentOrderDataUI = order;
        orderImage.sprite = order.OrderSprite;
        clientImage.sprite = order.ClientSprite;
        originalPos = containerRect.anchoredPosition;
        initialTime = order.RemainingTime;

        UpdateTimerUI();
        timerCoroutine = StartCoroutine(TimerCoroutine());
    }


    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentOrderDataUI.RemainingTime / 60f);
        int seconds = Mathf.FloorToInt(currentOrderDataUI.RemainingTime % 60f);

        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private IEnumerator TimerCoroutine()
    {
        yield return new WaitForSeconds(1);

        while (currentOrderDataUI.RemainingTime > 0)
        {
            currentOrderDataUI.RemainingTime -= Time.deltaTime;
            UpdateTimerUI();

            if (currentOrderDataUI.RemainingTime <= initialTime * 0.3f && alertCoroutine == null)
            {
                alertCoroutine = StartCoroutine(AlertCoroutine());
            }

            yield return null; // Esperar un frame
        }

        OrdersManagerUI.Instance.RemoveOrder(currentOrderDataUI);
    }

    private IEnumerator AlertCoroutine()
    {
        float speed = 6f;
        float amplitude = 6f; 

        while (currentOrderDataUI.RemainingTime > 0)
        {
            float offset = Mathf.Sin(Time.time * speed) * amplitude;
            containerRect.anchoredPosition = originalPos + new Vector3(0, offset, 0);
            yield return null;
        }
    }
}

[System.Serializable]
public class OrderDataUI
{
    private Sprite orderSprite;
    private Sprite clientSprite;

    private float remainingTime;

    public Sprite OrderSprite { get => orderSprite; }
    public Sprite ClientSprite { get => clientSprite; }

    public float RemainingTime { get => remainingTime; set => remainingTime = value; }


    public OrderDataUI(Sprite orderSprite, Sprite clientSprite, float maxTime)
    {
        this.orderSprite = orderSprite;
        this.clientSprite = clientSprite;
        remainingTime = maxTime;
    }
}