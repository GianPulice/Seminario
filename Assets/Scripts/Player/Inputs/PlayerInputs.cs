using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : Singleton<PlayerInputs>
{
    [SerializeField] private InputsData keyboardInputs;
    [SerializeField] private InputsData joystickInputs;

    private PlayerInputActions inputActions; // Representa la clase creada por default del nuevo Inputsystem
    private Vector2 joystick = Vector2.zero;

    [SerializeField] private bool testJoystickButtonsInDebugger;

    public InputsData KeyboardInputs { get => keyboardInputs; }
    public InputsData JoystickInputs { get => joystickInputs; } 


    void Awake()
    {
        CreateSingleton(true);
        SuscribeToUpdateManagerEvent();
        InitializePlayerInputActions();
    }

    // Simulacion de Update
    void UpdatePlayerInputs()
    {
        TestJoystickButtonsInDebbuger();
    }


    public Vector2 GetMoveAxis()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    public Vector2 MouseRotation()
    {
        return new Vector2(Input.GetAxis("Mouse X") * keyboardInputs.SensitivityX, Input.GetAxis("Mouse Y") * keyboardInputs.SensitivityY);
    }

    public Vector2 JoystickRotation()
    {
        return new Vector2(joystick.x * joystickInputs.SensitivityX, joystick.y * joystickInputs.SensitivityY);
    }


    /* -------------------------------------------TABERN----------------------------------------- */

    public bool ShowOrHideDish() => Input.GetKeyDown(keyboardInputs.ShowOrHideDish) || Input.GetKeyDown(joystickInputs.ShowOrHideDish);

    /* -------------------------------------------DUNGEON----------------------------------------- */

    public bool Attack() => Input.GetKeyDown(keyboardInputs.Attack) || Input.GetKeyDown(joystickInputs.Attack); //Click izquierdo
    public bool Shield() => Input.GetKeyDown(keyboardInputs.Shield) || Input.GetKeyDown(joystickInputs.Shield);
    public bool RunHeld() => Input.GetKey(keyboardInputs.Run) || Input.GetKey(joystickInputs.Run);
    public bool _Debug() => Input.GetKeyDown(keyboardInputs.Debug);

    /* -------------------------------------------BOTH----------------------------------------- */

    public bool Run() => Input.GetKeyDown(keyboardInputs.Run) || Input.GetKeyDown(joystickInputs.Run);
    public bool StopRun() => Input.GetKeyDown(keyboardInputs.Run) || Input.GetKeyDown(joystickInputs.Run);
    public bool InteractPress() => Input.GetKeyDown(keyboardInputs.Interact) || Input.GetKeyDown(joystickInputs.Interact);
    public bool InteractHold() => Input.GetKey(keyboardInputs.Interact) || Input.GetKey(joystickInputs.Interact);
    public bool Jump() => Input.GetKeyDown(keyboardInputs.Jump) || Input.GetKeyDown(joystickInputs.Jump);
    public bool Inventory() => Input.GetKeyDown(keyboardInputs.Inventory) || Input.GetKeyDown(joystickInputs.Inventory);
    public bool Pause() => Input.GetKeyDown(keyboardInputs.Pause) || Input.GetKeyDown(joystickInputs.Pause);

    /* -------------------------------------------UI----------------------------------------- */
    
    public KeyCode GetInteractInput() => DeviceManager.Instance.CurrentDevice == Device.Joystick ? instance.joystickInputs.Interact : instance.keyboardInputs.Interact;
    public bool R1() => Input.GetKeyDown(KeyCode.Joystick1Button5);
    public bool L1() => Input.GetKeyDown(KeyCode.Joystick1Button4);


    // No es necesario desuscribirse porque es singleton
    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdatePlayerInputs;
    }

    private void InitializePlayerInputActions()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();

        inputActions.Player.Look.performed += ctx =>
        {
            joystick = ctx.ReadValue<Vector2>();
            if (ctx.control.device is Gamepad)
            {
                // Necesario verificar que no este en pausa para que cuando este pausado no afecta a la UI por el cursor
                if (PauseManager.Instance != null && !PauseManager.Instance.IsGamePaused)
                {
                    DeviceManager.Instance.CurrentDevice = Device.Joystick;
                }
            }
        };

        inputActions.Player.Look.canceled += ctx =>
        {
            joystick = Vector2.zero;
        };
    }

    private void TestJoystickButtonsInDebbuger()
    {
        if (testJoystickButtonsInDebugger)
        {
            for (int i = 0; i < 20; i++)
            {
                if (Input.GetKeyDown(KeyCode.JoystickButton0 + i))
                {
                    Debug.Log("Joystick button " + i + " presionado");
                }
            }
        }
    }
}
