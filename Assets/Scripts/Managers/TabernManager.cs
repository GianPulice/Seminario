using System.Collections;
using UnityEngine;

public class TabernManager : Singleton<TabernManager>
{
    private bool isTabernOpen = false;
    private bool canOpenTabern = true;

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
        // Aca se podria agregar algun evento que heche a la mierda a los clientes que estan en la taberna y que elimine las ordenes en la UI
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
