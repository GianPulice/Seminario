using UnityEngine;

public enum DamageSourceType
{
    Unknown = 0,
    EnemyMelee = 1,
    EnemyProjectile = 2,
    Trap = 3,
    Environment = 4,
}

/// <summary>
/// Contexto ligero para pasar metadatos del golpe SIN romper IDamageable.
/// Los atacantes lo setean justo antes de llamar a TakeDamage y (opcional) lo limpian luego.
/// </summary>
public static class DamageContext
{
    [System.ThreadStatic] public static DamageSourceType Source;
    [System.ThreadStatic] public static Transform Attacker;
    [System.ThreadStatic] public static Vector3 HitPoint;
    /// <summary>
    /// Dirección desde el ATACANTE hacia el JUGADOR (para empujar al jugador alejándolo del atacante).
    /// </summary>
    [System.ThreadStatic] public static Vector3 HitDirection;

    public static void Set(DamageSourceType source, Transform attacker, Vector3 hitPoint, Vector3 hitDirection)
    {
        Source = source;
        Attacker = attacker;
        HitPoint = hitPoint;
        HitDirection = hitDirection;
    }

    public static void Clear()
    {
        Source = DamageSourceType.Unknown;
        Attacker = null;
        HitPoint = Vector3.zero;
        HitDirection = Vector3.zero;
    }
}
