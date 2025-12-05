using UnityEngine;

public class PlayerStateIdle<T> : State<T>
{
    private PlayerModel playerModel;

    private T inputToWalk;
    private T inputToJump;
    private T inputToCook;
    private T inputToAdmin;


    public PlayerStateIdle(T inputToWalk, T inputToJump, T inputToCook, T inputToAdmin, PlayerModel playerModel)
    {
        this.inputToWalk = inputToWalk;
        this.inputToJump = inputToJump;
        this.inputToCook = inputToCook;
        this.inputToAdmin = inputToAdmin;
        this.playerModel = playerModel;
    }

    public override void Enter()
    {
        base.Enter();
        //Debug.Log("Idle");
    }

    public override void Execute()
    {
        base.Execute();

        if (PlayerInputs.Instance.GetMoveAxis() != Vector2.zero)
        {
            Fsm.TransitionTo(inputToWalk);
        }

        if (PlayerInputs.Instance.Jump() && playerModel.IsGrounded && playerModel.ReadyToJump && !playerModel.IsInTeleportPanel && !playerModel.IsInTrashPanel && !playerModel.IsInTutorial && !playerModel.IsInResumeDayPanel)
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
}
