using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerModel playerModel;
    private PlayerView playerView;
    private PlayerCollisions playerCollisions;

    private FSM<PlayerStates> fsm;
    private PlayerStateCook<PlayerStates> psCook;
    private PlayerStateAdministration<PlayerStates> psAdmin;

    private static event Action onHandOverFood;
    private static event Action onTakeOrder;
    private static event Action<GameObject> onSupportFood;
    private static event Action onThrowFoodToTrash;

    // Estos 2 eventos corresponden a entregar el plato una vez tomado el pedido
    private static event Action<Table> onTableCollisionEnterForHandOverFood;
    private static event Action onTableCollisionExitForHandOverFood;

    // Estos 2 eventos corresponden a tomar el pedido de un cliente
    private static event Action<Table> onTableCollisionEnterForTakeOrder;
    private static event Action onTableCollisionExitForTakeOrder;

    // Estos 2 eventos corresponden a limpiar la mesa sucia
    private static event Action<Table> onCleanDirtyTableIncreaseSlider;
    private static event Action<Table> onCleanDirtyTableDecreaseSlider;

    public PlayerModel PlayerModel { get => playerModel; }
    public PlayerView PlayerView { get => playerView; }

    public static Action OnHandOverFood { get => onHandOverFood; set => onHandOverFood = value; }
    public static Action OnTakeOrder { get => onTakeOrder; set => onTakeOrder = value; }
    public static Action<GameObject> OnSupportFood { get => onSupportFood; set => onSupportFood = value; }
    public static Action OnThrowFoodToTrash { get => onThrowFoodToTrash; set => onThrowFoodToTrash = value; }

    // Estos 2 eventos corresponden a Entregar el plato una vez tomado el pedido
    public static Action<Table> OnTableCollisionEnterForHandOverFood { get => onTableCollisionEnterForHandOverFood; set => onTableCollisionEnterForHandOverFood = value; }
    public static Action OnTableCollisionExitForHandOverFood { get => onTableCollisionExitForHandOverFood; set => onTableCollisionExitForHandOverFood = value; }

    // Estos 2 eventos corresponden a Tomar el pedido de un cliente
    public static Action<Table> OnTableCollisionEnterForTakeOrder { get => onTableCollisionEnterForTakeOrder; set => onTableCollisionEnterForTakeOrder = value; }
    public static Action OnTableCollisionExitForTakeOrder { get => onTableCollisionExitForTakeOrder; set => onTableCollisionExitForTakeOrder = value; }

    // Estos 2 eventos corresponden a limpiar la mesa sucia
    public static Action<Table> OnCleanDirtyTableIncreaseSlider { get => onCleanDirtyTableIncreaseSlider; set => onCleanDirtyTableIncreaseSlider = value; }
    public static Action<Table> OnCleanDirtyTableDecreaseSlider { get => onCleanDirtyTableDecreaseSlider; set => onCleanDirtyTableDecreaseSlider = value; }


    void Awake()
    {
        SuscribeToUpdateManagerEvents();
        GetComponentsAndInitializeReferences();
        InitializeFSM();
    }

    // Simulacion de Update
    void UpdatePlayerController()
    {
        fsm.OnExecute();
        CheckInputs();
    }

    // Simulacion de FixedUpdate
    void FixedUpdatePlayerController()
    {
        playerModel.Movement();
    }

    void OnDestroy()
    {
        UnsuscribeToUpdateManagerEvents();
        psCook.UnsuscribeToEventWhenPlayerDestroy();
        psAdmin.UnsuscribeToEventWhenPlayerDestroy();
    }

    void OnCollisionEnter(Collision collision)
    {
        playerCollisions.OnCollisionsEnter(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        playerCollisions.OnCollisionsStay(collision);
    }

    void OnCollisionExit(Collision collision)
    {
        playerCollisions.OnCollisionsExit(collision);
    }

    void OnTriggerEnter(Collider collider)
    {
        playerCollisions.OnTriggerEnter(collider);
    }


    private void SuscribeToUpdateManagerEvents()
    {
        UpdateManager.OnUpdate += UpdatePlayerController;
        UpdateManager.OnFixedUpdate += FixedUpdatePlayerController;
    }

    private void UnsuscribeToUpdateManagerEvents()
    {
        UpdateManager.OnUpdate -= UpdatePlayerController;
        UpdateManager.OnFixedUpdate -= FixedUpdatePlayerController;
    }

    private void GetComponentsAndInitializeReferences()
    {
        playerModel = GetComponent<PlayerModel>();
        playerView = GetComponent<PlayerView>();
        playerCollisions = new PlayerCollisions(this);
    }

    private void InitializeFSM() 
    {
        fsm = new FSM<PlayerStates>();

        PlayerStateIdle<PlayerStates> psIdle = new PlayerStateIdle<PlayerStates>(PlayerStates.Walk, PlayerStates.Jump, PlayerStates.Cook, PlayerStates.Admin, playerModel);
        PlayerStateWalk<PlayerStates> psWalk = new PlayerStateWalk<PlayerStates> (PlayerStates.Idle, PlayerStates.Run, PlayerStates.Jump, PlayerStates.Cook, PlayerStates.Admin, playerModel);
        PlayerStateJump<PlayerStates> psJump = new PlayerStateJump<PlayerStates>(PlayerStates.Idle, playerModel);
        psCook = new PlayerStateCook<PlayerStates>(PlayerStates.Idle, playerModel, playerView);
        PlayerStateRun<PlayerStates> psRun = new PlayerStateRun<PlayerStates>(PlayerStates.Idle, PlayerStates.Walk, PlayerStates.Jump, PlayerStates.Cook, PlayerStates.Admin, playerModel);
        psAdmin = new PlayerStateAdministration<PlayerStates>(PlayerStates.Idle, playerModel, playerView);

        psIdle.AddTransition(PlayerStates.Walk, psWalk);
        psIdle.AddTransition(PlayerStates.Jump, psJump);
        psIdle.AddTransition(PlayerStates.Cook, psCook);
        psIdle.AddTransition(PlayerStates.Admin, psAdmin);

        psWalk.AddTransition(PlayerStates.Idle, psIdle);
        psWalk.AddTransition(PlayerStates.Jump, psJump);
        psWalk.AddTransition(PlayerStates.Cook, psCook);
        psWalk.AddTransition(PlayerStates.Run, psRun);
        psWalk.AddTransition(PlayerStates.Admin, psAdmin);
        
        psJump.AddTransition(PlayerStates.Idle, psIdle);
        psJump.AddTransition(PlayerStates.Walk, psWalk);

        psCook.AddTransition(PlayerStates.Idle, psIdle);

        psRun.AddTransition(PlayerStates.Idle, psIdle);
        psRun.AddTransition(PlayerStates.Walk, psWalk);
        psRun.AddTransition(PlayerStates.Jump, psJump);
        psRun.AddTransition(PlayerStates.Cook, psCook);
        psRun.AddTransition(PlayerStates.Admin, psAdmin);

        psAdmin.AddTransition(PlayerStates.Idle, psIdle);

        fsm.SetInit(psIdle);
    }


    private void CheckInputs()
    {
        if (PlayerInputs.Instance == null) return;
        if (PauseManager.Instance == null) return;
        if (PauseManager.Instance.IsGamePaused) return;
        if (playerModel.IsCooking || playerModel.IsAdministrating || playerModel.IsInTeleportPanel || playerModel.IsInTrashPanel) return;

        ShowOrHideDish();
    }

    private void ShowOrHideDish()
    {
        if (PlayerInputs.Instance.ShowOrHideDish())
        {
            foreach (Transform child in playerView.Dish.transform)
            {
                // Verifica que las posiciones de la bandeja tengan hijos (COMIDAS), es decir si tienen hijos termina el metodo
                if (child.childCount > 0) return;
            }

            if (playerView.Dish.activeSelf)
            {
                playerView.ShowOrHideDish(false);
            }

            else
            {
                playerView.ShowOrHideDish(true);
            }
        }   
    }
}
