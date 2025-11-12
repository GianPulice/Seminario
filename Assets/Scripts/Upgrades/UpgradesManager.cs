using System.Collections.Generic;
using UnityEngine;

public class UpgradesManager : Singleton<UpgradesManager>
{
    [SerializeField] private List<MonoBehaviour> upgradeComponents; // Todos los Upgrade1, Upgrade2, etc.
    private List<IUpgradable> upgrades = new List<IUpgradable>();


    void Awake()
    {
        CreateSingleton(false);
        Initialize();
    }


    public IUpgradable GetUpgrade(int index) => upgrades[index];

    public void UnlockUpgrade(int index)
    {
        upgrades[index].Unlock();
    }


    private void Initialize()
    {
        foreach (var comp in upgradeComponents)
        {
            upgrades.Add(comp as IUpgradable);
        }
    }
}
