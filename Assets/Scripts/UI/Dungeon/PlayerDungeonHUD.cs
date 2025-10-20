using System;


public class PlayerDungeonHUD
{
    public static Action<float, float> OnHealthChanged; // currentHP, maxHP
    public static Action OnPlayerDeath;

    public static Action<float, float> OnStaminaChanged;

    public static Action<int> OnLayerChanged;

    public static Action<string> OnShowTeleportConfirm;
    public static Action OnHideTeleportConfirm;

    public static Action OnLockPlayerInput;
    public static Action OnUnlockPlayerInput;
}
