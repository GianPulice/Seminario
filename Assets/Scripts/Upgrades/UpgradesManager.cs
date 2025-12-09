using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradesManager : Singleton<UpgradesManager>
{
    [SerializeField] private List<MonoBehaviour> upgradeComponents; // Todos los Upgrade1, Upgrade2, etc.
    private List<IUpgradable> upgrades = new List<IUpgradable>();

    private int purchasedUpgradesCount = 0;

    public int PurchasedUpgradesCount => purchasedUpgradesCount;
    public bool AllUpgradesPurchased => purchasedUpgradesCount >= upgrades.Count;
    public bool reachedMoneyToPurchase => !AllUpgradesPurchased && MoneyManager.instance.CurrentMoney >= upgrades[purchasedUpgradesCount].UpgradesData.Cost;

    public event Action<bool> OnCanPurchaseStatusChanged;
    public event Action OnAllUpgradesCompleted;
    

    void Awake()
    {
        CreateSingleton(false);
        SubscribeToSaveSystemManagerEvents();
        Initialize();
    }

    void OnDestroy()
    {
        UnsuscribeToSaveSystemManagerEvents();
    }

    public IUpgradable GetUpgrade(int index) =>
        (index >= 0 && index < upgrades.Count) ? upgrades[index] : null;

    public void UnlockUpgrade(int index)
    {
        if (upgrades[index].CanUpgrade)
        {
            upgrades[index].Unlock();
            purchasedUpgradesCount++;

            // --- NOTIFICAR CAMBIOS ---
            if (AllUpgradesPurchased)
            {
                OnAllUpgradesCompleted?.Invoke();    
            }

            // Verificamos de nuevo si alcanza para la siguiente (o si ya no hay mas)
            RefreshAvailabilityState();
        }
    }

    public void RefreshAvailabilityState()
    {
        if (AllUpgradesPurchased) return;

        OnCanPurchaseStatusChanged?.Invoke(reachedMoneyToPurchase);
    }

    public int GetUpgradesCount()
    {
        return upgrades.Count;
    }


    private void SubscribeToSaveSystemManagerEvents()
    {
        SaveSystemManager.OnSaveAllGameData += OnSaveUpgrades;
        SaveSystemManager.OnLoadAllGameData += OnLoadUpgrades;
    }

    private void UnsuscribeToSaveSystemManagerEvents()
    {
        SaveSystemManager.OnSaveAllGameData -= OnSaveUpgrades;
        SaveSystemManager.OnLoadAllGameData -= OnLoadUpgrades;
    }

    private void OnSaveUpgrades()
    {
        SaveData data = SaveSystemManager.LoadGame();
        data.purchasedUpgradesCount = purchasedUpgradesCount;
        SaveSystemManager.SaveGame(data);
    }

    private void OnLoadUpgrades()
    {
        SaveData data = SaveSystemManager.LoadGame();

        purchasedUpgradesCount = data.purchasedUpgradesCount;

        // Forzar desbloqueo de las compras anteriores
        for (int i = 0; i < purchasedUpgradesCount; i++)
        {
            var upgrade = GetUpgrade(i);
            if (upgrade != null && upgrade.CanUpgrade) // CanUpgrade = !isUnlocked
                upgrade.Unlock();
        }

        // Actualizar estado general
        RefreshAvailabilityState();
    }

    private void Initialize()
    {
        purchasedUpgradesCount = 0;
        foreach (var comp in upgradeComponents)
        {
            var upgradeInterface = comp as IUpgradable;
            upgrades.Add(upgradeInterface);
            if (!upgradeInterface.CanUpgrade)
            {
                purchasedUpgradesCount++;
            }
        }
    }
}
