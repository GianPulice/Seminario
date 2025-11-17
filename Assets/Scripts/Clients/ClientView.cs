using UnityEngine;
using System.Collections.Generic;

public class ClientView : MonoBehaviour
{
    /// <summary>
    /// Recordatorio: Buscar en la mesa el componente hijo Food y tomar el tiempo que tarda en cocinarse en ese instante de forma local
    /// </summary>

    [SerializeField] private ClientsFoodPreferencesData clientsFoodPreferencesData;

    private ClientModel clientModel;

    private PlayerController playerController;
    private ClientManager clientManager;
    private Table tablePlayerCollision; 

    private Animator anim;
    private Transform order; // GameObject padre de la UI
    private Transform orderBorder;
    private List<SpriteRenderer> spritesTypeList = new List<SpriteRenderer>();

    private List<string> orderFoodNames = new List<string>(); /// <summary>
    /// Modificar esto para que sea unicamente un string solo
    /// </summary>

    private Dictionary<string, SpriteRenderer> spriteTypeDict = new();

    private FoodType currentSelectedFood;

    private float currentFoodCookingTime;

    private bool canTakeOrder = false; // Se pone en true cuando nos acercamos a la mesa y no pidio nada todavia

    public Animator Anim { get => anim; }

    public List<string> OrderFoodNames { get => orderFoodNames; }

    public FoodType CurrentSelectedFood { get => currentSelectedFood; }

    public float CurrentFoodCookingTime { get => currentFoodCookingTime; }

    public bool CanTakeOrder { get => canTakeOrder; set => canTakeOrder = value; }


    void Awake()
    {
        GetComponents();
        SuscribeToPlayerControllerEvent();
    }

    void OnDestroy()
    {
        UnsuscribeToPlayerControllerEvent();
    }


    private void SuscribeToPlayerControllerEvent()
    {
        PlayerController.OnTakeOrder += InitializeRandomFoodUI;

        PlayerController.OnTableCollisionEnterForTakeOrder += SaveTable;
        PlayerController.OnTableCollisionExitForTakeOrder += ClearTable;
    }

    private void UnsuscribeToPlayerControllerEvent()
    {
        PlayerController.OnTakeOrder -= InitializeRandomFoodUI;

        PlayerController.OnTableCollisionEnterForTakeOrder -= SaveTable;
        PlayerController.OnTableCollisionExitForTakeOrder -= ClearTable;
    }


    public void ExecuteAnimParameterName(string animParameterName)
    {
        RestartAnimationsValues();
        anim.SetBool(animParameterName, true);
    }

    public void SetSpriteTypeName(string spriteTypeNameGameObjectInHierarchy)
    {
        DisableAllSpriteTypes();
        if (spriteTypeDict.TryGetValue(spriteTypeNameGameObjectInHierarchy, out var spriteRenderer))
        {
            spriteRenderer.gameObject.SetActive(true);
        }
    }

    public void DisableAllSpriteTypes()
    {
        foreach (var type in spritesTypeList)
        {
            type.gameObject.SetActive(false);
        }
    }

    public bool ReturnSpriteFoodIsActive() // Hace referencia al sprite que ya puede recibir comida
    {
        return spritesTypeList[0].gameObject.activeSelf;
    }

    public bool ReturnSpriteWaitingToBeAttendedIsActive()
    {
        return spritesTypeList[2].gameObject.activeSelf;
    }

    public void InitializeRandomFoodUI()
    {
        if (spritesTypeList[2].gameObject.activeSelf && tablePlayerCollision != null) // Si esta activado el sprite de pedir comida, quiere decir que ya se le puede tomar el pedido
        {
            orderFoodNames.Clear();

            FoodType? selectedFood = clientsFoodPreferencesData.GetRandomFood();

            if (!selectedFood.HasValue) return;

            currentSelectedFood = selectedFood.Value;

            Sprite spriteSelectedFood = clientManager.GetSpriteForRandomFood(selectedFood.Value);

            FoodData data = FoodTimesManager.Instance.GetFoodData(selectedFood.Value);
            currentFoodCookingTime = data.TimeToBeenCooked;

            clientModel.CurrentOrderDataUI = new OrderDataUI(spriteSelectedFood, clientModel.ClientData.ClientImage, clientModel.ClientData.MaxTimeWaitingFood + currentFoodCookingTime);
            OrdersManagerUI.Instance.AddOrder(clientModel.CurrentOrderDataUI);

            if (spriteSelectedFood != null)
            {
                DisableAllSpriteTypes();
                spritesTypeList[0].gameObject.SetActive(true);

                spritesTypeList[0].sprite = spriteSelectedFood;
                orderFoodNames.Clear();
                orderFoodNames.Add(selectedFood.ToString());
                AutoAdjustSpriteScale(spriteSelectedFood);

                canTakeOrder = false;
                ClearTable();
            }
        }
    }

    public void RotateOrderUIToLookAtPlayer()
    {
        Vector3 playerDirection = (playerController.transform.position - transform.position).normalized;
        Vector3 lookDirection = new Vector3(playerDirection.x, 0, playerDirection.z);

        if (lookDirection != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(lookDirection);
            order.rotation = rotation;
            orderBorder.rotation = rotation;
        }
    }


    private void GetComponents()
    {
        clientModel = GetComponent<ClientModel>();

        playerController = FindFirstObjectByType<PlayerController>();
        clientManager = FindFirstObjectByType<ClientManager>();

        anim = GetComponentInChildren<Animator>();
        order = transform.Find("Order");
        orderBorder = transform.Find("OrderBorder");

        foreach (Transform child in order)
        {
            SpriteRenderer spriteRenderer = child.GetComponentInChildren<SpriteRenderer>();
            spritesTypeList.Add(spriteRenderer);
            spriteTypeDict.Add(child.name, spriteRenderer);
        }
    }

    private void AutoAdjustSpriteScale(Sprite sprite)
    {
        float maxDimension = 0.75f; 
        Vector2 spriteSize = sprite.bounds.size;

        float scaleFactor = maxDimension / Mathf.Max(spriteSize.x, spriteSize.y);
        spritesTypeList[0].transform.localScale = Vector3.one * scaleFactor;
    }

    private void RestartAnimationsValues()
    {
        string[] parametersNames = { "Walk", "Sit", "StandUp", "DuringSit", "WaitingForChair" };

        for (int i = 0; i < parametersNames.Length; i++)
        {
            anim.SetBool(parametersNames[i], false);
        }
    }

    private void SaveTable(Table table)
    {
        if (canTakeOrder && !spritesTypeList[0].gameObject.activeSelf) // No tiene activado el sprite de la comida
        {
            tablePlayerCollision = table;
        }
    }

    private void ClearTable()
    {
        tablePlayerCollision = null;
    }
}