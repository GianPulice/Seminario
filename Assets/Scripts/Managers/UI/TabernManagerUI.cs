using System.Collections;
using TMPro;
using UnityEngine;

public class TabernManagerUI : Singleton<TabernManagerUI>
{
    [Header("Animation")]
    [SerializeField] private DailyCostAnim dailyCostAnim;

    [Header("GaneralContainers:")]
    [SerializeField] private GameObject tabernStatusContainer;
    [SerializeField] private GameObject tabernCurrentTimeContainer;
    [SerializeField] private GameObject currentDayContainer;
    [SerializeField] private GameObject blackBackground;
    [SerializeField] private CanvasGroup blackBackgroundCanvasGroup;
    [SerializeField] private GameObject containerResumeDay;
    [SerializeField] private GameObject loadingCircle;

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

    private Coroutine fadeRoutine;

    private float fadeDuration = 1f;

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
        SaveSystemManager.OnSaveAllGameData?.Invoke();
    }

    public void PlayResumeDayAnimation()
    {
        FadeBlackBackground(true);
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
        StartCoroutine(LoadingDataEffect());
        containerResumeDay.gameObject.SetActive(false);
        FadeBlackBackground(false);
        PlayerView.OnExitInResumeDay?.Invoke();
        DeviceManager.Instance.IsUIModeActive = false;

        TabernManager.Instance.OrderPaymentsAmount = 0;
        TabernManager.Instance.TipsEarnedAmount = 0;

        TabernManager.Instance.MaintenanceAmount = 0;
        TabernManager.Instance.TaxesAmount = 0;
        TabernManager.Instance.BurntDishesAmount = 0;

        TabernManager.Instance.BrokenThingsAmount = 0;
        TabernManager.Instance.PurchasedIngredientsAmount = 0;

        TabernManager.Instance.CanOpenTabern = true;
        TabernManager.Instance.CurrentDay++;
        tabernStatusText.text = "Tabern is closed";
        tabernCurrentTimeText.text = "08 : 00";
        currentDayText.text = "Day " + TabernManager.Instance.CurrentDay.ToString();
        AdministratingManagerUI.OnCloseTabern?.Invoke();
    }

    private void InitializeTabernTexts()
    {
        tabernStatusText.text = "Tabern is closed";
        tabernCurrentTimeText.text = "08 : 00";
        currentDayText.text = "Day " + TabernManager.Instance.CurrentDay.ToString();
    }

    private void FadeBlackBackground(bool fadeIn)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        blackBackground.SetActive(true);
        fadeRoutine = StartCoroutine(FadeRoutine(fadeIn));
    }

    private IEnumerator FadeRoutine(bool fadeIn)
    {
        float start = blackBackgroundCanvasGroup.alpha;
        float end = fadeIn ? 1f : 0f;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            blackBackgroundCanvasGroup.alpha = Mathf.Lerp(start, end, t / fadeDuration);
            yield return null;
        }

        blackBackgroundCanvasGroup.alpha = end;

        blackBackground.SetActive(fadeIn);
        containerResumeDay.SetActive(fadeIn);

        if (fadeIn)
        {
            dailyCostAnim.ShowPanel();
        }

        else
        {
            dailyCostAnim.HidePanel();
        }
    }

    private IEnumerator LoadingDataEffect()
    {
        loadingCircle.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f);

        loadingCircle.gameObject.SetActive(false);
    }
}
