using UnityEngine;

public class PlayerStateWalk<T> : State<T>
{
    private PlayerModel playerModel;

    private T inputToIdle;
    private T inputToRun;
    private T inputToJump;
    private T inputToCook;
    private T inputToAdmin;

    public PlayerStateWalk(T inputToIdle, T inputToRun, T inputToJump, T inputToCook, T inputToAdmin, PlayerModel playerModel)
    {
        this.inputToIdle = inputToIdle;
        this.inputToRun = inputToRun;
        this.inputToJump = inputToJump;
        this.inputToCook = inputToCook;
        this.inputToAdmin = inputToAdmin;
        this.playerModel = playerModel;
    }

    public override void Enter()
    {
        base.Enter();
        //Debug.Log("Walk");

        playerModel.Speed = playerModel.PlayerTabernData.WalkSpeed;

        if (playerModel.IsGrounded)
        {
            AudioManager.Instance.PlayLoopSFX("PlayerFootSteps");
        }
    }

    public override void Execute()
    {
        base.Execute();

        if (!playerModel.IsGrounded)
        {
            AudioManager.Instance.StopLoopSFX("PlayerFootSteps");
        }

        else
        {
            AudioSource playerFootSteps = AudioManager.Instance.GetActiveSFX("PlayerFootSteps");

            if (playerFootSteps == null || !playerFootSteps.isPlaying)
            {
                AudioManager.Instance.PlayLoopSFX("PlayerFootSteps");
            }
        }

        if (PlayerInputs.Instance.GetMoveAxis() == Vector2.zero || playerModel.IsInTutorial)
        {
            Fsm.TransitionTo(inputToIdle);
        }

        if (PlayerInputs.Instance.Run())
        {
            Fsm.TransitionTo(inputToRun);
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

        AudioManager.Instance.StopLoopSFX("PlayerFootSteps");
    }
}