using System;
using System.Collections;
using UnityEngine;

public class TabernManager : Singleton<TabernManager>
{
    [SerializeField] private TabernManagerData tabernManagerData;

    private int currentDay = 1;

    private float currentMinute = 0f;

    private float orderPaymentsAmount = 0f;
    private float tipsEarnedAmount = 0f;
    private float maintenanceAmount = 0f;
    private float taxesAmount = 0f;
    private float burntDishesAmonut = 0f;
    private float brokenThingsAmount = 0f;
    private float purchasedIngredientsAmount = 0f;

    private bool isTabernOpen = false;
    private bool canOpenTabern = true;

    private const int OPEN_HOUR = 8;
    private const float DAY_DURATION_MINUTES = 16f * 60f;

    public TabernManagerData TabernManagerData { get => tabernManagerData; }

    public int CurrentDay { get => currentDay; set => currentDay = value; }

    public float OrderPaymentsAmount { get => orderPaymentsAmount; set => orderPaymentsAmount = value; }
    public float TipsEarnedAmount { get => tipsEarnedAmount; set => tipsEarnedAmount = value; }
    public float MaintenanceAmount { get => maintenanceAmount; set => maintenanceAmount = value; }
    public float TaxesAmount { get => taxesAmount; set => taxesAmount = value; }
    public float BurntDishesAmount { get => burntDishesAmonut; set => burntDishesAmonut = value; }
    public float BrokenThingsAmount { get => brokenThingsAmount; set => brokenThingsAmount = value; }
    public float PurchasedIngredientsAmount { get => purchasedIngredientsAmount; set => purchasedIngredientsAmount = value; }

    public bool IsTabernOpen { get => isTabernOpen; }


    void Awake()
    {
        CreateSingleton(false);

        if (SaveSystemManager.SaveExists())
        {
            SaveSystemManager.OnSavOrLoadAllGame?.Invoke();
        }

        SubscribeToUpdateManagerEvent();
        SubscribeToOpenTabernButtonEvent();
        VictoryScreen.OnVictory += ApplyDailyCosts;
    }

    // Simulacion de Update
    void UpdateTabernManager()
    {
        UpdateTimer();
    }

    void Start()
    {
        StartCoroutine(PlayCurrentTabernMusic("TabernClose"));
    }

    void OnDestroy()
    {
        UnsubscribeToUpdateManagerEvent();
        UnsubscribeToOpenTabernButtonEvent();
        VictoryScreen.OnVictory -= ApplyDailyCosts;
    }


    public void SkipCurrentDay()
    {
        PlayerView.OnEnterInResumeDay?.Invoke();
        DeviceManager.Instance.IsUIModeActive = true;
        CalculetaDifferentTypesOfCosts();

        TabernManagerUI.Instance.PlayResumeDayAnimation();

        TabernManagerUI.Instance.OrderPaymentsText.text =
            "Order Payments: <color=#00FF00>$" + orderPaymentsAmount.ToString() + "</color>";

        TabernManagerUI.Instance.TipsEarnedText.text =
            "Tips Earned: <color=#00FF00>$" + tipsEarnedAmount.ToString() + "</color>";

        TabernManagerUI.Instance.MaintenanceText.text =
            "Maintenance: <color=#FF0000>$" + maintenanceAmount.ToString() + "</color>";

        TabernManagerUI.Instance.TaxesText.text =
            "Taxes: <color=#FF0000>$" + taxesAmount.ToString() + "</color>";

        TabernManagerUI.Instance.BurntDishesText.text =
            "Burnt Dishes: <color=#FF0000>$" + burntDishesAmonut.ToString() + "</color>";

        TabernManagerUI.Instance.BrokenThingsText.text =
            "Broken Things: <color=#FF0000>$" + brokenThingsAmount.ToString() + "</color>";

        TabernManagerUI.Instance.PurchasedIngredientsText.text =
            "Purchased Ingredients: <color=#FF0000>$" + purchasedIngredientsAmount.ToString() + "</color>";

        float totalInncomes = orderPaymentsAmount + tipsEarnedAmount;
        float totalExpenses = maintenanceAmount + taxesAmount + burntDishesAmonut + brokenThingsAmount + purchasedIngredientsAmount;
        float finalAmount = totalInncomes - totalExpenses;

        string netColor = finalAmount >= 0 ? "#00FF00" : "#FF0000";

        TabernManagerUI.Instance.NetProfitText.text =
            "Net Profit: total incomes(<color=#00FF00>$" + totalInncomes.ToString() +
            "</color>) - total expenses(<color=#FF0000>$" + totalExpenses.ToString() +
            "</color>) = <color=" + netColor + ">$" + finalAmount.ToString() + "</color>";

        canOpenTabern = true;
        currentDay++;
        TabernManagerUI.instance.TabernStatusText.text = "Tabern is closed";
        TabernManagerUI.instance.TabernCurrentTimeText.text = "08 : 00";
        TabernManagerUI.instance.CurrentDayText.text = "Day " + currentDay.ToString();
        AdministratingManagerUI.OnCloseTabern?.Invoke();
    }


    private void SubscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdateTabernManager;
    }

    private void UnsubscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate -= UpdateTabernManager;
    }
    private void SubscribeToOpenTabernButtonEvent()
    {
        AdministratingManagerUI.OnStartTabern += SetIsTabernOpen;       
    }

    private void UnsubscribeToOpenTabernButtonEvent()
    {
        AdministratingManagerUI.OnStartTabern -= SetIsTabernOpen;
    }

    private void SetIsTabernOpen()
    {
        if (canOpenTabern)
        {
            StartCoroutine(PlayCurrentTabernMusic("TabernOpen"));
            canOpenTabern = false;
            isTabernOpen = true;

            currentMinute = 0f;
            TabernManagerUI.instance.TabernStatusText.text = "Tabern is open";

            ClientManager.Instance.SpawnTime = 0f;
            ClientManager.Instance.SetRandomSpawnTime();
        }
    }

    public void SetIsTabernClosed()
    {
        isTabernOpen = false;
        currentMinute = DAY_DURATION_MINUTES;
        TabernManagerUI.instance.TabernCurrentTimeText.text = "24 : 00";
        TabernManagerUI.instance.TabernStatusText.text = "Tabern is closed";

        StartCoroutine(PlayCurrentTabernMusic("TabernClose"));
        AdministratingManagerUI.OnSetSelectedCurrentGameObject?.Invoke(null);
        AdministratingManagerUI.OnStartTabern?.Invoke();

        bool canTrigger = TutorialListener.Instance != null && TutorialListener.instance.TryTriggerManualTutorial(TutorialType.Bed);
        if (canTrigger)
            TutorialScreensManager.instance.SetTutorialType(TutorialType.Bed);
    }

    private void UpdateTimer()
    {
        if (!isTabernOpen) return;
        if (currentMinute >= DAY_DURATION_MINUTES)
        {
            SetIsTabernClosed();
            return;
        }

        currentMinute += Time.deltaTime * tabernManagerData.TimeSpeed;

        float totalMinutes = currentMinute;

        int hours = OPEN_HOUR + Mathf.FloorToInt(totalMinutes / 60f);
        int minutes = Mathf.FloorToInt(totalMinutes % 60f);

        if (hours >= 24) hours = 0;

        TabernManagerUI.Instance.TabernCurrentTimeText.text = $"{hours:00} : {minutes:00}";
    }

    private void CalculetaDifferentTypesOfCosts()
    {
        maintenanceAmount = tabernManagerData.MaintenanceCostPerDay + TablesManager.Instance.Tables.Count * tabernManagerData.MaintenanceCostPerTable;
        taxesAmount = (orderPaymentsAmount + tipsEarnedAmount) * (tabernManagerData.TaxesPorcentajeFromIncomes / 100f);
    }
    private void ApplyDailyCosts()
    {
        float fixedExpensesAmount = maintenanceAmount + taxesAmount;
        MoneyManager.Instance.SubMoney(fixedExpensesAmount);
    }
    private IEnumerator PlayCurrentTabernMusic(string musicClipName)
    {
        yield return new WaitUntil(() => AudioManager.Instance != null);
        StartCoroutine(AudioManager.Instance.PlayMusic(musicClipName));
    }
}
