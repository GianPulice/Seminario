using UnityEngine;

public class PlayerStateRun<T> : State<T>
{
    private PlayerModel playerModel;

    private T inputToIdle;
    private T inputToWalk;
    private T inputToJump;
    private T inputToCook;
    private T inputToAdmin;


    public PlayerStateRun(T inputToIdle, T inputToWalk, T inputToJump, T inputToCook, T inputToAdmin, PlayerModel playerModel)
    {
        this.inputToIdle = inputToIdle;
        this.inputToWalk = inputToWalk;
        this.inputToJump = inputToJump;
        this.inputToCook = inputToCook;
        this.inputToAdmin = inputToAdmin;
        this.playerModel = playerModel;
    }

    public override void Enter()
    {
        base.Enter();
        //Debug.Log("Run");

        playerModel.Speed = playerModel.PlayerTabernData.RunSpeed;

        //AudioManager.Instance.PlayLoopSFX("PlayerFootSteps");
    }

    public override void Execute()
    {
        base.Execute();

        if (PlayerInputs.Instance.GetMoveAxis() == Vector2.zero)
        {
            Fsm.TransitionTo(inputToIdle);
        }

        if (PlayerInputs.Instance.StopRun())
        {
            Fsm.TransitionTo(inputToWalk);
        }

        if (PlayerInputs.Instance.Jump() && playerModel.IsGrounded && playerModel.ReadyToJump && !playerModel.IsInTeleportPanel && !playerModel.IsInTrashPanel && !playerModel.IsInTutorial)
        {
            Fsm.TransitionTo(inputToJump);
        }

        if (playerModel.IsCooking)
        {
            Fsm.TransitionTo(inputToCook);
        }

        if (playerModel.IsAdministrating)
        {
            Fsm.TransitionTo(inputToAdmin);
        }
    }

    public override void Exit()
    {
        base.Exit();

        //AudioManager.Instance.StopLoopSFX("PlayerFootSteps");
    }
}
