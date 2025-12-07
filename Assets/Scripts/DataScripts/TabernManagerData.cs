using UnityEngine;

[CreateAssetMenu(fileName = "TabernManagerData", menuName = "ScriptableObjects/Create New TabernManagerData")]
public class TabernManagerData : ScriptableObject
{
    [Header("Poner el valor en 1 en la velocidad representa el tiempo de vida real")]
    [SerializeField] private float timeSpeed;

    [SerializeField] private float maintenanceCostPerDay;
    [SerializeField] private float maintenanceCostPerTable;
    [SerializeField] private float costPerBurntDish;
    [SerializeField] private float costPerBrokenThings;

    [Range(0f, 100f)]
    [SerializeField] private float taxesPorcentajeFromIncomes;
    
    public float TimeSpeed { get => timeSpeed; }

    public float MaintenanceCostPerDay { get => maintenanceCostPerDay; }
    public float MaintenanceCostPerTable { get => maintenanceCostPerTable; }
    public float CostPerBurntDish { get => costPerBurntDish; }
    public float CostPerBrokenThings { get => costPerBrokenThings; }

    public float TaxesPorcentajeFromIncomes { get => taxesPorcentajeFromIncomes; }
}
