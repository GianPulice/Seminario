using TMPro;
using UnityEngine;

public class TabernManagerUI : Singleton<TabernManagerUI>
{
    /// <summary>
    /// Agregar que si hay clientes dentro de la taberna y termino el tiempo, que no te puedas ir a dormir todavia hasta que se vayan todos
    /// </summary>

    [Header("Animation")]
    [SerializeField] private DailyCostAnim dailyCostAnim;

    [Header("GaneralContainers:")]
    [SerializeField] private GameObject tabernStatusContainer;
    [SerializeField] private GameObject tabernCurrentTimeContainer;
    [SerializeField] private GameObject currentDayContainer;
    [SerializeField] private GameObject containerResumeDay;

    [Header("TabernStatus:")]
    [SerializeField] private TextMeshProUGUI tabernStatusText;
    [SerializeField] private TextMeshProUGUI tabernCurrentTimeText;
    [SerializeField] private TextMeshProUGUI currentDayText;

    [Header("ResumeDayTexts:")]
    [SerializeField] private TextMeshProUGUI orderPaymentsText;
    [SerializeField] private TextMeshProUGUI tipsEarnedText;
    [SerializeField] private TextMeshProUGUI maintenanceText;
    [SerializeField] private TextMeshProUGUI taxesText;
    [SerializeField] private TextMeshProUGUI burntDishesText;
    [SerializeField] private TextMeshProUGUI brokenThingsText;
    [SerializeField] private TextMeshProUGUI purchasedIngredientsText;
    [SerializeField] private TextMeshProUGUI netProfitText;

    public GameObject PanelResumeDay { get => containerResumeDay; }

    public TextMeshProUGUI TabernStatusText { get => tabernStatusText; }
    public TextMeshProUGUI TabernCurrentTimeText { get => tabernCurrentTimeText; }
    public TextMeshProUGUI CurrentDayText { get => currentDayText; }

    public TextMeshProUGUI OrderPaymentsText { get => orderPaymentsText; }
    public TextMeshProUGUI TipsEarnedText { get => tipsEarnedText; }
    public TextMeshProUGUI MaintenanceText { get => maintenanceText; }
    public TextMeshProUGUI TaxesText { get => taxesText; }
    public TextMeshProUGUI BurntDishesText { get => burntDishesText; }
    public TextMeshProUGUI BrokenThingsText { get => brokenThingsText; }
    public TextMeshProUGUI PurchasedIngredientsText { get => purchasedIngredientsText; }
    public TextMeshProUGUI NetProfitText { get => netProfitText; }


    void Awake()
    {
        CreateSingleton(false);
        SuscribeToPlayerViewEvents();
        InitializeTabernTexts();
    }

    void OnDestroy()
    {
        UnsuscribeToPlayerViewEvents();
    }


    // Metodo agregado a boton en la UI
    public void ButtonStartNextDay()
    {
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        CloseResumeDayPanel();
    }

    public void PlayResumeDayAnimation()
    {
        containerResumeDay.SetActive(true);

        if (dailyCostAnim != null)
            dailyCostAnim.ShowPanel();
    }


    private void SuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInCookMode += OnDisableUI;
        PlayerView.OnEnterInAdministrationMode += OnDisableUI;
        PlayerView.OnEnterTutorial += OnDisableUI;
        PlayerView.OnEnterInResumeDay += OnDisableUI;

        PlayerView.OnExitInCookMode += OnEnabledUI;
        PlayerView.OnExitInAdministrationMode += OnEnabledUI;
        PlayerView.OnExitTutorial += OnEnabledUI;
        PlayerView.OnExitInResumeDay += OnEnabledUI;
    }

    private void UnsuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInCookMode -= OnDisableUI;
        PlayerView.OnEnterInAdministrationMode -= OnDisableUI;
        PlayerView.OnEnterTutorial -= OnDisableUI;
        PlayerView.OnEnterInResumeDay -= OnDisableUI;

        PlayerView.OnExitInCookMode -= OnEnabledUI;
        PlayerView.OnExitInAdministrationMode -= OnEnabledUI;
        PlayerView.OnExitTutorial -= OnEnabledUI;
        PlayerView.OnExitInResumeDay -= OnEnabledUI;
    }

    private void OnDisableUI()
    {
        tabernStatusContainer.SetActive(false);
        tabernCurrentTimeContainer.SetActive(false);
        currentDayContainer.SetActive(false);
    }

    private void OnEnabledUI()
    {
        tabernStatusContainer.SetActive(true);
        tabernCurrentTimeContainer.SetActive(true);
        currentDayContainer.SetActive(true);
    }

    private void CloseResumeDayPanel()
    {
        containerResumeDay.gameObject.SetActive(false);
        PlayerView.OnExitInResumeDay?.Invoke();
        DeviceManager.Instance.IsUIModeActive = false;

        TabernManager.Instance.OrderPaymentsAmount = 0;
        TabernManager.Instance.TipsEarnedAmount = 0;

        TabernManager.Instance.MaintenanceAmount = 0;
        TabernManager.Instance.TaxesAmount = 0;
        TabernManager.Instance.BurntDishesAmount = 0;

        TabernManager.Instance.BrokenThingsAmount = 0;
        TabernManager.Instance.PurchasedIngredientsAmount = 0;
    }

    private void InitializeTabernTexts()
    {
        tabernStatusText.text = "Tabern is closed";
        tabernCurrentTimeText.text = "08 : 00";
        currentDayText.text = "Day " + TabernManager.Instance.CurrentDay.ToString();
    }
}
