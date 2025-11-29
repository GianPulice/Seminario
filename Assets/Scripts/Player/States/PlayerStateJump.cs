using System.Collections;
using UnityEngine;

public class PlayerStateJump<T> : State<T>
{
    private PlayerModel playerModel;

    private T inputToIdle;


    public PlayerStateJump(T inputToIdle, PlayerModel playerModel)
    {
        this.inputToIdle = inputToIdle;
        this.playerModel = playerModel;
    }

    public override void Enter()
    {
        base.Enter();
        //Debug.Log("Jump");

        AudioManager.Instance.PlayOneShotSFX("Jump");
        playerModel.JumpStart();
    }

    public override void Execute()
    {
        base.Execute();

        if (playerModel.IsGrounded)
        {
            Fsm.TransitionTo(inputToIdle);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
