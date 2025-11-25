using UnityEngine;

[CreateAssetMenu(fileName = "SliderCleanDiirtyTableUIData", menuName = "ScriptableObjects/Tabern/Create New SliderCleanDiirtyTableUIData")]
public class SliderCleanDiirtyTableUIData : ScriptableObject
{
    [SerializeField] private float maxHoldTime;

    public float MaxHoldTime { get => maxHoldTime; set => maxHoldTime = value; }
}
