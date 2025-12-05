using System.Collections;
using UnityEngine;

public class TabernManager : Singleton<TabernManager>
{
    private float orderPaymentsAmount = 0f;
    private float tipsEarnedAmount = 0f;
    private float maintenanceAmount = 0f;
    private float taxesAmount = 0f;
    private float purchasedUpgrades = 0f;
    private float purchasedIngredients = 0f;

    private bool isTabernOpen = false;
    private bool canOpenTabern = true;

    public float OrderPaymentsAmount { get => orderPaymentsAmount; set => orderPaymentsAmount = value; }
    public float TipsEarnedAmount { get => tipsEarnedAmount; set => tipsEarnedAmount = value; }
    public float MaintenanceAmount { get => maintenanceAmount; set => maintenanceAmount = value; }
    public float TaxesAmount { get => taxesAmount; set => taxesAmount = value; }

    public bool IsTabernOpen { get => isTabernOpen; }


    void Awake()
    {
        CreateSingleton(false);
        SuscribeToOpenTabernButtonEvent();
    }

    void Start()
    {
        StartCoroutine(PlayCurrentTabernMusic("TabernClose"));
    }

    void OnDestroy()
    {
        UnsuscribeToOpenTabernButtonEvent();
    }


    public void SkipCurrentDay()
    {
        PlayerView.OnEnterInResumeDay?.Invoke();
        DeviceManager.Instance.IsUIModeActive = true;

        TabernManagerUI.Instance.PlayResumeDayAnimation();

        TabernManagerUI.Instance.OrderPaymentsText.text = "Order Payments: " + orderPaymentsAmount.ToString();
        TabernManagerUI.Instance.TipsEarnedText.text = "Tips Earned: " + tipsEarnedAmount.ToString();

        float incomes = orderPaymentsAmount + tipsEarnedAmount;
        float expenses = maintenanceAmount + taxesAmount + purchasedUpgrades + purchasedIngredients;
        float finalAmount = incomes - expenses;

        TabernManagerUI.Instance.NetProfitText.text = "Net Profit: " + "total incomes(" + incomes.ToString() + ") - total expenses(" + expenses.ToString() + ") = " + finalAmount.ToString();

        canOpenTabern = true;
        TabernManagerUI.instance.CurrentDay++;
        TabernManagerUI.instance.TabernStatusText.text = "Tabern is closed";
        TabernManagerUI.instance.TabernCurrentTimeText.text = "08 : 00";
        TabernManagerUI.instance.CurrentDayText.text = "Day " + TabernManagerUI.instance.CurrentDay.ToString();
    }

    private void SuscribeToOpenTabernButtonEvent()
    {
        AdministratingManagerUI.OnStartTabern += SetIsTabernOpen;
        //AdministratingManagerUI.OnCloseTabern += SetIsTabernClosed;
    }

    private void UnsuscribeToOpenTabernButtonEvent()
    {
        AdministratingManagerUI.OnStartTabern -= SetIsTabernOpen;
        //AdministratingManagerUI.OnCloseTabern -= SetIsTabernClosed;
    }

    private void SetIsTabernOpen()
    {
        if (canOpenTabern)
        {
            StartCoroutine(PlayCurrentTabernMusic("TabernOpen"));
            canOpenTabern = false;
            isTabernOpen = true;

            TabernManagerUI.instance.CurrentMinute = 0f;
            TabernManagerUI.instance.TabernStatusText.text = "Tabern is open";

            ClientManager.Instance.SetRandomSpawnTime();
        }
    }

    public void SetIsTabernClosed()
    {
        isTabernOpen = false;
        TabernManagerUI.instance.CurrentMinute = TabernManagerUI.instance.DAY_DURATION_MINUTES_;
        TabernManagerUI.instance.TabernCurrentTimeText.text = "24 : 00";
        TabernManagerUI.instance.TabernStatusText.text = "Tabern is closed";

        StartCoroutine(PlayCurrentTabernMusic("TabernClose"));
    }

    private IEnumerator PlayCurrentTabernMusic(string musicClipName)
    {
        yield return new WaitUntil(() => AudioManager.Instance != null);
        StartCoroutine(AudioManager.Instance.PlayMusic(musicClipName));
    }
}
