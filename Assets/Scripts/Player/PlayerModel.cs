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
    private bool isInTrashPanel = false;
    private bool isInTutorial = false;
    private bool isInResumeDayPanel = false;

    private bool readyToJump = true;
    private bool exitingSlope = false;

    private RaycastHit slopeHit;
    private Vector3 moveDir;
    public PlayerTabernData PlayerTabernData { get => playerTabernData; }

    public PlayerCamera PlayerCamera { get => playerCamera; set => playerCamera = value; }

    public Rigidbody Rb { get => rb; }
    public CapsuleCollider CapsuleCollider { get => capsuleCollider; set => capsuleCollider = value; }
    public PhysicMaterial PhysicsMaterial { get => physicMaterial; }

    public float Speed { get => speed; set => speed = value; }

    public bool IsGrounded { get => Physics.Raycast(transform.position, Vector3.down, playerTabernData.PlayerHeight / 2 + 0.2f, groundLayer); }
    public bool IsCooking { get => isCooking; set => isCooking = value; }
    public bool IsAdministrating { get => isAdministrating; set => isAdministrating = value; }
    public bool IsInTeleportPanel { get => isInTeleportPanel; set => isInTeleportPanel = value; }
    public bool IsInTrashPanel { get => isInTrashPanel; set => isInTrashPanel = value; }
    public bool IsInTutorial { get => isInTutorial; set => isInTutorial = value; }
    public bool IsInResumeDayPanel { get => isInResumeDayPanel; }
    public bool ReadyToJump { get => readyToJump; set => readyToJump = value; }

    void Awake()
    {
        SuscribeToTutorialEvents();
        SuscribeToResumeDayEvents();
        GetComponents();
        Initialize();
        SpawnPlayerPosition();
    }

    void OnDestroy()
    {
        UnsuscribeToTutorialEvents();
        UnsuscribeToResumeDayEvents();
    }

    public void HandleMovement()
    {
        if (PlayerInputs.Instance == null) return;
        if (isCooking || isAdministrating || isInTeleportPanel || isInTrashPanel || isInTutorial || isInResumeDayPanel) return;

        Vector2 input = PlayerInputs.Instance.GetMoveAxis();

        moveDir = transform.forward * input.y + transform.right * input.x;
        float currentSpeed = speed;

        if (OnSlope()&& !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * currentSpeed * playerTabernData.SlopeForceMult, ForceMode.Force);
            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * playerTabernData.SlopeStickForce, ForceMode.Force);
        }

        else if (IsGrounded)
            rb.AddForce(moveDir.normalized * currentSpeed * playerTabernData.GroundForceMult, ForceMode.Force);
        else if (!IsGrounded)
            rb.AddForce(moveDir.normalized * currentSpeed * playerTabernData.GroundForceMult * PlayerTabernData.AirMultiplier, ForceMode.Force);

        rb.useGravity = !OnSlope();

    }
    public void JumpStart()
    {
        readyToJump = false;
        HandleJump();
        Invoke(nameof(ResetJump), PlayerTabernData.JumpCooldown);

    }
    public void HandleJump()
    {
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * PlayerTabernData.JumpForce, ForceMode.Impulse);
    }
    public void HandleDrag()
    {
        if (rb == null || playerTabernData == null) return;

        if (IsGrounded)
            rb.drag = playerTabernData.GroundDrag;
        else
            rb.drag = 0;

    }
    public void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > speed)
            {
                rb.velocity = rb.velocity.normalized * speed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            float currentSpeed = speed;
            if (flatVel.magnitude > currentSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * currentSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }

        }

    }
    public void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }
    public void HandleGravity()
    {
        if (IsGrounded) return;
        float gravity = Physics.gravity.y;

        if (rb.velocity.y > 0)
        {
            rb.velocity += Vector3.up * gravity * (playerTabernData.JumpGravityMult - 1) * Time.fixedDeltaTime;
        }
        // Caída --> más gravedad
        else if(rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * gravity * (playerTabernData.FallGravityMult - 1) * Time.fixedDeltaTime;
        }


    }
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerTabernData.PlayerHeight / 2 + 0.5f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < playerTabernData.MaxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
    }

    private void SuscribeToTutorialEvents()
    {
        TutorialScreensManager.OnEnterTutorial += OnEnterInTutorial;
        TutorialScreensManager.OnExitTutorial += OnExitInTutorial;
    }

    private void SuscribeToResumeDayEvents()
    {
        PlayerView.OnEnterInResumeDay += OnEnterInResumeDay;
        PlayerView.OnExitInResumeDay += OnExitInResumeDay;
    }

    private void UnsuscribeToResumeDayEvents()
    {
        PlayerView.OnEnterInResumeDay -= OnEnterInResumeDay;
        PlayerView.OnExitInResumeDay -= OnExitInResumeDay;
    }

    private void UnsuscribeToTutorialEvents()
    {
        TutorialScreensManager.OnEnterTutorial -= OnEnterInTutorial;
        TutorialScreensManager.OnExitTutorial -= OnExitInTutorial;
    }

    private void GetComponents()
    {
        playerCamera = GetComponentInChildren<PlayerCamera>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
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

    private void OnEnterInTutorial()
    {
        isInTutorial = true;
    }

    private void OnExitInTutorial()
    {
        isInTutorial = false;
    }

    private void OnEnterInResumeDay()
    {
        isInResumeDayPanel = true;
    }

    private void OnExitInResumeDay()
    {
        isInResumeDayPanel = false;
    }
}
