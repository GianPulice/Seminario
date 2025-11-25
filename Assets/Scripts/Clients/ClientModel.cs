using UnityEngine;
using UnityEngine.AI;

public enum ClientStates
{
    Idle, GoChair, WaitingFood, WaitingForChair, Leave, Eating
}

public enum ClientType
{
    Ogre, Orc, Goblin 
}

public class ClientModel : MonoBehaviour
{
    [SerializeField] private ClientData clientData;

    private ClientManager clientManager;
    private OrderDataUI currentOrderDataUI;

    private Rigidbody rb;
    private NavMeshAgent navMeshAgent;
    private AudioSource audioSource3D;
    private CapsuleCollider capsuleCollider;
    private Table currentTable;

    private Vector3 currentDirection;

    [SerializeField] private ClientType clientType;

    private bool isInstantiateFirstTime = true;
    private bool wasTableDirtyWhenSeated = false;

    public ClientData ClientData { get => clientData; }

    public ClientManager ClientManager { get => clientManager; }
    public OrderDataUI CurrentOrderDataUI { get => currentOrderDataUI; set => currentOrderDataUI = value; }

    public NavMeshAgent NavMeshAgent { get => navMeshAgent; set => navMeshAgent = value; }
    public AudioSource AudioSource3D { get => audioSource3D; }
    public Table CurrentTable { get => currentTable; set => currentTable = value; }

    public ClientType ClientType { get => clientType; }

    public bool WasTableDirtyWhenSeated { get => wasTableDirtyWhenSeated; set => wasTableDirtyWhenSeated = value; }


    void Awake()
    {
        GetComponents();
        InitializeValuesFromNavMeshAgent();

        /// Provisorio
        navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        //navMeshAgent.avoidancePriority = 0;
    }

    void OnEnable()
    {
        InitializeClientForPool();
    }


    public void Movement()
    {
        if (!rb.isKinematic)
        {
            rb.velocity = currentDirection * clientData.Speed * Time.fixedDeltaTime;
        }
    }

    // SetDestination ademas de rotar al player hacia el destino, frana gradualmente la velocidad cuando llega al destiono
    public void MoveToTarget(Vector3 target)
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.updatePosition = true;
        navMeshAgent.updateRotation = true;
        navMeshAgent.SetDestination(target);
    }

    public void LookAt(Vector3 target, Transform anim)
    {
        Vector3 newDirection = (target - transform.position).normalized;
        Vector3 lookDirection = new Vector3(newDirection.x, 0, newDirection.z);

        if (lookDirection != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = rotation;
            anim.transform.rotation = rotation;
        }
    }

    public void StopVelocity()
    {
        navMeshAgent.isStopped = true;
        //navMeshAgent.updatePosition = false;
        //navMeshAgent.updateRotation = false;
        navMeshAgent.velocity = Vector3.zero;
    }

    /// Metodo para futuro setea del npc correctamente la posicion de la animacion en la silla
    public void SetRbAndCollider(bool rb, bool collider)
    {
        this.rb.isKinematic = rb;
        capsuleCollider.enabled = collider;
    }

    public void ReturnFoodFromTableToPool()
    {
        foreach (Food food in currentTable.CurrentFoods)
        {
            food.ReturnObjetToPool();
        }

        currentTable.CurrentFoods.Clear();
    }


    private void GetComponents()
    {
        clientManager = FindFirstObjectByType<ClientManager>();

        rb = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        audioSource3D = GetComponentInChildren<AudioSource>();
        capsuleCollider =  GetComponent<CapsuleCollider>();
    }

    private void InitializeValuesFromNavMeshAgent()
    {
        navMeshAgent.speed = clientData.Speed;
    }

    public void InitializeSpawnPosition()
    {
        transform.position = clientManager.SpawnPosition.position;
    }

    public void InitializeTablePosition()
    {
        currentTable = TablesManager.Instance.GetRandomAvailableTableForClient();
    }

    private void InitializeClientForPool()
    {
        if (!isInstantiateFirstTime)
        {
            InitializeSpawnPosition();
            InitializeTablePosition();
            return;
        }

        isInstantiateFirstTime = false;
    }
}
