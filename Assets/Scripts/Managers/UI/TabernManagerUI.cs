using TMPro;
using UnityEngine;

public class TabernManagerUI : Singleton<TabernManagerUI>
{
    /// <summary>
    /// Agregar que si hay clientes dentro de la taberna y termino el tiempo, que no te puedas ir a dormir todavia hasta que se vayan todos
    /// </summary>

    /// <summary>
    /// Agregar en los layers del canvas, que el canvas de la plata se pueda ver mientras estoy en el Resumen paraque cuando descuente la plata de los gastos se vea la animacion
    /// </summary>

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
    [SerializeField] private TextMeshProUGUI purchasedUpgradesText;
    [SerializeField] private TextMeshProUGUI purchasedIngredientsText;
    [SerializeField] private TextMeshProUGUI netProfitText;

    private int currentDay = 1;

    private float currentMinute = 0f;
    private float timeSpeed = 20f;

    private const int OPEN_HOUR = 8;
    private const float DAY_DURATION_MINUTES = 16f * 60f;

    public GameObject PanelResumeDay { get => containerResumeDay; }

    public TextMeshProUGUI TabernStatusText { get => tabernStatusText; }
    public TextMeshProUGUI TabernCurrentTimeText { get => tabernCurrentTimeText; }
    public TextMeshProUGUI CurrentDayText { get => currentDayText; }

    public TextMeshProUGUI OrderPaymentsText { get => orderPaymentsText; }
    public TextMeshProUGUI TipsEarnedText { get => tipsEarnedText; }
    public TextMeshProUGUI MaintenanceText { get => maintenanceText; }
    public TextMeshProUGUI TaxesText { get => taxesText; }
    public TextMeshProUGUI PurchasedUpgradesText { get => purchasedUpgradesText; }
    public TextMeshProUGUI PurchasedIngredientsText { get => purchasedIngredientsText; }
    public TextMeshProUGUI NetProfitText { get => netProfitText; }

    public int CurrentDay { get => currentDay; set => currentDay = value; }

    public float CurrentMinute { get => currentMinute; set => currentMinute = value; }
    public float DAY_DURATION_MINUTES_ { get => DAY_DURATION_MINUTES; }


    void Awake()
    {
        CreateSingleton(false);
        SuscribeToUpdateManagerEvent();
        SuscribeToPlayerViewEvents();
        InitializeTabernTexts();
    }

    // Simulacion de Update
    void UpdateTabernManagerUI()
    {
        UpdateTimer();
        ChekcInputs();
    }

    void OnDestroy()
    {
        UnsuscribeToUpdateManagerEvent();
        UnsuscribeToPlayerViewEvents();
    }


    // Metodo agregado a boton en la UI
    public void ButtonClose()
    {
        AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        CloseResumeDayPanel();
    }


    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdateTabernManagerUI;
    }

    private void UnsuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate -= UpdateTabernManagerUI;
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

    private void UpdateTimer()
    {
        if (!TabernManager.Instance.IsTabernOpen) return;
        if (currentMinute >= DAY_DURATION_MINUTES)
        {
            TabernManager.Instance.SetIsTabernClosed();
            return;
        }

        currentMinute += Time.deltaTime * timeSpeed;

        float totalMinutes = currentMinute;

        int hours = OPEN_HOUR + Mathf.FloorToInt(totalMinutes / 60f);
        int minutes = Mathf.FloorToInt(totalMinutes % 60f);

        if (hours >= 24) hours = 0;

        tabernCurrentTimeText.text = $"{hours:00} : {minutes:00}";
    }

    private void ChekcInputs()
    {
        if (!containerResumeDay.activeSelf) return;

        if (PlayerInputs.Instance.BackPanelsUI() && !PauseManager.Instance.IsGamePaused)
        {
            CloseResumeDayPanel();
        }
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
    }

    private void InitializeTabernTexts()
    {
        tabernStatusText.text = "Tabern is closed";
        tabernCurrentTimeText.text = "08 : 00";
        currentDayText.text = "Day " + currentDay.ToString();
    }
}
