using UnityEngine;

public class PlayerStateCook<T> : State<T>
{
    private PlayerModel playerModel;
    private PlayerView playerView;

    private bool lastDishState;

    private T inputToIdle;


    public PlayerStateCook(T inputToIdle, PlayerModel playerModel, PlayerView playerView)
    {
        this.inputToIdle = inputToIdle;
        this.playerModel = playerModel;
        this.playerView = playerView;
    }

    public override void Enter()
    {
        base.Enter();
        //Debug.Log("Cook");

        PlayerView.OnEnterInCookMode?.Invoke();

        playerModel.Rb.velocity = Vector3.zero;
        playerModel.CapsuleCollider.material = null;

        lastDishState = playerView.Dish.gameObject.activeSelf;

        playerView.ShowOrHideDish(false);
    }

    public override void Execute()
    {
        base.Execute();

        if (PlayerInputs.Instance.InteractPress())
        {
            Fsm.TransitionTo(inputToIdle);
        }
    }

    public override void Exit()
    {
        PlayerView.OnExitInCookMode?.Invoke();
        playerView.ShowOrHideDish(lastDishState);
        playerModel.IsCooking = false;

        playerModel.CapsuleCollider.material = playerModel.PhysicsMaterial;
    }
}
