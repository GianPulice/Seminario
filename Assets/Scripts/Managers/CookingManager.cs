using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class CookingManager : MonoBehaviour
{
    [SerializeField] private AbstractFactory foodAbstractFactory;
    [SerializeField] private List<ObjectPooler> foodPools;

    [Header("Positions")]
    // Para las posiciones de las sarten
    [SerializeField] private List<Transform> stovesPositions;
    private Transform currentStove;
    private Queue<Transform> availableStovesPositions = new Queue<Transform>();
    private HashSet<Transform> occupiedStovesPositions = new HashSet<Transform>();

    // Para las posiciones en la bandeja del player
    [SerializeField] private List<Transform> dishPositions;
    private Queue<Transform> availableDishPositions = new Queue<Transform>();
    private HashSet<Transform> occupiedDishPositions = new HashSet<Transform>();

    private Dictionary<FoodType, ObjectPooler> foodPoolDictionary = new Dictionary<FoodType, ObjectPooler>();

    public Transform CurrentStove { get => currentStove; }
    public Queue<Transform> AvailableDishPositions { get => availableDishPositions; }


    void Awake()
    {
        SuscribeToCookingManagerUIEvent();
        EnqueueStovesPositions();
        EnqueueDishPositions();
        InitializeFoodPoolDictionary();
    }

    void Start()
    {
        StartCoroutine(PlayTabernMusic());
    }

    void OnDestroy()
    {
        UnsuscribeToCookingManagerUIEvent();
    }


    public void ReturnObjectToPool(FoodType foodType, Food currentFood)
    {
        if (foodPoolDictionary.ContainsKey(foodType))
        {
            foodPoolDictionary[foodType].ReturnObjectToPool(currentFood);
        }
    }

    public void ReleaseStovePosition(Transform stovePosition)
    {
        if (occupiedStovesPositions.Contains(stovePosition))
        {
            occupiedStovesPositions.Remove(stovePosition);
            availableStovesPositions.Enqueue(stovePosition);
        }
    }

    public void ReleaseDishPosition(Transform dishPosition)
    {
        if (occupiedDishPositions.Contains(dishPosition))
        {
            occupiedDishPositions.Remove(dishPosition);
            availableDishPositions.Enqueue(dishPosition);
        }
    }

    public Transform MoveFoodToDish(Food currentFood)
    {
        Transform targetPosition = null;

        while (availableDishPositions.Count > 0)
        {
            targetPosition = availableDishPositions.Dequeue();

            if (occupiedDishPositions.Contains(targetPosition))
            {
                availableDishPositions.Enqueue(targetPosition);
                continue;
            }

            occupiedDishPositions.Add(targetPosition);
            break;
        }

        if (targetPosition != null)
        {
            currentFood.transform.position = targetPosition.position;
            currentFood.transform.SetParent(targetPosition);
        }

        return targetPosition;
    }


    private void SuscribeToCookingManagerUIEvent()
    {
        CookingManagerUI.OnButtonSetFood += GetFood;
    }

    private void UnsuscribeToCookingManagerUIEvent()
    {
        CookingManagerUI.OnButtonSetFood -= GetFood;
    }

    private void GetFood(string prefabFoodName)
    {
        if (Enum.TryParse(prefabFoodName, out FoodType foodType))
        {
            if (IngredientInventoryManager.Instance.TryCraftFood(foodType))
            {
                currentStove = GetNextAvailableStove();

                if (currentStove != null)
                {
                    foodAbstractFactory.CreateObject(prefabFoodName, currentStove, new Vector3(0, 0.2f, 0));
                }
            }
        }
    }

    private Transform GetNextAvailableStove()
    {
        Transform targetPosition = null;

        if (availableStovesPositions.Count == 0) return null;

        while (availableStovesPositions.Count > 0)
        {
            targetPosition = availableStovesPositions.Dequeue();

            if (occupiedStovesPositions.Contains(targetPosition))
            {
                availableStovesPositions.Enqueue(targetPosition);
                continue;
            }

            occupiedStovesPositions.Add(targetPosition);
            break;
        }

        return targetPosition;
    }

    private void EnqueueStovesPositions()
    {
        foreach (var position in stovesPositions)
        {
            availableStovesPositions.Enqueue(position);
        }
    }

    private void EnqueueDishPositions()
    {
        foreach (var position in dishPositions)
        {
            availableDishPositions.Enqueue(position);
        }
    }

    private void InitializeFoodPoolDictionary()
    {
        for (int i = 0; i < foodPools.Count; i++)
        {
            GameObject prefab = foodPools[i].Prefab;

            if (Enum.TryParse(prefab.name, out FoodType foodType))
            {
                foodPoolDictionary[foodType] = foodPools[i];
            }
        }
    }

    private IEnumerator PlayTabernMusic()
    {
        yield return new WaitUntil(() => AudioManager.Instance != null);

        StartCoroutine(AudioManager.Instance.PlayMusic("TabernBGM"));
    }
}
