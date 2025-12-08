using UnityEngine;

public enum SliderCleanDirtyTableType
{
    SliderBar, SliderRadial
}

[CreateAssetMenu(fileName = "SliderCleanDiirtyTableUIData", menuName = "ScriptableObjects/Tabern/Create New SliderCleanDiirtyTableUIData")]
public class SliderCleanDiirtyTableUIData : ScriptableObject
{
    [SerializeField] private SliderCleanDirtyTableType sliderType;

    [SerializeField] private float initializeMaxHoldTime;

    public SliderCleanDirtyTableType SliderType { get => sliderType; }

    public float InitializeMaxHoldTime { get => initializeMaxHoldTime; }
}
