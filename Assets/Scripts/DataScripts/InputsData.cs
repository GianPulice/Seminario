using UnityEngine;

[CreateAssetMenu(fileName = "InputsData", menuName = "ScriptableObjects/Create New InputsData")]
public class InputsData : ScriptableObject
{
    [Header("Tabern Inputs:")]
    [SerializeField] private KeyCode showOrHideDish;

    [Header("Dungeon Inputs:")]
    [SerializeField] private KeyCode shield;
    [SerializeField] private KeyCode attack;

    [Header("Both Inputs:")]
    [SerializeField] private KeyCode interact;
    [SerializeField] private KeyCode run;
    [SerializeField] private KeyCode jump;
    [SerializeField] private KeyCode inventory;
    [SerializeField] private KeyCode pause;
    [SerializeField] private KeyCode debug;

    [Header("Sensitivity:")]
    [SerializeField] private float sensitivityX;
    [SerializeField] private float sensitivityY;

    // --- Tabern Inputs ---
    public KeyCode ShowOrHideDish { get => showOrHideDish; }

    // --- Dungeon Inputs ---
    public KeyCode Attack { get => attack; }
    public KeyCode Shield { get => shield; }

    //--- General Inputs ---    
    public KeyCode Run { get => run; }
    public KeyCode Jump { get => jump; }
    public KeyCode Interact { get => interact; }
    public KeyCode Inventory { get => inventory; }
    public KeyCode Pause { get => pause; }
    public KeyCode Debug { get => debug; }

    public float SensitivityX { get => sensitivityX; set => sensitivityX = value; }
    public float SensitivityY { get => sensitivityY; set => sensitivityY = value; }
}
