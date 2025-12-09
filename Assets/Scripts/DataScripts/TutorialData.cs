
using UnityEngine;

[CreateAssetMenu(fileName = "Tutorial Data", menuName = "ScriptableObjects/Tabern/Create New TutorialData")]
public class TutorialData : ScriptableObject
{
    [SerializeField]private bool activateTutorial = true;
    public bool ActivateTutorial { get => activateTutorial; }
}
