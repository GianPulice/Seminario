using System.Collections;
using UnityEngine;

public class ClientStateLeave<T> : State<T>
{
    private ClientModel clientModel;
    private ClientView clientView;
    private ClientController clientController;
    private Transform newTransform;

    private bool canLeave = false;

    public bool CanLeave { get => canLeave; set => canLeave = value; }


    public ClientStateLeave(ClientController clientController, ClientModel clientModel, ClientView clientView, Transform newTransform)
    {
        this.clientController = clientController;
        this.clientModel = clientModel;
        this.clientView = clientView;
        this.newTransform = newTransform;
    }


    public override void Enter()
    {
        base.Enter();
        Debug.Log("Leave");

        // Quiere decir que entro al estado leave porque todas las mesas estaban ocupadas
        if (clientModel.CurrentTable == null)
        {
            clientView.ExecuteAnimParameterName("Walk");
            clientModel.MoveToTarget(newTransform.position);
            clientModel.LookAt(newTransform.position, clientView.Anim.transform);
        }

        // Sino quiere decir que se fue de leave porque estaba ya en una mesa
        else
        {
            clientView.ExecuteAnimParameterName("StandUp");
            clientView.Anim.transform.position += Vector3.up * 0.35f;
            clientView.StartCoroutine(WalkAnimationAfterExitTime());
        }

        CheckIfFoodIsCorrect();

        clientModel.StartCoroutine(FreeCurrentTableAfterSeconds());
    }

    public override void Execute()
    {
        base.Execute();
    }

    public override void Exit()
    {
        base.Exit();

        canLeave = false;
        ClientManager.Instance.ClientsInsideTabern.Remove(clientModel.gameObject);
    }


    private IEnumerator WalkAnimationAfterExitTime()
    {
        yield return new WaitForSeconds(3f);// Tiempo que tarda en pararse por completo 

        clientView.Anim.transform.position += Vector3.down * 0.35f;
        clientModel.MoveToTarget(newTransform.position);
        clientModel.LookAt(newTransform.position, clientView.Anim.transform);
        clientView.ExecuteAnimParameterName("Walk");

        yield return clientController.StartCoroutine(EnabledTriggerFromTable());
    }

    private IEnumerator EnabledTriggerFromTable()
    {
        /// Ajustar tiempo segun sea necesario
        yield return new WaitForSeconds(2f);

        clientController.OnCollisionEnterWithTriggerChair = false;
    }

    private IEnumerator FreeCurrentTableAfterSeconds()
    {
        yield return new WaitForSeconds(ClientManager.Instance.ClientManagerData.DelayToFreeTableWhenClientLeaveTable);

        if (clientModel.CurrentTable == null) yield break; // Cortar el metodo si la mesa es null, quiere decir que se fue porque se quedo esperando

        clientModel.CurrentTable.SetNavMeshObstacles(true);
        clientModel.CurrentTable = TablesManager.Instance.FreeTable(clientModel.CurrentTable);
    }

    private void CheckIfFoodIsCorrect()
    {
        if (clientModel.CurrentTable != null)
        {
            if (clientModel.CurrentTable.CurrentFoods != null && clientModel.CurrentTable.CurrentFoods.Count > 0)
            {
                // Si la comida no esta en estado correcto y es la que pidio sumar el minimo
                if (clientModel.CurrentTable.CurrentFoods[0].CurrentCookingState != CookingStates.Cooked && clientModel.CurrentTable.CurrentFoods[0].FoodType == clientView.CurrentSelectedFood)
                {
                    AudioManager.Instance.PlaySFX("ClientHungry");
                    clientView.SetSpriteTypeName("SpriteHungry");
                    TabernManager.Instance.BrokenThingsAmount += TabernManager.Instance.TabernManagerData.CostPerBrokenThings;
                    MoneyManager.Instance.AddMoney(ClientManager.Instance.ClientManagerData.MinimumPaymentAmount);
                }

                // Si la comida no es la que pidio
                else if (clientModel.CurrentTable.CurrentFoods[0].FoodType != clientView.CurrentSelectedFood)
                {
                    AudioManager.Instance.PlaySFX("ClientHungry");
                    clientView.SetSpriteTypeName("SpriteHungry");
                    TabernManager.Instance.BrokenThingsAmount += TabernManager.Instance.TabernManagerData.CostPerBrokenThings;
                }
                else if (clientModel.CurrentTable.CurrentFoods[0].FoodType == clientView.CurrentSelectedFood)
                {
                    AudioManager.Instance.PlaySFX("ClientHappy");
                    clientView.SetSpriteTypeName("SpriteHappy");
                    int paymentAmout = ClientManager.Instance.ClientManagerData.GetPayment(clientModel.CurrentTable.CurrentFoods[0].FoodType);
                    MoneyManager.Instance.AddMoney(paymentAmout);

                    // Solamente dar propina si la mesa no estaba sucia cuando se sento
                    if (!clientModel.WasTableDirtyWhenSeated)
                    {
                        clientModel.StartCoroutine(AddGratutityAfterSomeSeconeds(paymentAmout));
                    }
                }

                clientModel.ReturnFoodFromTableToPool();
                clientModel.CurrentTable.SetDirty(true);
            }

            // Verifica que no le hayan servido ninguna comida en el plato porque no le tomaron el pedido o no llegaron a entregarsela
            else
            {
                AudioManager.Instance.PlaySFX("ClientHungry");
                clientView.SetSpriteTypeName("SpriteHungry");
                TabernManager.Instance.BrokenThingsAmount += TabernManager.Instance.TabernManagerData.CostPerBrokenThings;
                //MoneyManager.Instance.SubMoney(GratuityManager.Instance.GratuityManagerData.MissedClientCost);
            }
        }

        // Si la mesa es null ejecuta este bloque, quiere decir que todas las mesas estaban ocupadas y se quedo esperando afuera
        else
        {
            AudioManager.Instance.PlaySFX("ClientWasWaitingOutsideTooMuchTime");
            TabernManager.Instance.BrokenThingsAmount += TabernManager.Instance.TabernManagerData.CostPerBrokenThings;
            //MoneyManager.Instance.SubMoney(GratuityManager.Instance.GratuityManagerData.MissedClientCost);
        }
    }

    private IEnumerator AddGratutityAfterSomeSeconeds(int paymentAmout)
    {
        yield return new WaitForSeconds(3);

        GratuityManager.Instance.TryGiveGratuity(paymentAmout, clientModel.ClientData);
    }
}
