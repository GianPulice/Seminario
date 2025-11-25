using System.Collections.Generic;
using UnityEngine;

public class Upgrade6 : MonoBehaviour, IUpgradable
{
    [SerializeField] private UpgradesData upgradesData;

    [SerializeField] private List<GameObject> newCookingDeskUI;
    [SerializeField] private List<Table> tablesToActive;
    [SerializeField] private ClientType newClientTypeToAdd;

    private bool isUnlocked = false;

    public UpgradesData UpgradesData => upgradesData;

    public bool CanUpgrade => !isUnlocked;


    public void Unlock()
    {
        foreach (var kitchen  in newCookingDeskUI) // Desbloquear soportes de cocina nueva
        {
            kitchen.gameObject.SetActive(true);
        }

        if (tablesToActive != null)
        {
            foreach (var table in tablesToActive) // Desbloquear mesas
            {
                table.gameObject.SetActive(true);
            }
        }

        ClientManager.Instance.AvailableClientTypes.Add(newClientTypeToAdd); // Agregar nuevo cliente

        isUnlocked = true;
    }
}
