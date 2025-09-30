using UnityEngine;

public enum PlayerStates
{
    Idle, Walk, Run, Jump, Cook, Admin
}

public enum SpawnPoints
{
    Default, Cooking, Admin
}

public class PlayerModel : MonoBehaviour
{
    [SerializeField] private PlayerTabernData playerTabernData;

    private PlayerCamera playerCamera;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private PhysicMaterial physicMaterial;

    [SerializeField] private Transform CookingZone;
    [SerializeField] private Transform AdministrationZone;

    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private SpawnPoints spawnPosition;

    private float speed;
    private float distanceToGround = 1.05f;
   
    private bool isCooking = false;
    private bool isAdministrating = false;
    private bool isInTeleportPanel = false;

    public PlayerTabernData PlayerTabernData { get => playerTabernData; }

    public PlayerCamera PlayerCamera { get => playerCamera; set => playerCamera = value; }

    public Rigidbody Rb { get => rb; }
    public CapsuleCollider CapsuleCollider { get => capsuleCollider; set => capsuleCollider = value; }
    public PhysicMaterial PhysicsMaterial { get => physicMaterial; }

    public float Speed { get => speed; set => speed = value; }

    public bool IsGrounded { get =>  Physics.SphereCast(transform.position, 0.3f, Vector3.down, out _, distanceToGround, groundLayer); }
    public bool IsCooking { get => isCooking; set => isCooking = value; }
    public bool IsAdministrating { get => isAdministrating; set => isAdministrating = value; }
    public bool IsInTeleportPanel { get => isInTeleportPanel; set => isInTeleportPanel = value; }


    void Awake()
    {
        GetComponents();
        Initialize();
        SpawnPlayerPosition();
    }


    public void Movement()
    {
        if (BookManagerUI.Instance == null) return;
        if (BookManagerUI.Instance.IsBookOpen) return;
        if (PlayerInputs.Instance == null) return;
        if (isCooking || isAdministrating || isInTeleportPanel) return;

        Vector3 cameraForward = playerCamera.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        Vector3 right = playerCamera.transform.right;
        Vector3 movement = (cameraForward * PlayerInputs.Instance.GetMoveAxis().y + right * PlayerInputs.Instance.GetMoveAxis().x).normalized * speed * Time.fixedDeltaTime;

        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);

        if (IsGrounded)
        {
            rb.AddForce(Vector3.down * 10f, ForceMode.Force);
        }

        float targetDrag = IsGrounded ? playerTabernData.GroundDrag : 0.15f;
        rb.drag = Mathf.Lerp(rb.drag, targetDrag, Time.fixedDeltaTime * 10f);        
    }

    public void LookAt(Vector3 target)
    {
        Vector3 newDirection = (target - transform.position).normalized;
        Vector3 lookDirection = new Vector3(newDirection.x, 0, newDirection.z);

        if (lookDirection != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = rotation;
        }
    }

    public void StopVelocity()
    {
        rb.velocity = Vector3.zero;
    }


    private void GetComponents()
    {
        playerCamera = GetComponentInChildren<PlayerCamera>();
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void Initialize()
    {
        physicMaterial = capsuleCollider.material;
    }

    private void SpawnPlayerPosition()
    {
        if (spawnPosition == SpawnPoints.Cooking)
        {
            transform.position = CookingZone.transform.position;  
        }

        else if (spawnPosition == SpawnPoints.Admin)
        {
            transform.position = AdministrationZone.transform.position;
        }
    }
}
