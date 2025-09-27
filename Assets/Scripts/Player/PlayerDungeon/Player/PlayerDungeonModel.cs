using System;
using System.Collections;
using UnityEngine;

public class PlayerDungeonModel : MonoBehaviour, IDamageable
{
    #region Inspector Fields
    [Header("Stamina")]
    [SerializeField] private float staminaRunCostPerSecond = 5f;

    [Header("Movement")]
    [SerializeField] private float speed;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 40f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float airMultiplier = 5f;
    [SerializeField] private float groundDrag = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7f;

    [Header("Combat")]
    [SerializeField] private int currentWeaponDamage = 5;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool showGroundGizmo = true;
    #endregion

    #region Private Fields
    private PlayerHealth playerHealth;
    private PlayerStamina playerStamina;
    private Vector3 moveDirection;
    private Transform orientation;
    private Rigidbody rb;
    private CombatHandler combatHandler;
    private SafetyPoint safetyPoint;

    [SerializeField] private bool debugLogs = true;
    private bool wasRunning = false;
    private float nextRunLogTime = 0f;  // para no spamear, logueamos máx. 1 vez por segundo mientras corre
    #endregion

    #region Properties & Events
    public bool IsInvulnerable { get; private set; } = false;
    public bool CanMove { get; set; } = true;

    public float Speed { get => speed; set => speed = value; }
    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float JumpForce => jumpForce;
    public Rigidbody Rb => rb;
    public Vector3 MoveDirection { get => moveDirection; set => moveDirection = value; }
    public bool IsGrounded => Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    public bool IsFalling => rb.velocity.y < -0.1f && !IsGrounded;
    public int CurrentWeaponDamage { get => currentWeaponDamage; set => currentWeaponDamage = value; }
    public float AttackCooldown { get => attackCooldown; set => attackCooldown = value; }

    public bool isTeleportPannelOpened = false;

    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action OnPlayerDied;

    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        GetComponents();
        PlayerDungeonHUD.OnShowTeleportConfirm += TeleportMessageShow;
        PlayerDungeonHUD.OnHideTeleportConfirm += HideTeleportMessage;
    }
    private void OnDestroy()
    {
        PlayerDungeonHUD.OnShowTeleportConfirm -= TeleportMessageShow;
        PlayerDungeonHUD.OnHideTeleportConfirm -= HideTeleportMessage;
    }
    private void FixedUpdate()
    {
        if (PlayerInputs.Instance != null)
        {
            MovePlayer();
        }
    }

    private void Update()
    {
        HandleInputs();
        SpeedControl();
        rb.drag = IsGrounded ? groundDrag : 0f;
        HandleRunStamina();
    }
    #endregion

    #region Public Methods
    public void TakeDamage(int amount)
    {
        playerHealth.TakeDamage(amount);
    }

    public void SetInvulnerable(bool value) => IsInvulnerable = value;

    public PlayerStamina GetStaminaManager() => playerStamina;

    public void SetTeleportPanel(bool value)
    {
        isTeleportPannelOpened = value;
        rb.velocity = Vector3.zero;
    }
    #endregion

    #region Inputs & Movimiento
    private void HandleInputs()
    {
        if (PlayerInputs.Instance == null) return;

        if (PauseManager.Instance == null) return;
        if (PauseManager.Instance.IsGamePaused) return;

        if (isTeleportPannelOpened) return;
       
        if (PlayerInputs.Instance.Jump())
            Jump();

        if (PlayerInputs.Instance.Shield())
        {
            if (!combatHandler.IsAttacking)
            {
                combatHandler.TryUseShield();
            }
        }
        else
        {
            if (PlayerInputs.Instance.Attack())
            {
                if (!combatHandler.IsShieldActive)
                {
                    combatHandler.TryAttack();
                }
            }
        }
    }

    private void HandleRunStamina()
    {
        if (PlayerInputs.Instance == null) return;

        bool runningHeld = PlayerInputs.Instance.RunHeld();
        if (runningHeld)
        {
            float speedNow = new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;
            if (!wasRunning || Time.time >= nextRunLogTime)
            {
                if (debugLogs) Debug.Log($"[Movimiento] Corriendo a {speedNow:F2} m/s");
                nextRunLogTime = Time.time + 1f; // 1 log/seg mientras corre
            }
            wasRunning = true;
        }
        else if (wasRunning)
        {
            if (debugLogs) Debug.Log("[Movimiento] Dejaste de correr");
            wasRunning = false;
        }

        if (playerStamina != null)
            playerStamina.SetRegenSuppressed(runningHeld);

        if (!IsGrounded) return;

        bool tryingToRun = runningHeld;
        if (tryingToRun && playerStamina != null && playerStamina.CanUse(staminaRunCostPerSecond * Time.deltaTime))
        {
            playerStamina.Use(staminaRunCostPerSecond * Time.deltaTime);
        }

    }

    private void MovePlayer()
    {
        if(isTeleportPannelOpened) return;
        Vector2 input = PlayerInputs.Instance.GetMoveAxis();
        Vector3 targetDir = (orientation.forward * input.y + orientation.right * input.x).normalized;

        bool canRun = PlayerInputs.Instance.RunHeld() && playerStamina != null && playerStamina.CanUse(staminaRunCostPerSecond * Time.deltaTime);
        float targetSpeed = canRun ? runSpeed : walkSpeed;

        Vector3 targetVelocity = targetDir * targetSpeed;
        Vector3 currentVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 velocityChange = targetVelocity - currentVelocity;

        float accelRate = (targetDir.magnitude > 0.1f) ? acceleration : deceleration;
        float frictionBoost = (targetDir.magnitude == 0f) ? 1.5f : 1f;

        Vector3 force = velocityChange * accelRate * frictionBoost;

        const float maxForce = 50f;
        if (force.magnitude > maxForce) force = force.normalized * maxForce;
        force *= rb.mass;

        if (IsGrounded)
        {
            rb.AddForce(new Vector3(force.x, 0f, force.z), ForceMode.Force);
        }
        else
        {
            const float airControl = 0.5f;
            rb.AddForce(new Vector3(force.x, 0f, force.z) * airControl * airMultiplier, ForceMode.Force);
        }

        float targetDrag = IsGrounded ? groundDrag : 0f;
        rb.drag = Mathf.Lerp(rb.drag, targetDrag, Time.deltaTime * 10f);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > walkSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * walkSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        if (!IsGrounded) return;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    #endregion

    #region Death & Respawn
    private void HandleDeath()
    {
        CanMove = false;
        rb.velocity = Vector3.zero;
        playerHealth.SetInvulnerable(true);
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(1f);

        DungeonManager.Instance.OnPlayerDeath();

        playerHealth.Revive();
        CanMove = true;

        Debug.Log("Jugador respawneado en la dungeon");
    }

    private void GetComponents()
    {
        rb = GetComponent<Rigidbody>();
        playerHealth = GetComponent<PlayerHealth>();
        combatHandler = GetComponent<CombatHandler>();
        playerStamina = GetComponent<PlayerStamina>();
        safetyPoint = GetComponent<SafetyPoint>();
        orientation = transform.Find("Orientation");
        rb.freezeRotation = true;

        playerHealth.OnHealthChanged += (current, max) => OnHealthChanged?.Invoke(current, max);
        playerHealth.OnPlayerDied += HandleDeath;

        if (playerStamina != null)
        {
            playerStamina.OnStaminaChanged += (current, max) => OnStaminaChanged?.Invoke(current, max);
        }
    }

    private void TeleportMessageShow(string nada)
    {
        SetTeleportPanel(true);
    }
    private void HideTeleportMessage()
    {
        isTeleportPannelOpened = false;
        if (debugLogs) Debug.Log("[Teleport] Panel cerrado");
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        if (!showGroundGizmo) return;

        Gizmos.color = Color.green;
        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * groundCheckDistance;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(end, 0.05f);
    }
    #endregion
}
