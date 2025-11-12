using System.Collections.Generic;
using UnityEngine;

public class Upgrades3 : MonoBehaviour, IUpgradable
{
    [SerializeField] private UpgradesData upgradesData;

    [SerializeField] private GameObject newCookingDeskUI;
    [SerializeField] private List<FoodSupport> foodSupportsToActive;

    private bool isUnlocked = false;

    public UpgradesData UpgradesData => upgradesData;

    public bool CanUpgrade => !isUnlocked;


    public void Unlock()
    {
        if (foodSupportsToActive != null)
        {
            foreach (var table in foodSupportsToActive) // Desbloquear soportes de comida
            {
                table.gameObject.SetActive(true);
            }
        }

        ClientManager.Instance.ClientManagerData.TimeToWaitForSpawnNewClient = 1; // Aumentar el tiempo

        isUnlocked = true;
    }
}
