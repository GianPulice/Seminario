using UnityEngine;

[CreateAssetMenu(fileName = "UpgradesData", menuName = "ScriptableObjects/Tabern/Create New UpgradesData")]
public class UpgradesData : ScriptableObject
{
    [SerializeField] private Sprite imageZoneUnlock;
    [SerializeField] private int cost;
    [SerializeField] private string informationCurrentZone;

    public Sprite ImageZoneUnlock { get => imageZoneUnlock; }
    public int Cost { get => cost; }
    public string InformationCurrentZone { get => informationCurrentZone; }
}