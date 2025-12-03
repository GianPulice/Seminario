using System.Collections;
using TMPro;
using UnityEngine;

public class TabernManager : Singleton<TabernManager>
{
    [SerializeField] private TextMeshPro tabernStatusText;
    [SerializeField] private TextMeshPro tabernCurrentTimeText;
    [SerializeField] private TextMeshPro currentDayText;

    private int currentDay = 1;

    private float currentMinute = 0f; 
    private float timeSpeed = 20f;

    private bool isTabernOpen = false;
    private bool canOpenTabern = true;

    private const int OPEN_HOUR = 8;
    private const float DAY_DURATION_MINUTES = 16f * 60f; 

    public TextMeshPro TabernCurrentTimeText { get => tabernCurrentTimeText; }

    public bool IsTabernOpen { get => isTabernOpen; }


    void Awake()
    {
        CreateSingleton(false);
        SuscribeToUpdateManagerEvent();
        SuscribeToOpenTabernButtonEvent();
    }

    void Start()
    {
        InitializeTabernTexts();
        StartCoroutine(PlayCurrentTabernMusic("TabernClose"));
    }

    // Simulacion de Update
    void UpdateTabernManager()
    {
        UpdateTimer();
    }

    void OnDestroy()
    {
        UnsuscribeToUpdateManagerEvent();
        UnsuscribeToOpenTabernButtonEvent();
    }


    public void SkipCurrentDay()
    {
        // Aca se podria agregar algun evento que heche a la mierda a los clientes que estan en la taberna y que elimine las ordenes en la UI
        canOpenTabern = true;
        currentDay++;
        tabernStatusText.text = "Tabern is closed";
        tabernCurrentTimeText.text = "08 : 00";
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

    private void UpdateTimer()
    {
        if (!isTabernOpen) return;
        if (currentMinute >= DAY_DURATION_MINUTES)
        {
            SetIsTabernClosed();
            return;
        }

        currentMinute += Time.deltaTime * timeSpeed;

        float totalMinutes = currentMinute;

        int hours = OPEN_HOUR + Mathf.FloorToInt(totalMinutes / 60f);
        int minutes = Mathf.FloorToInt(totalMinutes % 60f);

        if (hours >= 24) hours = 0;

        tabernCurrentTimeText.text = $"{hours:00} : {minutes:00}";
    }

    private void SetIsTabernOpen()
    {
        if (canOpenTabern)
        {
            StartCoroutine(PlayCurrentTabernMusic("TabernOpen"));
            canOpenTabern = false;
            isTabernOpen = true;
            currentMinute = 0f;
            tabernStatusText.text = "Tabern is open";
            UpdateTimer();
            ClientManager.Instance.SetRandomSpawnTime();
        }
    }

    private void SetIsTabernClosed()
    {
        isTabernOpen = false;
        currentMinute = DAY_DURATION_MINUTES;
        tabernCurrentTimeText.text = "24 : 00";
        tabernStatusText.text = "Tabern is closed";
        StartCoroutine(PlayCurrentTabernMusic("TabernClose"));
    }

    private void InitializeTabernTexts()
    {
        tabernStatusText.text = "Tabern is closed";
        tabernCurrentTimeText.text = "08 : 00";
    }

    private IEnumerator PlayCurrentTabernMusic(string musicClipName)
    {
        yield return new WaitUntil(() => AudioManager.Instance != null);

        StartCoroutine(AudioManager.Instance.PlayMusic(musicClipName));
    }
}
