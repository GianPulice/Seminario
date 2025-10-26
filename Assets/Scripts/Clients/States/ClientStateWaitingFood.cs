using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientStateWaitingFood<T> : State<T>
{
    private ClientModel clientModel;
    private ClientView clientView;

    private ClientStateLeave<T> clientStateLeave;
    private ClientStateEating<T> clientStateEating;

    private bool canExecuteTimers = false;

    private float waitingToBeAttendedTime = 0f;
    private float waitingFoodTime = 0f;


    public ClientStateWaitingFood(ClientModel clientModel, ClientView clientView, ClientStateLeave<T> clientStateLeave, ClientStateEating<T> clientStateEating)
    {
        this.clientModel = clientModel;
        this.clientView = clientView;
        this.clientStateLeave = clientStateLeave;
        this.clientStateEating = clientStateEating;
    }


    public override void Enter()
    {
        base.Enter();
        Debug.Log("WaitingFood");

        clientModel.StopVelocity(); /// Recorda que esta linea de codigo se puede borrar y funciona igual / incluso mejor, porque el SetDestination automaticamente frena la velocidad cuando llega a un punto
        clientModel.LookAt(clientModel.CurrentTable.DishPosition.position, clientView.Anim.transform);
        clientView.ExecuteAnimParameterName("Sit");
        clientView.StartCoroutine(DuringSitAnimationAfterExitTime());
        clientModel.transform.SetParent(clientModel.CurrentTable.ChairPosition);
    }

    public override void Execute()
    {
        base.Execute();

        ExecuteTimers();
        CheckIfFoodIsInDish();
    }

    public override void Exit()
    {
        base.Exit();

        // Solamente mover la posicion hacia abajo cuando sale del estado si paso al estado leave, sino nada
        if (!clientStateEating.IsEating)
        {
            clientView.Anim.transform.position += Vector3.down * 0.38f;
        }

        waitingToBeAttendedTime = 0f;
        waitingFoodTime = 0f;
        clientModel.ClientManager.SetParentToHisPoolGameObject(clientModel.ClientType, clientModel);
        canExecuteTimers = false;
    }


    private IEnumerator DuringSitAnimationAfterExitTime()
    {
        yield return new WaitForSeconds(4.12f); // Tiempo que tarda en sentarse por completo
        
        clientView.ExecuteAnimParameterName("DuringSit");
        clientView.Anim.transform.position += Vector3.up * 0.38f;
        clientView.SetSpriteTypeName("SpriteWaitingToBeAttended");
        canExecuteTimers = true;
    }

    private void ExecuteTimers()
    {
        if (canExecuteTimers)
        {
            // Ejecuta el tiempo a ser atendido
            if (clientView.ReturnSpriteWaitingToBeAttendedIsActive())
            {
                waitingToBeAttendedTime += Time.deltaTime;

                if (waitingToBeAttendedTime >= clientModel.ClientData.MaxTimeWaitingToBeAttended)
                {
                    clientStateLeave.CanLeave = true;
                    waitingToBeAttendedTime = 0f;
                    canExecuteTimers = false;
                    return;
                }
            }

            // Ejecuta el tiempo a recibir el pedido
            else
            {
                if (!clientStateEating.IsEating)
                {
                    waitingFoodTime += Time.deltaTime;

                    if (waitingFoodTime >= clientModel.ClientData.MaxTimeWaitingFood)
                    {
                        clientStateLeave.CanLeave = true;
                        waitingFoodTime = 0f;
                        canExecuteTimers = false;
                        return;
                    }
                }
            }
        }
    }

    private void CheckIfFoodIsInDish()
    {
        List<string> expectedDishNames = new List<string>();
        List<string> servedDishNames = new List<string>();

        foreach (string food in clientView.OrderFoodNames)
        {
            expectedDishNames.Add(food + "(Clone)");
        }

        foreach (Transform dishSpot in clientModel.CurrentTable.DishPositions)
        {
            if (dishSpot.childCount > 0)
            {
                servedDishNames.Add(dishSpot.GetChild(0).name);
            }
        }

        if (servedDishNames.Count == 0)
        {
            return;
        }

        expectedDishNames.Sort();
        servedDishNames.Sort();

        bool dishesMatch = expectedDishNames.Count == servedDishNames.Count;

        if (dishesMatch)
        {
            for (int i = 0; i < expectedDishNames.Count; i++)
            {
                if (expectedDishNames[i] != servedDishNames[i])
                {
                    dishesMatch = false;
                    break;
                }
            }
        }

        if (!dishesMatch)
        {
            clientStateEating.IsEating = true;
        }

        else
        {
            clientStateEating.IsEating = true;
        }
    }
}
