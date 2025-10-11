using UnityEngine;

public class blockingtest : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private Transform cameraRef;

    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    private float currentSpeed;

    // --- VARIABLE DE SALTO ---
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f;
    private bool jumpInput; // Para almacenar la entrada del salto

    private float groundDrag = 5f;
    [SerializeField] private LayerMask groundLayer;
    private float rotationX = 0f;
    [SerializeField] private float rotationSpeedMult = 1f;
    private const float GroundCheckDistance = 1.05f;
    private const float GroundCheckRadius = 0.3f;

    private bool IsGrounded => Physics.SphereCast(
        transform.position,
        GroundCheckRadius,
        Vector3.down,
        out _,
        GroundCheckDistance,
        groundLayer
    );

    private float mouseX, mouseY;
    private float horizontalInput, verticalInput;
    private bool isRunning;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("blockingtest requires a Rigidbody component on the same GameObject.");
            enabled = false;
            return;
        }

        // Es importante evitar rotaciones no deseadas que pueden causar problemas de física.
        rb.freezeRotation = true;

        currentSpeed = walkSpeed;
        Cursor.lockState = CursorLockMode.Locked;

        if (transform.childCount > 0)
        {
            cameraRef = transform.GetChild(0);
        }
        else
        {
            Debug.LogError("No child transform found to act as the camera reference for rotation.");
            cameraRef = Camera.main?.transform;
        }
    }

    private void Update()
    {
        GetUnityDefaultInput();
        HandleSpeed();
        HandleCameraRotation();

        // Manejamos la lógica de salto en FixedUpdate
        if (jumpInput)
        {
            Jump();
            jumpInput = false; // Consumimos la entrada inmediatamente
        }
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void GetUnityDefaultInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        isRunning = Input.GetKey(KeyCode.LeftShift);

        // --- NUEVA ENTRADA DE SALTO (ESPACIO) ---
        // Usamos GetKeyDown para asegurarnos de que solo se salte una vez por pulsación
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpInput = true;
        }
    }

    private void HandleSpeed()
    {
        bool isMoving = horizontalInput != 0 || verticalInput != 0;

        if (isRunning && isMoving)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, runSpeed, Time.deltaTime * 10f);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, walkSpeed, Time.deltaTime * 10f);
        }
    }

    // --- NUEVO MÉTODO DE SALTO ---
    private void Jump()
    {
        // Solo saltamos si estamos en el suelo
        if (IsGrounded)
        {
            // Resetear la velocidad vertical para asegurar un salto consistente (opcional)
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // Aplicar la fuerza de salto
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }


    private void HandleCameraRotation()
    {
        float x = mouseX * rotationSpeedMult * Time.deltaTime * 150f;
        float y = mouseY * rotationSpeedMult * Time.deltaTime * 150f;

        // Rotación Vertical (Eje X) -> Aplicada a la CÁMARA (Hijo)
        rotationX -= y;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        if (cameraRef != null)
        {
            cameraRef.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        }

        transform.Rotate(Vector3.up * x);
    }

    private void Movement()
    {
        Vector3 cameraForward;
        Vector3 right;

        if (cameraRef != null)
        {
            cameraForward = transform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();

            right = transform.right;
        }
        else
        {
            cameraForward = Vector3.forward;
            right = Vector3.right;
        }

        Vector3 movementDirection = (cameraForward * verticalInput + right * horizontalInput).normalized;
        Vector3 movement = movementDirection * currentSpeed;

        // Aplicamos la velocidad horizontal, manteniendo la velocidad vertical (que incluye la gravedad/salto)
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);

        if (IsGrounded)
        {
            // Pequeña fuerza descendente para 'pegar' al jugador
            rb.AddForce(Vector3.down * 10f, ForceMode.Force);
        }

        float targetDrag = IsGrounded ? groundDrag : 0.15f;
        rb.drag = Mathf.Lerp(rb.drag, targetDrag, Time.fixedDeltaTime * 10f);
    }
}