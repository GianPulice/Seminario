using UnityEngine;

public class ClientStateWaitingForChair<T> : State<T>
{
    private ClientModel clientModel;
    private ClientView clientView;

    private ClientStateLeave<T> clientStateLeave;

    private Transform auxiliarWaitingChairPosition;

    private float waitingForChairTime = 0f;

    private bool isInWaitingChairPosition = false;


    public ClientStateWaitingForChair(ClientModel clientModel, ClientView clientView, ClientStateLeave<T> clientStateLeave)
    {
        this.clientModel = clientModel;
        this.clientView = clientView;
        this.clientStateLeave = clientStateLeave;
    }


    public override void Enter()
    {
        base.Enter();
        Debug.Log("WaitingForChair");

        auxiliarWaitingChairPosition = clientModel.ClientManager.GetAvailableWaitingChairsPositions(clientModel);
        clientModel.MoveToTarget(auxiliarWaitingChairPosition.position);
        clientView.ExecuteAnimParameterName("Walk");
        clientView.SetSpriteTypeName("SpriteGoChair");
    }

    public override void Execute()
    {
        base.Execute();

        if (clientModel.CurrentTable == null)
        {
            clientModel.CurrentTable = TablesManager.Instance.GetRandomAvailableTableForClient();
        }

        if (Vector3.Distance(clientModel.transform.position, auxiliarWaitingChairPosition.position) <= 2f && !isInWaitingChairPosition)
        {
            isInWaitingChairPosition = true;
            clientView.ExecuteAnimParameterName("WaitingForChair");
            clientView.SetSpriteTypeName("SpriteHungry");
        }

        waitingForChairTime += Time.deltaTime;

        if (waitingForChairTime >= clientModel.ClientData.MaxTimeWaitingForChair)
        {
            clientStateLeave.CanLeave = true;
            return;
        }
    }

    public override void Exit()
    {
        base.Exit();

        auxiliarWaitingChairPosition = null;
        waitingForChairTime = 0f;
        isInWaitingChairPosition = false;
        clientModel.ClientManager.ReleaseWaitingChair(clientModel);
    }
}
