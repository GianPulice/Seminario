using UnityEngine;
using System;
using UnityEngine.UI;

public class SliderCleanDirtyTableUI : MonoBehaviour
{
    [SerializeField] private SliderCleanDiirtyTableUIData sliderCleanDiirtyTableUIData;

    [SerializeField] private Slider sliderBar;
    [SerializeField] private Slider sliderRadial;

    private Slider currentSlider;

    private Table currentTable;

    private Action onActiveSlider, onDeactivateSlider;


    void Awake()
    {
        SuscribeToUpdateManagerEvent();
        SuscribeToLamdaEvents();
        SuscribeToPlayerViewEvents();
        SuscribeToPlayerControllerEvents();
        ChooseCurrentSliderType();
    }

    // Simulacion de Update
    void UpdateSliderCleanDirtyTableUI()
    {
        DecreaseAllSliderValuesExceptCurrentTable();
    }

    void OnDestroy()
    {
        UnsuscribeToUpdateManagerEvent();
        UnsuscribeToLamdaEvents();
        UnuscribeToPlayerViewEvents();
        UnsuscribeToPlayerControllerEvents();
    }


    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdateSliderCleanDirtyTableUI;
    }

    private void UnsuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate -= UpdateSliderCleanDirtyTableUI;
    }

    private void SuscribeToLamdaEvents()
    {
        onActiveSlider += () => ActivateOrDeactivateSlider(true);
        onDeactivateSlider += () => ActivateOrDeactivateSlider(false);
    }

    private void UnsuscribeToLamdaEvents()
    {
        onActiveSlider -= () => ActivateOrDeactivateSlider(true);
        onDeactivateSlider -= () => ActivateOrDeactivateSlider(false);
    }

    private void SuscribeToPlayerViewEvents()
    {
        PlayerView.OnActivateSliderCleanDirtyTable += onActiveSlider;
        PlayerView.OnDeactivateSliderCleanDirtyTable += onDeactivateSlider;
    }

    private void UnuscribeToPlayerViewEvents()
    {
        PlayerView.OnActivateSliderCleanDirtyTable -= onActiveSlider;
        PlayerView.OnDeactivateSliderCleanDirtyTable -= onDeactivateSlider;
    }

    private void SuscribeToPlayerControllerEvents()
    {
        PlayerController.OnCleanDirtyTableIncreaseSlider += IncreaseFromCurrentTable;
        PlayerController.OnCleanDirtyTableDecreaseSlider += DecreaseSliderFromCurrentTable;
    }

    private void UnsuscribeToPlayerControllerEvents()
    {
        PlayerController.OnCleanDirtyTableIncreaseSlider -= IncreaseFromCurrentTable;
        PlayerController.OnCleanDirtyTableDecreaseSlider -= DecreaseSliderFromCurrentTable;
    }

    private void ActivateOrDeactivateSlider(bool current)
    {
        if (!current)
        {
            currentTable = null;
        }

        currentSlider.gameObject.SetActive(current);
    }

    private void IncreaseFromCurrentTable(Table table)
    {
        if (currentTable == null)
        {
            currentTable = table;
        }

        currentTable.CurrentCleanProgress += Time.deltaTime;
        currentTable.CurrentCleanProgress = Mathf.Min(currentTable.CurrentCleanProgress, sliderCleanDiirtyTableUIData.MaxHoldTime);

        UpdateSliderValueFromCurrentTable(currentTable);

        if (currentTable.CurrentCleanProgress >= sliderCleanDiirtyTableUIData.MaxHoldTime)
        {
            currentTable.CurrentCleanProgress = 0;
            currentSlider.value = currentSlider.minValue;
            onDeactivateSlider?.Invoke();
            table.SetDirty(false);
        }
    }

    private void DecreaseSliderFromCurrentTable(Table table)
    {
        if (currentTable != table)
        {
            currentTable = table;
        }

        if (currentTable != null)
        {
            currentTable.CurrentCleanProgress -= Time.deltaTime;
            currentTable.CurrentCleanProgress = Mathf.Max(currentTable.CurrentCleanProgress, 0f);

            UpdateSliderValueFromCurrentTable(currentTable);
        }
    }

    private void UpdateSliderValueFromCurrentTable(Table table)
    {
        currentSlider.value = table.CurrentCleanProgress / sliderCleanDiirtyTableUIData.MaxHoldTime;
    }

    private void DecreaseAllSliderValuesExceptCurrentTable()
    {
        for (int i = 0; i < TablesManager.Instance.Tables.Count; i++)
        {
            Table table = TablesManager.Instance.Tables[i];

            if (currentSlider.gameObject.activeSelf && currentTable != null && table == currentTable)
            {
                continue;
            }

            table.CurrentCleanProgress -= Time.deltaTime;
            table.CurrentCleanProgress = Mathf.Max(table.CurrentCleanProgress, 0f);
        }
    }

    private void ChooseCurrentSliderType()
    {
        if (sliderCleanDiirtyTableUIData.SliderType == SliderCleanDirtyTableType.SliderBar)
        {
            currentSlider = sliderBar;
        }

        else
        {
            currentSlider = sliderRadial;
        }
    }
}
