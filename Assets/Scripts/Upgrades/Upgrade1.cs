using System.Collections.Generic;
using UnityEngine;

public class Upgrade1 : MonoBehaviour, IUpgradable
{
    [SerializeField] private UpgradesData upgradesData;

    [SerializeField] private List<Table> tablesToActive;

    private bool isUnlocked = false;

    public UpgradesData UpgradesData => upgradesData;

    public bool CanUpgrade => !isUnlocked;


    public void Unlock()
    {
        if (tablesToActive != null)
        {
            foreach (var table in tablesToActive) // Desbloquear mesas
            {
                table.gameObject.SetActive(true);
            }
        }

        isUnlocked = true;
    }
}
