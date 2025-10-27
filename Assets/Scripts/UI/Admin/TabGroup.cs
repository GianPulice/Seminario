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

    private int currentTabIndex = -1;
    void Awake()
    {
        tabButtons = GetComponentsInChildren<TabTweenButton>().ToList();

        foreach (TabTweenButton button in tabButtons)
        {
            button.Initialize(this);
        }
    }

    void Start()
    {
        if (startingSelectedButton != null)
        {
            OnTabSelected(startingSelectedButton);
        }
        else
        {
            foreach (var button in tabButtons)
            {
                button.SetSelected(false);
            }
            foreach (var page in pagesToSwap)
            {
                if (page != null) page.SetActive(false);
            }
        }
    }
    public void OnTabSelected(TabTweenButton selectedButton)
    {
        foreach (TabTweenButton button in tabButtons)
        {
            button.SetSelected(button == selectedButton);
        }
        int selectedIndex = tabButtons.IndexOf(selectedButton);
        currentTabIndex = selectedIndex;
        if (selectedIndex < 0)
        {
            Debug.LogError("El botón seleccionado no está en la lista de tabs.", this);
            return;
        }
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
    }
    public int GetTabCount()
    {
        return tabButtons.Count;
    }

    public int GetCurrentTabIndex()
    {
        return currentTabIndex;
    }
    public void SelectTabByIndex(int index)
    {
        if (index < 0 || index >= tabButtons.Count)
        {
            return;
        }
        if (index == currentTabIndex)
        {
            return;
        }

        TabTweenButton buttonToSelect = tabButtons[index];
        OnTabSelected(buttonToSelect);
    }
}


