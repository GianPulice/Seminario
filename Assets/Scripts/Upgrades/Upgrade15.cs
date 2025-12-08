using UnityEngine;

public class Upgrade15 : MonoBehaviour, IUpgradable
{
    [SerializeField] private UpgradesData upgradesData;

    [SerializeField] private ClientType clientToAdd;

    private bool isUnlocked = false;

    public UpgradesData UpgradesData => upgradesData;

    public bool CanUpgrade => !isUnlocked;


    public void Unlock()
    {
        // Ganar el juego

        ClientManager.Instance.AvailableClientTypes.Add(clientToAdd); // Agregar nuevo cliente

        isUnlocked = true;
    }
}
