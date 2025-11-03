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

    private Rigidbody rb;
    private NavMeshAgent navMeshAgent;
    private CapsuleCollider capsuleCollider;
    private Table currentTable;

    private Vector3 currentDirection;

    [SerializeField] private ClientType clientType;

    private bool isInstantiateFirstTime = true;
    private bool wasTableDirtyWhenSeated = false;

    public ClientData ClientData { get => clientData; }

    public ClientManager ClientManager { get => clientManager; }

    public NavMeshAgent NavMeshAgent { get => navMeshAgent; }
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
