using UnityEngine;

/// <summary>
/// Baja y sube el arma (visual) en respuesta al estado del escudo.
/// No bloquea el ataque (eso lo hace CombatHandler al consultar ShieldHandler.IsActive),
/// acá solo manejamos la PRESENTACIÓN de la postura del arma.
/// </summary>
public class WeaponStanceController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Handler del escudo al que nos suscribimos")] public ShieldHandler shield;
    [Tooltip("Transform del arma (por ejemplo, 'Espada') que queremos bajar/subir")] public Transform weaponTransform;

    [Header("Poses (locales)")]
    [Tooltip("Pose en combate (arma arriba / visible)")] public Vector3 combatLocalPos;
    public Vector3 combatLocalEuler;
    [Tooltip("Pose bajada (arma fuera de pantalla o baja)")] public Vector3 loweredLocalPos;
    public Vector3 loweredLocalEuler;

    [Header("Tuning")]
    [Tooltip("Velocidad de interpolación visual")] public float lerpSpeed = 12f;
    [Tooltip("Si no definiste poses, usa un offset relativo desde la pose de combate")] public bool useOffsetFallback = true;
    public Vector3 loweredOffset = new Vector3(0f, -0.35f, -0.1f);

    private bool targetLowered;

    private void Reset()
    {
        // Intento de autollenar referencias en el editor
        if (!shield) shield = GetComponentInParent<ShieldHandler>();
        if (!weaponTransform)
        {
            // Si comparte GameObject con WeaponController, intentar tomar su 'sword'
            var wc = GetComponent<WeaponController>() ?? GetComponentInParent<WeaponController>();
            if (wc != null && wc.gameObject != null)
                weaponTransform = wc.gameObject.transform; // si tu WeaponController esta en el propio arma
        }
    }

    private void Awake()
    {
        if (!shield) shield = GetComponentInParent<ShieldHandler>();

        // Capturar pose de combate si está vacía
        if (weaponTransform != null)
        {
            if (combatLocalPos == Vector3.zero && combatLocalEuler == Vector3.zero)
            {
                combatLocalPos = weaponTransform.localPosition;
                combatLocalEuler = weaponTransform.localEulerAngles;
            }

            if (useOffsetFallback)
            {
                loweredLocalPos = combatLocalPos + loweredOffset;
                loweredLocalEuler = combatLocalEuler; // mismo yaw/pitch/roll por defecto
            }
        }

        if (shield != null)
        {
            shield.OnShieldActivated += HandleShieldOn;
            shield.OnShieldDeactivated += HandleShieldOff;
        }
    }

    private void OnDestroy()
    {
        if (shield != null)
        {
            shield.OnShieldActivated -= HandleShieldOn;
            shield.OnShieldDeactivated -= HandleShieldOff;
        }
    }

    private void HandleShieldOn() { SetLowered(true); }
    private void HandleShieldOff() { SetLowered(false); }

    public void SetLowered(bool lowered)
    {
        targetLowered = lowered;
    }

    private void LateUpdate()
    {
        if (weaponTransform == null) return;

        Vector3 targetPos = targetLowered ? loweredLocalPos : combatLocalPos;
        Quaternion targetRot = Quaternion.Euler(targetLowered ? loweredLocalEuler : combatLocalEuler);

        weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, targetPos, Time.deltaTime * lerpSpeed);
        weaponTransform.localRotation = Quaternion.Slerp(weaponTransform.localRotation, targetRot, Time.deltaTime * lerpSpeed);
    }
}
