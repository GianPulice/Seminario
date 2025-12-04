using TMPro;
using UnityEngine;

public class TabernManagerUI : Singleton<TabernManagerUI>
{
    [SerializeField] private GameObject tabernStatusContainer;
    [SerializeField] private GameObject tabernCurrentTimeContainer;
    [SerializeField] private GameObject currentDayContainer;
    [SerializeField] private GameObject panelResumeDay;

    [SerializeField] private TextMeshProUGUI tabernStatusText;
    [SerializeField] private TextMeshProUGUI tabernCurrentTimeText;
    [SerializeField] private TextMeshProUGUI currentDayText;

    private int currentDay = 1;

    private float currentMinute = 0f;
    private float timeSpeed = 20f;

    private const int OPEN_HOUR = 8;
    private const float DAY_DURATION_MINUTES = 16f * 60f;

    public TextMeshProUGUI TabernStatusText { get => tabernStatusText; }
    public TextMeshProUGUI TabernCurrentTimeText { get => tabernCurrentTimeText; }
    public TextMeshProUGUI CurrentDayText { get => currentDayText; }

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
    }

    void OnDestroy()
    {
        UnsuscribeToUpdateManagerEvent();
        UnsuscribeToPlayerViewEvents();
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

        PlayerView.OnExitInCookMode += OnEnabledUI;
        PlayerView.OnExitInAdministrationMode += OnEnabledUI;
        PlayerView.OnExitTutorial += OnEnabledUI;
    }

    private void UnsuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInCookMode -= OnDisableUI;
        PlayerView.OnEnterInAdministrationMode -= OnDisableUI;
        PlayerView.OnEnterTutorial -= OnDisableUI;

        PlayerView.OnExitInCookMode -= OnEnabledUI;
        PlayerView.OnExitInAdministrationMode -= OnEnabledUI;
        PlayerView.OnExitTutorial -= OnEnabledUI;
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

    private void InitializeTabernTexts()
    {
        tabernStatusText.text = "Tabern is closed";
        tabernCurrentTimeText.text = "08 : 00";
        currentDayText.text = "Day " + currentDay.ToString();
    }
}
