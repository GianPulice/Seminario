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
        SuscribeToUpdateManagerEvent();
        SuscribeToOpenTabernButtonEvent();
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
        UnsuscribeToOpenTabernButtonEvent();
        UnsuscribeToOpenTabernButtonEvent();
    }


    public void SkipCurrentDay()
    {
        PlayerView.OnEnterInResumeDay?.Invoke();
        DeviceManager.Instance.IsUIModeActive = true;
        CalculetaDifferentTypesOfCosts();

        TabernManagerUI.Instance.PlayResumeDayAnimation();

        TabernManagerUI.Instance.OrderPaymentsText.text = "Order Payments: " + orderPaymentsAmount.ToString();
        TabernManagerUI.Instance.TipsEarnedText.text = "Tips Earned: " + tipsEarnedAmount.ToString();

        TabernManagerUI.Instance.MaintenanceText.text = "Maintenance: " + maintenanceAmount.ToString();
        TabernManagerUI.Instance.TaxesText.text = "Taxes: " + taxesAmount.ToString();

        TabernManagerUI.Instance.BurntDishesText.text = "Burnt Dishes: " + burntDishesAmonut.ToString();
        TabernManagerUI.Instance.BrokenThingsText.text = "Broken Things: " + brokenThingsAmount.ToString();
        TabernManagerUI.Instance.PurchasedIngredientsText.text = "Purchased Ingredients: " + purchasedIngredientsAmount.ToString();

        float totalInncomes = orderPaymentsAmount + tipsEarnedAmount;
        float totalExpenses = maintenanceAmount + taxesAmount + burntDishesAmonut +brokenThingsAmount + purchasedIngredientsAmount;
        float finalAmount = totalInncomes - totalExpenses;
        TabernManagerUI.Instance.NetProfitText.text = "Net Profit: " + "total incomes(" + totalInncomes.ToString() + ") - total expenses(" + totalExpenses.ToString() + ") = " + finalAmount.ToString();

        canOpenTabern = true;
        currentDay++;
        TabernManagerUI.instance.TabernStatusText.text = "Tabern is closed";
        TabernManagerUI.instance.TabernCurrentTimeText.text = "08 : 00";
        TabernManagerUI.instance.CurrentDayText.text = "Day " + currentDay.ToString();
    }


    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdateTabernManager;
    }

    private void UnsuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate -= UpdateTabernManager;
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

            currentMinute = 0f;
            TabernManagerUI.instance.TabernStatusText.text = "Tabern is open";

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

    private IEnumerator PlayCurrentTabernMusic(string musicClipName)
    {
        yield return new WaitUntil(() => AudioManager.Instance != null);
        StartCoroutine(AudioManager.Instance.PlayMusic(musicClipName));
    }
}
