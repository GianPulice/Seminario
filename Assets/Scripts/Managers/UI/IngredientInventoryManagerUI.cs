using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientInventoryManagerUI : MonoBehaviour//, IBookableUI
{
    [SerializeField] private RawImage inventoryPanel;

    private Transform slotParentObject;
    private List<Transform> slotPositions = new List<Transform>();
    private Dictionary<IngredientType, (GameObject slot, TextMeshProUGUI text)> ingredientSlots = new();

    [SerializeField] private int indexPanel;

    public int IndexPanel { get => indexPanel; }


    void Awake()
    {
        SuscribeToUpdateManagerEvent();
        GetComponents();
        InitializeSlots();
    }

    // Simulacion de Update
    void UpdateIngredientInventoryManagerUI()
    {
        CheckInputs();
    }

    void OnDestroy()
    {
        UnsuscribeToUpdateManagerEvent();
    }


    /*public void OpenPanel()
    {
        inventoryPanel.enabled = true;

        foreach (var kvp in ingredientSlots)
        {
            kvp.Value.slot.SetActive(true);
            int stock = IngredientInventoryManager.Instance.GetStock(kvp.Key);
            kvp.Value.text.text = stock.ToString();
        }
    }

    public void ClosePanel()
    {
        inventoryPanel.enabled = false;

        foreach (var kvp in ingredientSlots)
        {
            kvp.Value.slot.SetActive(false);
        }
    }*/


    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdateIngredientInventoryManagerUI;
    }

    private void UnsuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate -= UpdateIngredientInventoryManagerUI;
    }

    private void GetComponents()
    {
        slotParentObject = GameObject.Find("SlotObjects").transform;

        Transform slotParentPositions = GameObject.Find("SlotTransforms").transform;
        foreach (Transform slotChild in slotParentPositions)
        {
            slotPositions.Add(slotChild);
        }
    }

    private void InitializeSlots()
    {
        foreach (var slotPrefab in IngredientInventoryManager.Instance.IngredientsData)
        {
            IngredientType type = slotPrefab.IngredientType;
            if (!IngredientInventoryManager.Instance.GetAllIngredients().Contains(type))
                continue;

            int index = slotPositions.Count > ingredientSlots.Count ? ingredientSlots.Count : -1;
            if (index == -1) break;

            GameObject slotInstance = Instantiate(slotPrefab.PrefabSlotInventoryUI, slotPositions[index].position, Quaternion.identity, slotParentObject);
            slotInstance.SetActive(false);

            TextMeshProUGUI stockText = slotInstance.GetComponentInChildren<TextMeshProUGUI>();
            ingredientSlots[type] = (slotInstance, stockText);
        }
    }

    private void CheckInputs()
    {
        if (PlayerInputs.Instance.Book() && !inventoryPanel.enabled)
        {
            OpenInventory();
            return;
        }

        else if (PlayerInputs.Instance.Book() && inventoryPanel.enabled)
        {
            CloseInventory();
            return;
        }
    }

    private void OpenInventory()
    {
        inventoryPanel.enabled = true;

        foreach (var kvp in ingredientSlots)
        {
            kvp.Value.slot.SetActive(true);
            int stock = IngredientInventoryManager.Instance.GetStock(kvp.Key);
            kvp.Value.text.text = stock.ToString();
        }
    }

    private void CloseInventory()
    {
        inventoryPanel.enabled = false;

        foreach (var kvp in ingredientSlots)
        {
            kvp.Value.slot.SetActive(false);
        }
    }
}
