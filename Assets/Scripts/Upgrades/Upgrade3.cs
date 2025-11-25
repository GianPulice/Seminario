using System.Collections.Generic;
using UnityEngine;

public class Upgrades3 : MonoBehaviour, IUpgradable
{
    [SerializeField] private UpgradesData upgradesData;

    [SerializeField] private List<FoodSupport> foodSupportsToActive;
    [SerializeField] private ClientType newClientTypeToAdd;

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

        ClientManager.Instance.AvailableClientTypes.Add(newClientTypeToAdd); // Agregar nuevo cliente

        isUnlocked = true;
    }
}
