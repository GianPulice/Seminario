using System.Collections.Generic;
using UnityEngine;

public class Upgrade6 : MonoBehaviour, IUpgradable
{
    [SerializeField] private UpgradesData upgradesData;

    [SerializeField] private List<GameObject> newCookingDeskUI;
    [SerializeField] private SliderCleanDiirtyTableUIData sliderCleanDiirtyTableUIData;

    private bool isUnlocked = false;

    public UpgradesData UpgradesData => upgradesData;

    public bool CanUpgrade => !isUnlocked;


    // Provisorio restaurar el valor del MaxHoldTime porque los scriptable objects no restauran su valor
    void OnApplicationQuit()
    {
        sliderCleanDiirtyTableUIData.MaxHoldTime = 5f;
    }


    public void Unlock()
    {
        foreach (var kitchen  in newCookingDeskUI) // Desbloquear soportes de cocina nueva
        {
            kitchen.gameObject.SetActive(true);
        }

        sliderCleanDiirtyTableUIData.MaxHoldTime *= 0.65f; // Aumentar tiempo de limpieza de las mesas.

        isUnlocked = true;
    }
}
