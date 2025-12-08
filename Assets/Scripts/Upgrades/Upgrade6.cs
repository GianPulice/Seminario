using System;
using System.Collections.Generic;
using UnityEngine;

public class Upgrade6 : MonoBehaviour, IUpgradable
{
    [SerializeField] private UpgradesData upgradesData;

    [SerializeField] private List<GameObject> newCookingDeskUI;

    private static event Action onDecreaseCleanTableMaxHoldTime;

    private bool isUnlocked = false;

    public UpgradesData UpgradesData => upgradesData;

    public static Action OnDecreaseCleanTableMaxHoldTime { get => onDecreaseCleanTableMaxHoldTime; set => onDecreaseCleanTableMaxHoldTime = value; }

    public bool CanUpgrade => !isUnlocked;


    public void Unlock()
    {
        foreach (var kitchen  in newCookingDeskUI) // Desbloquear soportes de cocina nueva
        {
            kitchen.gameObject.SetActive(true);
        }

        onDecreaseCleanTableMaxHoldTime?.Invoke(); // Aumentar tiempo de limpieza de las mesas.

        isUnlocked = true;
    }
}
