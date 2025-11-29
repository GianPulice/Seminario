using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    [Header("Tabs")]
    [SerializeField] private List<TabTweenButton> tabButtons;
    [Tooltip("El botón de pestaña que debe aparecer seleccionado al inicio.")]
    [SerializeField] private TabTweenButton startingSelectedButton; // Tipo TabTweenButton
    [Space(2)]
    [Header("Paneles")]
    [Tooltip("Arrastra aquí los paneles que se activarán, EN EL MISMO ORDEN que los botones")]
    [SerializeField] private List<GameObject> pagesToSwap;

    public TabTweenButton StartingSelectedButton => startingSelectedButton;
    public TabTweenButton CurrentSelectedButton { get; private set; }

    private int currentTabIndex = -1;
    void Awake()
    {
        if (tabButtons == null || tabButtons.Count == 0)
            tabButtons = GetComponentsInChildren<TabTweenButton>(true).ToList();
        foreach (TabTweenButton button in tabButtons)
        {
            button.Initialize(this);
        }
    }

    void Start()
    {
        if (startingSelectedButton != null)
        {
            OnTabSelected(startingSelectedButton, true);
        }
        else if (tabButtons.Count > 0)
        {
            OnTabSelected(tabButtons[0], true);
        }
        else
        {
            DeselectAllTabsAndHidePages();
        }
    }
    public void OnTabSelected(TabTweenButton selectedButton, bool playSound)
    {
        if (selectedButton == null)
        {
            Debug.LogWarning("Se intentó seleccionar un Tab nulo.", this);
            return;
        }

        foreach (TabTweenButton button in tabButtons)
        {
            button.SetSelected(button == selectedButton);
        }

        int selectedIndex = tabButtons.IndexOf(selectedButton);
        if (selectedIndex < 0)
        {
            Debug.LogError("El botón seleccionado no está en la lista de tabs.", this);
            return;
        }

        currentTabIndex = selectedIndex;
        CurrentSelectedButton = selectedButton;

        if (pagesToSwap.Count != tabButtons.Count)
        {
            Debug.LogError("Error: La cantidad de botones no coincide con la cantidad de paneles.", this);
            return;
        }

        for (int i = 0; i < pagesToSwap.Count; i++)
        {
            if (pagesToSwap[i] != null)
            {
                pagesToSwap[i].SetActive(i == selectedIndex);
            }
        }

        if (playSound)
        {
            AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
        }
    }

    private void DeselectAllTabsAndHidePages()
    {
        foreach (var button in tabButtons)
            button.SetSelected(false);

        foreach (var page in pagesToSwap)
            if (page != null)
                page.SetActive(false);
    }
    public int GetCurrentTabIndex()
    {
        return currentTabIndex >= 0 ? currentTabIndex : 0;
    }

    public int GetTabCount()
    {
        return tabButtons.Count;
    }

    public void RefreshAllTabsVisuals()
    {
        foreach (var tab in tabButtons)
        {
            tab.ForceRefreshSelectedState();
        }
    }
    public void ForceShowCurrentTab()
    {
        if (currentTabIndex < 0 || currentTabIndex >= pagesToSwap.Count)
            return;

        for (int i = 0; i < pagesToSwap.Count; i++)
        {
            if (pagesToSwap[i] != null)
            {
                pagesToSwap[i].SetActive(i == currentTabIndex);
            }
        }

        RefreshAllTabsVisuals();
    }
    public void SelectTabByIndex(int index, bool playSound = true)
    {
        if (index < 0 || index >= tabButtons.Count)
            return;
        if (index == currentTabIndex)
            return;


        TabTweenButton buttonToSelect = tabButtons[index];
        OnTabSelected(buttonToSelect, playSound);
    }
    public int GetButtonIndex(TabTweenButton button)
    {
        return tabButtons.IndexOf(button);
    }

}


