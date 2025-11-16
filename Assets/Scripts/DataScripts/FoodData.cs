using UnityEngine;

[CreateAssetMenu(fileName = "FoodData", menuName = "ScriptableObjects/Tabern/Create New FoodData")]
public class FoodData : ScriptableObject
{
    [SerializeField] private float timeToBeenCooked;
    [SerializeField] private float timeToBeenBurned;

    [SerializeField] private Material foodMaterial;
    [SerializeField] private Material burnedMaterial;
    [SerializeField] private Material rawMaterial;

    public float TimeToBeenCooked { get =>  timeToBeenCooked; }
    public float TimeToBeenBurned { get => timeToBeenBurned; }
    public Material FoodMaterial { get => foodMaterial; }
    public Material BurnedMaterial { get => burnedMaterial; }
    public Material RawMaterial { get => rawMaterial; }
}
