using UnityEngine;
using System;

public class ShieldHandler : MonoBehaviour
{
    [Header("Shield Settings")]
    [SerializeField] private float staminaCost = 25f;
    [SerializeField] private float cooldownDuration = 1.5f;
    [SerializeField] private GameObject shieldObject; // GO visual del escudo

    [Header("Behaviour")]
    [SerializeField] private bool holdToBlock = true;   // true = mantener; false = toggle
    [SerializeField] private bool debugLogs = true;

    // ---- Eventos desacoplados (arma baja, UI, SFX, etc.)
    public event Action OnShieldActivated;
    public event Action OnShieldDeactivated;

    // ---- Backends de stamina (usá el que tengas en el Player)
    private PlayerStamina staminaMgr; // nuestro manager modular

    private float lastShieldUseTime = -999f;
    [SerializeField] private bool isActive;  // visible en Inspector para debug

    public bool IsActive => isActive;

    private void Awake()
    {
        // Busca primero en el mismo GO, luego en el padre
        staminaMgr = GetComponent<PlayerStamina>() ?? GetComponentInParent<PlayerStamina>();

        if (shieldObject != null) shieldObject.SetActive(false);

        if (debugLogs)
        {
            string backend = staminaMgr ? "PlayerStaminaManager"                            
                                        : "NONE";
            Debug.Log($"[Shield] Awake. shieldObject={(shieldObject ? shieldObject.name : "null")} | staminaBackend={backend}");
        }
    }

    public void TryUseShield()
    {
        // En modo HOLD, si ya está activo no re-activamos
        if (holdToBlock && isActive)
        {
            if (debugLogs) Debug.Log("[Shield] Ya activo (hold)");
            return;
        }

        // Cooldown
        if (Time.time < lastShieldUseTime + cooldownDuration)
        {
            if (debugLogs) Debug.Log("[Shield] En cooldown");
            return;
        }

        // Stamina
        if (!CanUseStamina(staminaCost))
        {
            if (debugLogs) Debug.Log("[Shield] Stamina insuficiente o backend no encontrado");
            return;
        }

        // En modo TOGGLE: si está activo y recibimos input -> desactivar
        if (!holdToBlock && isActive)
        {
            if (debugLogs) Debug.Log("[Shield] Toggle -> Desactivar");
            DeactivateShield();
            return;
        }

        ActivateShield();
    }

    private void ActivateShield()
    {
        isActive = true;
        lastShieldUseTime = Time.time;
        SpendStamina(staminaCost);

        if (shieldObject) shieldObject.SetActive(true);
        OnShieldActivated?.Invoke();

        if (debugLogs) Debug.Log("[Shield] ACTIVADO");
    }

    public void DeactivateShield()
    {
        if (!isActive) return;

        isActive = false;
        if (shieldObject) shieldObject.SetActive(false);
        OnShieldDeactivated?.Invoke();

        if (debugLogs) Debug.Log("[Shield] DESACTIVADO");
    }

    private void Update()
    {
        // En HOLD, si suelta el botón, bajamos el escudo
        if (holdToBlock && isActive && (PlayerInputs.Instance == null || !PlayerInputs.Instance.Shield()))
            DeactivateShield();
    }

    // ---------- Helpers: abstraen ambos backends de stamina ----------
    private bool CanUseStamina(float amount)
    {
        if (staminaMgr) return staminaMgr.CanUse(amount);
        return false;
    }

    private void SpendStamina(float amount)
    {
        if (staminaMgr) { staminaMgr.Use(amount); return; }
    }
}
