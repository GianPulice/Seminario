using UnityEngine;

[AddComponentMenu("Combat/Stance Reactive Mover")]
public class StanceReactiveMover : MonoBehaviour
{
    [Header("References")]
    [Tooltip("ShieldHandler a escuchar. Si está vacío, busca en padres.")]
    public ShieldHandler shield;

    [Tooltip("Transform que se moverá o rotará.")]
    public Transform target;

    [Header("Behaviour")]
    [Tooltip("Si TRUE, el target se muestra cuando el escudo está activo.")]
    public bool showWhenShieldActive = true;

    [Tooltip("Si TRUE, invierte el comportamiento típico del arma (baja cuando el escudo sube).")]
    public bool isAxe = false;

    [Tooltip("Desplazamiento local cuando está oculto.")]
    public Vector3 loweredOffset = new Vector3(0f, -0.35f, -0.1f);

    [Header("Smoothness")]
    [Tooltip("Velocidad de interpolación cuando el escudo se activa (sube).")]
    public float smoothActivate = 12f;

    [Tooltip("Velocidad de interpolación cuando el escudo se desactiva (baja).")]
    public float smoothDeactivate = 6f;

    [Tooltip("Interpolar también la rotación.")]
    public bool affectRotation = false;

    // Internos
    private Vector3 combatPos;
    private Quaternion combatRot;
    private Vector3 loweredPos;
    private Quaternion loweredRot;
    private bool targetLowered;
    private float currentSmooth;

    private void Awake()
    {
        if (!target) target = transform;
        if (!shield) shield = GetComponentInParent<ShieldHandler>();

        // Guardar poses base
        combatPos = target.localPosition;
        combatRot = target.localRotation;
        loweredPos = combatPos + loweredOffset;
        loweredRot = combatRot;

        if (shield != null)
        {
            shield.OnShieldActivated += () => UpdateTargetState(true);
            shield.OnShieldDeactivated += () => UpdateTargetState(false);

            // Inicializa al estado actual del escudo
            UpdateTargetState(shield.IsActive);
        }
    }

    private void OnDestroy()
    {
        if (shield != null)
        {
            shield.OnShieldActivated -= () => UpdateTargetState(true);
            shield.OnShieldDeactivated -= () => UpdateTargetState(false);
        }
    }

    private void UpdateTargetState(bool shieldActive)
    {
        // Si es hacha, invertimos la lógica automáticamente
        bool effectiveShow = isAxe ? !shieldActive : shieldActive;

        bool shouldShow = showWhenShieldActive ? effectiveShow : !effectiveShow;
        targetLowered = !shouldShow;

        // Configurar la suavidad según la dirección del movimiento
        currentSmooth = targetLowered ? smoothDeactivate : smoothActivate;
    }

    private void LateUpdate()
    {
        Vector3 targetPos = targetLowered ? loweredPos : combatPos;
        Quaternion targetRot = targetLowered ? loweredRot : combatRot;

        target.localPosition = Vector3.Lerp(target.localPosition, targetPos, Time.deltaTime * currentSmooth);
        if (affectRotation)
            target.localRotation = Quaternion.Slerp(target.localRotation, targetRot, Time.deltaTime * currentSmooth);
    }
}
