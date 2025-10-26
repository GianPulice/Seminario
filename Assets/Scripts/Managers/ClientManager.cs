using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    [SerializeField] private ClientManagerData clientManagerData;

    [SerializeField] private Transform spawnPosition, outsidePosition;
    [SerializeField] private List<WaitingChairPosition> waitingChairsPositions;

    [SerializeField] private AbstractFactory clientAbstractFactory;
    [SerializeField] private List<ObjectPooler> clientPools;
    [SerializeField] private List<FoodTypeSpritePair> foodSpritePairs;

    private Dictionary<ClientType, ObjectPooler> clientPoolDictionary = new();
    private Dictionary<FoodType, Sprite> foodSpriteDict = new();

    private float spawnTime = 0f;

    private bool isTabernOpen = false;

    [SerializeField] private bool spawnDifferentTypeOfClients;
    [SerializeField] private bool spawnTheSameClient;

    public Transform SpawnPosition { get => spawnPosition; }
    public Transform OutsidePosition { get => outsidePosition; }


    void Awake()
    {
        SuscribeToUpdateManagerEvent();
        SuscribeToOpenTabernButtonEvent();
        InitializeClientPoolDictionary();
        InitializeFoodSpriteDictionary();
    }

    // Simulacion de Update
    void UpdateClientManager()
    {
        SpawnClients();
    }

    void OnDestroy()
    {
        UnsuscribeToUpdateManagerEvent();
        UnsuscribeToOpenTabernButtonEvent();
    }


    public Transform GetAvailableWaitingChairsPositions(ClientModel client)
    {
        foreach (var chair in waitingChairsPositions)
        {
            if (!chair.IsOccupied)
            {
                chair.IsOccupied = true;
                chair.CurrentClient = client;
                return chair.Position;
            }
        }

        return null;
    }

    public void ReleaseWaitingChair(ClientModel client)
    {
        foreach (var chair in waitingChairsPositions)
        {
            if (chair.CurrentClient == client)
            {
                chair.IsOccupied = false;
                chair.CurrentClient = null;
                break;
            }
        }

        ReorderClientsInQueue();
    }

    public Sprite GetSpriteForRandomFood(FoodType foodType)
    {
        foodSpriteDict.TryGetValue(foodType, out Sprite sprite);
        return sprite;
    }

    public void ReturnObjectToPool(ClientType clientType, ClientModel currentClient)
    {
        if (clientPoolDictionary.ContainsKey(clientType))
        {
            clientPoolDictionary[clientType].ReturnObjectToPool(currentClient);
        }
    }

    public void SetParentToHisPoolGameObject(ClientType clientType, ClientModel currentClient)
    {
        if (clientPoolDictionary.TryGetValue(clientType, out ObjectPooler pooler))
        {
            currentClient.transform.SetParent(pooler.transform);
        }
    }


    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdateClientManager;
    }

    private void UnsuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate -= UpdateClientManager;
    }

    private void SuscribeToOpenTabernButtonEvent()
    {
        AdministratingManagerUI.OnStartTabern += SetIsTabernOpen;
    }

    private void UnsuscribeToOpenTabernButtonEvent()
    {
        AdministratingManagerUI.OnStartTabern -= SetIsTabernOpen;
    }

    private void SpawnClients()
    {
        if (isTabernOpen && GetIfAllWaitingChairPositionsAreOccupied())
        {
            if (spawnTheSameClient)
            {
                GetTheSameClientFromPool();
            }

            else if (spawnDifferentTypeOfClients)
            {
                GetClientRandomFromPool();
            }
        }
    }

    private void SetIsTabernOpen()
    {
        isTabernOpen = true;
    }

    private void GetClientRandomFromPool()
    {
        spawnTime += Time.deltaTime;

        if (spawnTime >= clientManagerData.TimeToWaitForSpawnNewClient)
        {
            int randomIndex = UnityEngine.Random.Range(0, clientPools.Count);
            string prefabName = clientPools[randomIndex].Prefab.name;
            clientAbstractFactory.CreateObject(prefabName);
            
            spawnTime = 0f;
        }
    }

    private void GetTheSameClientFromPool()
    {
        spawnTime += Time.deltaTime;

        if (spawnTime > clientManagerData.TimeToWaitForSpawnNewClient)
        {
            clientAbstractFactory.CreateObject("ClientGoblin");

            spawnTime = 0f;
        }
    }

    private void ReorderClientsInQueue()
    {
        waitingChairsPositions.Sort((a, b) => a.PriorityIndex.CompareTo(b.PriorityIndex));

        // Recorrer las sillas en orden
        for (int i = 0; i < waitingChairsPositions.Count; i++)
        {
            // Si esta silla está libre y hay alguien detrás
            if (!waitingChairsPositions[i].IsOccupied)
            {
                for (int j = i + 1; j < waitingChairsPositions.Count; j++)
                {
                    var clientBehind = waitingChairsPositions[j].CurrentClient;
                    if (clientBehind != null)
                    {
                        // Mover cliente al hueco
                        waitingChairsPositions[i].CurrentClient = clientBehind;
                        waitingChairsPositions[i].IsOccupied = true;

                        waitingChairsPositions[j].CurrentClient = null;
                        waitingChairsPositions[j].IsOccupied = false;

                        clientBehind.MoveToTarget(waitingChairsPositions[i].Position.position);
                        break; // solo mueve uno por iteración
                    }
                }
            }
        }
    }

    // Devuelve true si hay sillas que no estan ocupadas, sino devuelve false
    private bool GetIfAllWaitingChairPositionsAreOccupied()
    {
        foreach (var chair in waitingChairsPositions)
        {
            if (!chair.IsOccupied)
            {
                return true;
            }
        }

        return false;
    }

    private void InitializeClientPoolDictionary()
    {
        for (int i = 0; i < clientPools.Count; i++)
        {
            if (Enum.IsDefined(typeof(ClientType), i))
            {
                ClientType foodType = (ClientType)i;
                clientPoolDictionary[foodType] = clientPools[i];
            }
        }
    }

    private void InitializeFoodSpriteDictionary()
    {
        foodSpriteDict.Clear();

        foreach (var pair in foodSpritePairs)
        {
            if (!foodSpriteDict.ContainsKey(pair.FoodType))
            {
                foodSpriteDict.Add(pair.FoodType, pair.Sprite);
            }
        }
    }
}

[Serializable]
public class FoodTypeSpritePair
{
    [SerializeField] private FoodType foodType;
    [SerializeField] private Sprite sprite;

    public FoodType FoodType { get => foodType; }
    public Sprite Sprite { get => sprite; }
}

[Serializable]
public class WaitingChairPosition
{
    [SerializeField] private Transform position;
    [SerializeField] private int priorityIndex;

    private ClientModel currentClient;
    private bool isOccupied = false;

    public Transform Position { get => position; }
    public int PriorityIndex { get => priorityIndex; }  

    public ClientModel CurrentClient { get => currentClient; set => currentClient = value; }
    public bool IsOccupied { get => isOccupied; set => isOccupied = value; }
}