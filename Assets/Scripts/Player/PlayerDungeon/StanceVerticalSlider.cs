using UnityEngine;

/// <summary>
/// Desliza SOLO el eje Y (local) del 'target' cuando el escudo se activa/desactiva.
/// Úsalo tanto para el ancla del arma como para el ancla del escudo.
/// </summary>
public class StanceVerticalSlider : MonoBehaviour
{
    [Header("References")]
    [Tooltip("ShieldHandler a escuchar. Si está vacío, busca en padres.")]
    public ShieldHandler shield;

    [Tooltip("Transform a deslizar. Si está vacío, usa este GameObject.")]
    public Transform target;

    [Header("Behaviour")]
    [Tooltip("Si TRUE, el target sube (se muestra) cuando el escudo está ACTIVO, y baja cuando está INACTIVO.\nSi FALSE, al revés (útil para el arma).")]
    public bool showWhenShieldActive = false;

    [Tooltip("Desplazamiento vertical (en local) hacia ABAJO para la pose 'baja'.\nNegativo = baja respecto a la pose de combate.")]
    public float loweredOffsetY = -0.35f;

    [Tooltip("Tiempo de suavizado SmoothDamp (segundos). 0.06–0.12 suele ir bien.")]
    public float smoothTime = 0.08f;

    [Header("Debug")]
    public bool snapOnStart = true; // coloca el target en el estado correcto al iniciar

    private float combatY;          // Y local "arriba"
    private float loweredY;         // Y local "abajo" = combatY + loweredOffsetY
    private float targetY;          // Y objetivo actual
    private float yVelocity;        // estado interno de SmoothDamp

    private void Awake()
    {
        if (!target) target = transform;
        if (!shield) shield = GetComponentInParent<ShieldHandler>();

        // Capturamos la Y de combate al arrancar
        combatY = target.localPosition.y;
        loweredY = combatY + loweredOffsetY;

        if (shield != null)
        {
            shield.OnShieldActivated += HandleShieldOn;
            shield.OnShieldDeactivated += HandleShieldOff;
        }

        // Estado inicial
        bool shieldActive = shield != null && shield.IsActive;
        targetY = ComputeTargetY(shieldActive);
        if (snapOnStart) SnapToTarget();
    }

    private void OnDestroy()
    {
        if (shield != null)
        {
            shield.OnShieldActivated -= HandleShieldOn;
            shield.OnShieldDeactivated -= HandleShieldOff;
        }
    }

    private void HandleShieldOn() => targetY = ComputeTargetY(true);
    private void HandleShieldOff() => targetY = ComputeTargetY(false);

    private float ComputeTargetY(bool shieldActive)
    {
        // si showWhenShieldActive==true => “sube cuando escudo activo”
        bool shouldShow = showWhenShieldActive ? shieldActive : !shieldActive;
        return shouldShow ? combatY : loweredY;
    }

    private void SnapToTarget()
    {
        Vector3 lp = target.localPosition;
        lp.y = targetY;
        target.localPosition = lp;
    }

    private void LateUpdate()
    {
        // SmoothDamp SOLO en Y local
        Vector3 lp = target.localPosition;
        lp.y = Mathf.SmoothDamp(lp.y, targetY, ref yVelocity, smoothTime);
        target.localPosition = lp;
    }
}
