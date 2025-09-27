using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class PlayerKnockBack : MonoBehaviour
{
    [Header("Knockback Settings")]
    [Tooltip("Fuerza base del empujón cuando RECIBE daño sin bloquear.")]
    [SerializeField] private float baseForce = 5f;
    [Tooltip("Fuerza del empujón cuando BLOQUEA con el escudo.")]
    [SerializeField] private float blockedForce = 2f;
    [Tooltip("Levantamiento vertical para que se sienta el impacto.")]
    [SerializeField] private float verticalLift = 0.2f;


    private Rigidbody rb;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }


    /// <summary>
    /// Aplica un empujón al jugador.
    /// </summary>
    /// <param name="dir">Dirección horizontal, normalizada, desde el atacante hacia el jugador.</param>
    /// <param name="blocked">Si true, usa la fuerza reducida de bloqueo.</param>
    public void ApplyKnockback(Vector3 dir, bool blocked)
    {
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.y = 0f;
        float force = blocked ? blockedForce : baseForce;
        Vector3 impulse = dir.normalized * force + Vector3.up * verticalLift;
        rb.AddForce(impulse, ForceMode.Impulse);
    }
}