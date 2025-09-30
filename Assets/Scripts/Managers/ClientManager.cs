using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    [SerializeField] private ClientManagerData clientManagerData;

    [SerializeField] private Transform spawnPosition, outsidePosition;

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

    /*private System.Collections.IEnumerator InitializeRandomClient()
    {
        yield return new WaitUntil(() => System.Linq.Enumerable.All(clientPools, p => p != null && p.Prefab != null));
        
        int randomIndex = UnityEngine.Random.Range(0, clientPools.Count);
        string prefabName = clientPools[randomIndex].Prefab.name;
        clientAbstractFactory.CreateObject(prefabName);
    }*/

    private void SpawnClients()
    {
        if (isTabernOpen)
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