using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientInventoryManagerUI : MonoBehaviour
{
    private PlayerModel playerModel;

    [SerializeField] private RawImage inventoryPanel;

    private Transform slotParentObject;
    private List<Transform> slotPositions = new List<Transform>();
    private Dictionary<IngredientType, (GameObject slot, TextMeshProUGUI text)> ingredientSlots = new();

    private static Action onInventoryOpen;

    public static Action OnInventoryOpen { get => onInventoryOpen; set => onInventoryOpen = value; }


    void Awake()
    {
        SuscribeToUpdateManagerEvent();
        SuscribeToPauseManagerRestoreSelectedGameObjectEvent();
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
        UnscribeToPauseManagerRestoreSelectedGameObjectEvent();
    }


    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdateIngredientInventoryManagerUI;
    }

    private void UnsuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate -= UpdateIngredientInventoryManagerUI;
    }

    private void SuscribeToPauseManagerRestoreSelectedGameObjectEvent()
    {
        PauseManager.OnRestoreSelectedGameObject += OnRestoreCenterPointUICorrectlyIfGameWasPaused;
    }

    private void UnscribeToPauseManagerRestoreSelectedGameObjectEvent()
    {
        PauseManager.OnRestoreSelectedGameObject -= OnRestoreCenterPointUICorrectlyIfGameWasPaused;
    }

    // La funcionalidad basicamente es que si despuesa el juego mientras tiene abierto el inventario, desaparezca el punto de interaccion
    private void OnRestoreCenterPointUICorrectlyIfGameWasPaused()
    {
        StartCoroutine(DisableCenterPointUICorrutine());
    }

    private IEnumerator DisableCenterPointUICorrutine()
    {
        yield return null;

        if (inventoryPanel.enabled)
        {
            InteractionManagerUI.Instance.ShowOrHideCenterPointUI(false);
        }
    }

    private void GetComponents()
    {
        playerModel = FindFirstObjectByType<PlayerModel>();
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
        if (PlayerInputs.Instance == null) return;
        if (PauseManager.Instance == null) return;
        if (PauseManager.Instance.IsGamePaused) return;
        if (playerModel.IsAdministrating || playerModel.IsCooking) return;

        if (PlayerInputs.Instance.Inventory() && !inventoryPanel.enabled)
        {
            OpenInventory();
            return;
        }

        else if (PlayerInputs.Instance.Inventory() && inventoryPanel.enabled)
        {
            CloseInventory();
            return;
        }
    }

    private void OpenInventory()
    {
        onInventoryOpen?.Invoke();
        InteractionManagerUI.Instance.ShowOrHideCenterPointUI(false);
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
        InteractionManagerUI.Instance.ShowOrHideCenterPointUI(true);
        inventoryPanel.enabled = false;

        foreach (var kvp in ingredientSlots)
        {
            kvp.Value.slot.SetActive(false);
        }
    }
}
