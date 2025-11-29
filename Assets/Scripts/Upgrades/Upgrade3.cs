using System.Collections.Generic;
using UnityEngine;

public class Upgrades3 : MonoBehaviour, IUpgradable
{
    [SerializeField] private UpgradesData upgradesData;

    [SerializeField] private List<GameObject> newCookingDeskUI;
    [SerializeField] private List<FoodSupport> foodSupportsToActive;

    private bool isUnlocked = false;

    public UpgradesData UpgradesData => upgradesData;

    public bool CanUpgrade => !isUnlocked;


    public void Unlock()
    {
        foreach (var kitchen  in newCookingDeskUI) // Desbloquear soportes de cocina nueva
        {
            kitchen.gameObject.SetActive(true);
        }

        if (foodSupportsToActive != null)
        {
            foreach (var table in foodSupportsToActive) // Desbloquear soportes de comida
            {
                table.gameObject.SetActive(true);
            }
        }

        isUnlocked = true;
    }
}
