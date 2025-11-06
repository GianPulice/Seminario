using UnityEngine;

public class ClientStateEating<T> : State<T>
{
    private ClientModel clientModel;
    private ClientView clientView;

    private ClientStateLeave<T> clientStateLeave;

    private float eatingTime = 0f;

    private bool isEating = false;

    public bool IsEating { get => isEating; set => isEating = value; }


    public ClientStateEating(ClientModel clientModel, ClientView clientView, ClientStateLeave<T> clientStateLeave)
    {
        this.clientModel = clientModel;
        this.clientView = clientView;
        this.clientStateLeave = clientStateLeave;
    }


    public override void Enter()
    {
        base.Enter();
        Debug.Log("Eating");

        clientView.SetSpriteTypeName("SpriteEating");
    }

    public override void Execute()
    {
        base.Execute();

        eatingTime += Time.deltaTime;

        if (eatingTime >= clientModel.ClientData.MaxTimeEating)
        {
            isEating = false;
            clientStateLeave.CanLeave = true;
            eatingTime = 0f;
            return;
        }
    }

    public override void Exit()
    {
        base.Exit();

        isEating = false;
        clientView.Anim.transform.position += Vector3.down * 0.38f;
        eatingTime = 0f;
    }
}
