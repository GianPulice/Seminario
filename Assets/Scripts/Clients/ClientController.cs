using UnityEngine;

public class ClientController : MonoBehaviour
{
    private ClientModel clientModel;
    private ClientView clientView;

    private ClientStateLeave<ClientStates> csLeave;
    private ClientStateEating<ClientStates> csEating;

    private FSM<ClientStates> fsm = new FSM<ClientStates>();
    private ITreeNode root;

    private bool onCollisionEnterWithTriggerChair = false;

    public bool OnCollisionEnterWithTriggerChair { get => onCollisionEnterWithTriggerChair; set => onCollisionEnterWithTriggerChair = value; }


    void Awake()
    {
        GetComponents();
    }

    void OnEnable()
    {
        SuscribeToUpdateManagerEvents();
    }

    void OnDisable()
    {
        UnsuscribeToUpdateManagerEvents();
    }

    void Start()
    {
        // Obligatorio inicializar la maquina de estados despues del awake, ya que depende del awake del Model
        InitializeFSM();
        InitializeTree();
    }

    // Simulacion de Update
    void UpdateClientController()
    {
        fsm.OnExecute();
        root?.Execute();
        clientView.RotateOrderUIToLookAtPlayer();
    }

    // Simulacion de FixedUpdate
    void FixedUpdateClientController()
    {
        clientModel.Movement();
    }

    void OnDestroy()
    {
        UnsuscribeToUpdateManagerEvents();

        if (csEating != null)
        {
            csEating.UnsuscribeToPauseManagerEvents();
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        OnTriggerEnterWithChair(collider);
    }


    private void SuscribeToUpdateManagerEvents()
    {
        UpdateManager.OnUpdate += UpdateClientController;
        UpdateManager.OnFixedUpdate += FixedUpdateClientController;
    }

    private void UnsuscribeToUpdateManagerEvents()
    {
        UpdateManager.OnUpdate -= UpdateClientController;
        UpdateManager.OnFixedUpdate -= FixedUpdateClientController;
    }

    private void GetComponents()
    {
        clientModel = GetComponent<ClientModel>();
        clientView = GetComponent<ClientView>();
    }

    private void InitializeFSM()
    {
        ClientStateIdle<ClientStates> csIdle = new ClientStateIdle<ClientStates>(clientModel, clientView);
        ClientStateGoChair<ClientStates> csChair = new ClientStateGoChair<ClientStates>(clientModel, clientView, () => clientModel.CurrentTable.ChairPosition);
        csLeave = new ClientStateLeave<ClientStates>(this, clientModel, clientView, clientModel.ClientManager.OutsidePosition);
        csEating = new ClientStateEating<ClientStates>(clientModel, clientView, csLeave);
        ClientStateWaitingFood<ClientStates> csWaitingFood = new ClientStateWaitingFood<ClientStates>(clientModel, clientView, csLeave, csEating);
        ClientStateWaitingForChair<ClientStates> csWaitingForChair = new ClientStateWaitingForChair<ClientStates>(clientModel, clientView, csLeave);

        csIdle.AddTransition(ClientStates.GoChair, csChair);
        csIdle.AddTransition(ClientStates.WaitingForChair, csWaitingForChair);

        csChair.AddTransition(ClientStates.WaitingFood, csWaitingFood);

        csWaitingFood.AddTransition(ClientStates.Leave, csLeave);
        csWaitingFood.AddTransition(ClientStates.Eating, csEating);

        csLeave.AddTransition(ClientStates.Idle, csIdle);

        csWaitingForChair.AddTransition(ClientStates.GoChair, csChair);
        csWaitingForChair.AddTransition(ClientStates.Leave, csLeave);

        csEating.AddTransition(ClientStates.Leave, csLeave);

        fsm.SetInit(csWaitingForChair);
    }

    private void InitializeTree()
    {
        ActionNode idle = new ActionNode(() => fsm.TransitionTo(ClientStates.Idle));
        ActionNode goChair = new ActionNode(() => fsm.TransitionTo(ClientStates.GoChair));
        ActionNode waitingFood = new ActionNode(() => fsm.TransitionTo(ClientStates.WaitingFood));
        ActionNode leave = new ActionNode(() => fsm.TransitionTo(ClientStates.Leave));
        ActionNode waitingForChair = new ActionNode(() => fsm.TransitionTo(ClientStates.WaitingForChair));
        ActionNode eating = new ActionNode(() => fsm.TransitionTo(ClientStates.Eating));

        // Orden: WaitingForChair, GoChair, WaitingFood, Eating, Leave, Idle

        QuestionNode qLeaveOrEat = new QuestionNode(QuestionLeaveOrEat, eating, leave);
        QuestionNode qIsWaitingForFood = new QuestionNode(QuestionIsWaitingForFood, qLeaveOrEat, waitingFood);
        QuestionNode qCanGoToChair = new QuestionNode(QuestionCanGoToChair, goChair, qIsWaitingForFood);
        QuestionNode qIsChairFreeOrNoT = new QuestionNode(QuestionIsChairFreeOrNot, qCanGoToChair, waitingForChair);
        QuestionNode qIsOutside = new QuestionNode(QuestionIsOutside, idle, qIsChairFreeOrNoT);

        root = qIsOutside;
    }

    private bool QuestionLeaveOrEat()
    {
        if (csEating.IsEating)
        {
            return true;
        }

        return false;
    }

    private bool QuestionIsWaitingForFood()
    {
        if (csLeave.CanLeave)
        {
            return true;
        }

        if (csEating.IsEating)
        {
            return true;
        }

        return false;
    }

    private bool QuestionCanGoToChair()
    {
        if (clientModel.CurrentTable != null)
        {
            // si no esta colisionando con el trigger de la silla
            if (!onCollisionEnterWithTriggerChair)
            {
                return true;
            }

            return false;
        }

        return false;
    }

    private bool QuestionIsChairFreeOrNot()
    {
        if (csLeave.CanLeave)
        {
            return true;
        }

        if (clientModel.CurrentTable != null)
        {
            return true;
        }

        return false;
    }

    private bool QuestionIsOutside()
    {
        // si esta cerca del Transform de OutsidePosition
        if (Vector3.Distance(clientModel.ClientManager.OutsidePosition.position, transform.position) <= 2f)
        {
            return true;
        }

        return false;
    }

    private void OnTriggerEnterWithChair(Collider collider)
    {
        if (collider.gameObject.CompareTag("Chair"))
        {
            onCollisionEnterWithTriggerChair = true;
        }
    }
}
