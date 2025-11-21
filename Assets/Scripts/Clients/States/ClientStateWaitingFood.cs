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

        CheckIfTableIsDirty();
        clientModel.LookAt(clientModel.CurrentTable.DishPosition.position, clientView.Anim.transform);
        clientView.ExecuteAnimParameterName("Sit");
        clientView.StartCoroutine(DuringSitAnimationAfterExitTime());
        clientModel.transform.SetParent(clientModel.CurrentTable.ChairPosition);
    }

    public override void Execute()
    {
        base.Execute();

        // Ejecutar todo el tiempo que mire a la silla
        clientModel.LookAt(clientModel.CurrentTable.DishPosition.position, clientView.Anim.transform);
        ExecuteTimers();
        CheckIfFoodIsInDish();
    }

    public override void Exit()
    {
        base.Exit();

        // Solamente mover la posicion hacia abajo cuando sale del estado si paso al estado leave, sino nada
        if (!clientStateEating.IsEating)
        {
            float scaleY = clientView.Anim.transform.lossyScale.y;
            clientView.Anim.transform.position += Vector3.down * (0.38f * scaleY);
            //clientView.Anim.transform.position += Vector3.down * 0.38f;
        }

        canExecuteTimers = false;
        waitingToBeAttendedTime = 0f;
        waitingFoodTime = 0f;
        clientModel.ClientManager.SetParentToHisPoolGameObject(clientModel.ClientType, clientModel);
        OrdersManagerUI.Instance.RemoveOrder(clientModel.CurrentOrderDataUI);
    }


    private IEnumerator DuringSitAnimationAfterExitTime()
    {
        yield return new WaitForSeconds(2.06f);

        Vector3 directionToDish = clientModel.CurrentTable.DishPosition.position - clientModel.transform.position;
        directionToDish.y = 0f;
        directionToDish = directionToDish.normalized;
        float baseOffset = 1f;
        float scaleFactor = clientModel.transform.lossyScale.y;
        float forwardOffset = baseOffset * scaleFactor;
        clientModel.StartCoroutine(MoveForwardSmooth(directionToDish, forwardOffset, 1f));

        yield return new WaitForSeconds(2.06f); // Tiempo que tarda en sentarse por completo

        // Chequear el tema de transition duration en las settings de la transicion de "Sit" a "DuringSit"
        clientView.ExecuteAnimParameterName("DuringSit");
        float scaleY = clientView.Anim.transform.lossyScale.y;
        clientView.Anim.transform.position += Vector3.up * (0.38f * scaleY);
        //clientView.Anim.transform.position += Vector3.up * 0.38f;
        clientView.SetSpriteTypeName("SpriteWaitingToBeAttended");
        canExecuteTimers = true;
    }

    private IEnumerator MoveForwardSmooth(Vector3 direction, float distance, float duration)
    {
        Vector3 startPos = clientModel.transform.position;
        Vector3 endPos = startPos + (direction * distance);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Suavizado (opcional): acelera y desacelera
            t = Mathf.SmoothStep(0, 1, t);

            clientModel.transform.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        clientModel.transform.position = endPos; // asegurar posición final

        clientModel.StopVelocity();
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

                    if (waitingFoodTime >= clientModel.ClientData.MaxTimeWaitingFood + clientView.CurrentFoodCookingTime)
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

    private void CheckIfTableIsDirty()
    {
        clientModel.WasTableDirtyWhenSeated = clientModel.CurrentTable.IsDirty;
    }
}
