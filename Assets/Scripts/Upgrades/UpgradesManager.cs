using System.Collections.Generic;
using UnityEngine;

public class UpgradesManager : Singleton<UpgradesManager>
{
    [SerializeField] private List<MonoBehaviour> upgradeComponents; // Todos los Upgrade1, Upgrade2, etc.
    private List<IUpgradable> upgrades = new List<IUpgradable>();

    private int purchasedUpgradesCount = 0;
    public int PurchasedUpgradesCount => purchasedUpgradesCount;
    void Awake()
    {
        CreateSingleton(false);
        Initialize();
    }
    public IUpgradable GetUpgrade(int index) =>
        (index >= 0 && index < upgrades.Count) ? upgrades[index] : null;

    public void UnlockUpgrade(int index)
    {
        if (upgrades[index].CanUpgrade)
        {
            upgrades[index].Unlock();

            purchasedUpgradesCount++;

            //Debug.Log($"Upgrade desbloqueada. Total compradas: {purchasedUpgradesCount}");
        }
    }
    public int GetUpgradesCount()
    {
        return upgrades.Count;
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
