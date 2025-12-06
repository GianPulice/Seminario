using UnityEngine;

[CreateAssetMenu(fileName = "TabernManagerData", menuName = "ScriptableObjects/Create New TabernManagerData")]
public class TabernManagerData : ScriptableObject
{
    [Tooltip("Poner el valor en 1 representa el tiempo de vida real")]
    [SerializeField] private float timeSpeed;

    [SerializeField] private float maintenanceCostPerDay;
    [SerializeField] private float maintenanceCostPerTable;
    [Range(0f, 100f)]
    [SerializeField] private float taxesPorcentajeFromIncomes;
    
    public float TimeSpeed { get => timeSpeed; }

    public float MaintenanceCostPerDay { get => maintenanceCostPerDay; }
    public float MaintenanceCostPerTable { get => maintenanceCostPerTable; }
    public float TaxesPorcentajeFromIncomes { get => taxesPorcentajeFromIncomes; }
}
