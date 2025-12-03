using System.Collections.Generic;
using UnityEngine;

public class Upgrade8 : MonoBehaviour, IUpgradable
{
    [SerializeField] private UpgradesData upgradesData;

    [SerializeField] private List<GameObject> debrisSecondFloor;
    [SerializeField] private List<Table> tablesToActive;
    [SerializeField] private List<FoodSupport> foodSupports;

    private bool isUnlocked = false;

    public UpgradesData UpgradesData => upgradesData;

    public bool CanUpgrade => !isUnlocked;


    public void Unlock()
    {
        if (debrisSecondFloor != null) // Desbloquear escombros
        {
            foreach (var debris in debrisSecondFloor)
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

        if (foodSupports != null)
        {
            foreach (var foodSupports in foodSupports) // Desbloquear soportes
            {
                foodSupports.gameObject.SetActive(true);
            }
        }

        isUnlocked = true;
    }
}
