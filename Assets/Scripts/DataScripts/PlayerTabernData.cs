using UnityEngine;

[CreateAssetMenu(fileName = "PlayerTabernData", menuName = "ScriptableObjects/Tabern/Create New PlayerTabernData")]
public class PlayerTabernData : ScriptableObject
{
    [Header("Variables Movimiento")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float groundDrag;
    [SerializeField] private float groundForceMult;

    [Header("Variables Aereo")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    [SerializeField] private float playerHeight;

    [Header("Jump Gravity")]
    [SerializeField] private float jumpGravityMult;
    [SerializeField] private float fallGravityMult;

    [Header("Slope")]
    [SerializeField] private float maxSlopeAngle;
    [SerializeField] private float slopeForceMult;
    [SerializeField] private float slopeStickForce;

    public float WalkSpeed { get => walkSpeed; }
    public float RunSpeed { get => runSpeed; }
    public float AirMultiplier { get => airMultiplier; }
    public float GroundDrag { get => groundDrag; }
    public float JumpForce { get => jumpForce; }
    public float JumpCooldown { get => jumpCooldown; }
    public float FallGravityMult { get => fallGravityMult; }
    public float JumpGravityMult { get => jumpGravityMult; }
    public float PlayerHeight { get => playerHeight; }
    public float MaxSlopeAngle { get => maxSlopeAngle; }
    public float SlopeForceMult { get => slopeForceMult; }
    public float SlopeStickForce { get => slopeStickForce; }
    public float GroundForceMult { get => groundForceMult; }

}
