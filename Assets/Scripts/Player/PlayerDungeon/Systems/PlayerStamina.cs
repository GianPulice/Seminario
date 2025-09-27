using System;
using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regenRate = 10f;
    [SerializeField] private float regenDelay = 1f;

    private float currentStamina;
    private float lastUseTime;
    private bool suppressRegen;

    [SerializeField] private bool debugLogs = true;
    private bool exhaustedLogged = false;      // para no spamear el mensaje de “llegó a 0”



    public float Current => currentStamina;
    public float Max => maxStamina;
    public bool IsExhausted => currentStamina <= 0f;

    public event Action<float, float> OnStaminaChanged;

    private void Awake()
    {
        currentStamina = maxStamina;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    private void Update()
    {
        TryRegenerate();
    }

    public bool CanUse(float amount)
    {
        return currentStamina >= amount;
    }

    public void Use(float amount)
    {
        if (amount <= 0f || IsExhausted) return;

        currentStamina = Mathf.Clamp(currentStamina - amount, 0f, maxStamina);
        lastUseTime = Time.time;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);

        if (debugLogs)
            Debug.Log($"[Stamina] -{amount:F2} -> {currentStamina:F2}/{maxStamina:F2}");

        if (currentStamina <= 0f && !exhaustedLogged)
        {
            exhaustedLogged = true;
            Debug.Log("[Stamina] LLEGÓ A 0: no puedes atacar ni usar escudo (solo caminar).");
        }
    }

    public void ForceZero()
    {
        currentStamina = 0f;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    private void TryRegenerate()
    {
        float before = currentStamina;

        if (currentStamina >= maxStamina) return;
        if (Time.time < lastUseTime + regenDelay) return;

        if (suppressRegen) return;
        currentStamina += regenRate * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);

        float delta = currentStamina - before;
        if (debugLogs && delta > 0f)
            Debug.Log($"[Stamina] +{delta:F2} (regen) -> {currentStamina:F2}/{maxStamina:F2}");

        // si subió de 0, habilitamos de nuevo el aviso para la próxima vez que se agote
        if (currentStamina > 0f) exhaustedLogged = false;
    }

    public void RefillFull()
    {
        currentStamina = maxStamina;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    public void SetMaxStamina(float newMax)
    {
        maxStamina = newMax;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
    public void SetRegenSuppressed(bool value)
    {
        suppressRegen = value;
    }
}
