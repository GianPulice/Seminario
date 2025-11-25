using System.Collections.Generic;
using UnityEngine;

public class Upgrade4 : MonoBehaviour, IUpgradable
{
    [SerializeField] private UpgradesData upgradesData;

    [SerializeField] private List<GameObject> debrisFirstFloor;
    [SerializeField] private List<Table> tablesToActive;

    private bool isUnlocked = false;

    public UpgradesData UpgradesData => upgradesData;

    public bool CanUpgrade => !isUnlocked;


    public void Unlock()
    {
        if (debrisFirstFloor != null) // Desbloquear escombros
        {
            foreach (var debris in debrisFirstFloor) 
            {
                debris.gameObject.SetActive(false);
            }
        }

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
