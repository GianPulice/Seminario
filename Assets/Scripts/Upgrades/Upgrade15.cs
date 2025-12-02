using System.Collections.Generic;
using UnityEngine;

public class Upgrade15 : MonoBehaviour, IUpgradable
{
    [SerializeField] private UpgradesData upgradesData;

    private bool isUnlocked = false;

    public UpgradesData UpgradesData => upgradesData;

    public bool CanUpgrade => !isUnlocked;


    public void Unlock()
    {
        // Ganar el juego
    }
}
