using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum FoodType // FrutosDelBosqueOscuro, SopaDeLaLunaPlateada, CarneDeBestia, CarneCuradaDelAbismo, SusurroDelElixir
{
    DarkWoodBerries, SoupOfTheSilverMoon, BeastStew, AbyssCuredMeet, LastWhisperElixir
}

public enum CookingStates
{
    Raw,
    Cooked,
    Burned
}

public class Food : MonoBehaviour, IInteractable
{
    // No usar el metodo OnDisabled de Unity

    [SerializeField] private FoodData foodData;

    private FoodMesh defaultMesh;
    private FoodMesh foodMesh;

    private PlayerController playerController;
    private CookingManager cookingManager;
    private Table currentTable; // Esta Table hace referencia a la mesa en la cual podemos entregar el pedido
    private Slider cookingBar;
    private MeshRenderer meshRenderer;
    private Color originalColor;

    private Transform stovePosition;
    private Transform playerDishPosition;

    [SerializeField] private FoodType foodType;
    private CookingStates currentCookingState;

    private float cookTimeCounter = 0f;

    private bool isInstantiateFirstTime = true;
    private bool isInPlayerDishPosition = false;
    private bool isServedInTable = false;

    public InteractionMode InteractionMode { get => InteractionMode.Press; }

    public FoodType FoodType { get => foodType; }
    public CookingStates CurrentCookingState { get => currentCookingState; }


    public class FoodMesh
    {
        public GameObject root;
        public Collider collider;
        public Rigidbody rb;
        public MeshRenderer ms;

        public FoodMesh(GameObject root, Collider collider, Rigidbody rb, MeshRenderer ms)
        {
            this.root = root;
            this.collider = collider;
            this.rb = rb;
            this.ms = ms;
        }
    }


    void Awake()
    {
        SuscribeToPlayerControllerEvents();
        GetComponents();
        Initialize();
        CoroutineHelper.Instance.StartHelperCoroutine(RegisterOutline());
    }

    // Simulacion de Update
    void UpdateFood()
    {
        RotateSliderUIToLookAtPlayer();
    }

    void OnEnable()
    {
        SuscribeToUpdateManagerEvent();
        StartCoroutine(CookGameObject());
    }

    void OnDisable()
    {
        UnsuscribeToUpdateManagerEvent();
    }

    void OnDestroy()
    {
        UnsuscribeToUpdateManagerEvent();
        UnsuscribeToPlayerControllerEvents();
        OutlineManager.Instance.Unregister(defaultMesh.root);
        OutlineManager.Instance.Unregister(foodMesh.root);
    }


    public void Interact(bool isPressed)
    {
        if (gameObject.activeSelf && !isServedInTable && !isInPlayerDishPosition && cookingManager.AvailableDishPositions.Count > 0)
        {
            if (cookingBar.gameObject.activeSelf)
            {
                cookingBar.gameObject.SetActive(false);
            }

            PlayerView.OnEnabledDishForced?.Invoke(true);

            isInPlayerDishPosition = true;

            cookingManager.ReleaseStovePosition(stovePosition);
            playerDishPosition = cookingManager.MoveFoodToDish(this);

            SetMeshRootActive(defaultMesh, false);
            EnabledOrDisablePhysics(defaultMesh, false);
            SetMeshRootActive(foodMesh, true);
            EnabledOrDisablePhysics(foodMesh, false);
        }
    }

    public void ShowOutline()
    {
        if (cookingManager.AvailableDishPositions.Count > 0 && !isServedInTable && !isInPlayerDishPosition)
        {
            OutlineManager.Instance.ShowWithDefaultColor(defaultMesh.root);
            OutlineManager.Instance.ShowWithDefaultColor(foodMesh.root);

            InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Interactive);
        }
    }

    public void HideOutline()
    {
        OutlineManager.Instance.Hide(defaultMesh.root);
        OutlineManager.Instance.Hide(foodMesh.root);

        InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Normal);
    }

    public bool TryGetInteractionMessage(out string message)
    {
        if (cookingManager.AvailableDishPositions.Count > 0 && !isServedInTable && !isInPlayerDishPosition)
        {
            string keyText = $"<color=yellow> {PlayerInputs.Instance.GetInteractInput()} </color>";
            message = $"Press {keyText} to grab food";

            return true;
        }

        message = null;
        return false;
    }
    public void ReturnObjetToPool()
    {
        cookingManager.ReturnObjectToPool(foodType, this);
        RestartValues();
    }

    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdateFood;
    }

    private void UnsuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate -= UpdateFood;
    }

    private void SuscribeToPlayerControllerEvents()
    {
        PlayerController.OnHandOverFood += HandOver;
        PlayerController.OnSupportFood += SupportFood;
        PlayerController.OnThrowFoodToTrash += ThrowFoodToTrash;

        PlayerController.OnTableCollisionEnterForHandOverFood += SaveTable;
        PlayerController.OnTableCollisionExitForHandOverFood += ClearTable;
    }

    private void UnsuscribeToPlayerControllerEvents()
    {
        PlayerController.OnHandOverFood -= HandOver;
        PlayerController.OnSupportFood -= SupportFood;
        PlayerController.OnThrowFoodToTrash -= ThrowFoodToTrash;

        PlayerController.OnTableCollisionEnterForHandOverFood -= SaveTable;
        PlayerController.OnTableCollisionExitForHandOverFood -= ClearTable;
    }

    private void GetComponents()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        cookingManager = FindFirstObjectByType<CookingManager>();
        cookingBar = GetComponentInChildren<Slider>();
    }

    private IEnumerator RegisterOutline()
    {
        yield return new WaitUntil(() => OutlineManager.Instance != null);

        OutlineManager.Instance.Register(defaultMesh.root);
        OutlineManager.Instance.Register(foodMesh.root);
    }

    private void Initialize()
    {
        SetupFoodMesh(ref defaultMesh, "DefaultMesh");
        string cleanName = gameObject.name.Replace("(Clone)", "").Trim();
        SetupFoodMesh(ref foodMesh, cleanName + "Mesh");

        SetMeshRootActive(defaultMesh, true);
        SetMeshRootActive(foodMesh, false);

        meshRenderer = defaultMesh.ms;
        originalColor = meshRenderer.material.color;
        currentCookingState = CookingStates.Raw;
    }

    private void SetupFoodMesh(ref FoodMesh foodMesh, string gameObjectHierarchyName)
    {
        GameObject meshRoot = transform.Find(gameObjectHierarchyName).gameObject;
        Collider collider = meshRoot.GetComponent<Collider>();
        Rigidbody rb = meshRoot.GetComponent<Rigidbody>();
        MeshRenderer meshRenderer = meshRoot.GetComponent<MeshRenderer>();

        foodMesh = new FoodMesh(meshRoot, collider, rb, meshRenderer);
    }

    // Ejecutar unicamente la corrutina cuando se activa el objeto en caso de que se haya puesto en la hornalla
    private IEnumerator CookGameObject()
    {
        if (!isInPlayerDishPosition && !isInstantiateFirstTime)
        {
            stovePosition = cookingManager.CurrentStove;

            cookingBar.maxValue = foodData.TimeToBeenCooked;
            cookingBar.value = 0;
            cookTimeCounter = 0f;

            while (cookTimeCounter <= foodData.TimeToBeenCooked + foodData.TimeToBeenBurned)
            {
                cookTimeCounter += Time.deltaTime;

                if (cookTimeCounter <= foodData.TimeToBeenCooked)
                {
                    cookingBar.value = cookTimeCounter;
                }

                if (cookTimeCounter >= foodData.TimeToBeenCooked && cookingBar.gameObject.activeSelf)
                {
                    cookingBar.gameObject.SetActive(false);
                }

                if (isInPlayerDishPosition)
                {
                    CheckCookingState();
                    yield break;
                }

                // Lerp del color original a un color oscuro
                Color targetColor = Color.Lerp(originalColor, Color.black, cookTimeCounter / (foodData.TimeToBeenCooked + foodData.TimeToBeenBurned));
                meshRenderer.material.color = targetColor;                

                yield return null;
            }

            CheckCookingState();
        }

        isInstantiateFirstTime = false;
    }

    private void SetMeshRootActive(FoodMesh mesh, bool value)
    {
        mesh.root?.SetActive(value);
    }

    // Si es true activa las fisicas, sino las desactiva
    private void EnabledOrDisablePhysics(FoodMesh mesh, bool value)
    {
        if (value)
        {
            mesh.rb.isKinematic = false;
            mesh.collider.enabled = true;
        }

        else
        {
            mesh.rb.isKinematic = true;
            mesh.collider.enabled = false;
        }
    }

    private void RotateSliderUIToLookAtPlayer()
    {
        Vector3 playerDirection = (playerController.transform.position - cookingBar.transform.position).normalized;
        Vector3 lookDirection = new Vector3(playerDirection.x, 0, playerDirection.z);

        if (lookDirection != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(lookDirection);
            cookingBar.transform.rotation = rotation;
        }
    }

    private void RestartValues()
    {
        meshRenderer.material.color = originalColor;

        cookingBar.gameObject.SetActive(true);

        stovePosition = null;
        playerDishPosition = null;

        EnabledOrDisablePhysics(defaultMesh, true);
        EnabledOrDisablePhysics(foodMesh, true);

        currentCookingState = CookingStates.Raw;

        cookTimeCounter = 0f;
        isInPlayerDishPosition = false;
        isServedInTable = false;

        OutlineManager.Instance.Hide(defaultMesh.root);
        OutlineManager.Instance.Hide(foodMesh.root);

        SetMeshRootActive(defaultMesh, true);
        SetMeshRootActive(foodMesh, false);

        //transform.position = Vector3.zero;
    }

    private void SaveTable(Table table)
    {
        if (isInPlayerDishPosition)
        {
            currentTable = table;
        }
    }

    private void ClearTable()
    {
        currentTable = null;
    }

    private void CheckCookingState()
    {
        if (cookTimeCounter < foodData.TimeToBeenCooked)
        {
            currentCookingState = CookingStates.Raw;
        }

        else if (cookTimeCounter >= foodData.TimeToBeenCooked && cookTimeCounter <= foodData.TimeToBeenCooked + foodData.TimeToBeenBurned)
        {
            currentCookingState = CookingStates.Cooked;
        }

        else if (cookTimeCounter > foodData.TimeToBeenCooked + foodData.TimeToBeenBurned)
        {
            currentCookingState = CookingStates.Burned;
        }
    }

    private void HandOver()
    {
        if (isInPlayerDishPosition && currentTable != null && currentTable.IsOccupied)
        {
            cookingManager.ReleaseDishPosition(playerDishPosition);

            Transform freeSpot = null;

            foreach (Transform spot in currentTable.DishPositions)
            {
                if (spot.childCount == 0)
                {
                    freeSpot = spot;
                    break;
                }
            }

            if (freeSpot != null)
            {
                transform.SetParent(freeSpot);
                transform.position = freeSpot.position + new Vector3(0, 0.1f, 0);

                EnabledOrDisablePhysics(foodMesh, true);

                isInPlayerDishPosition = false;
                isServedInTable = true;

                currentTable.CurrentFoods.Add(this); 

                ClearTable();
            }
        }
    }

    private void SupportFood(GameObject currentFood)
    {
        if (currentFood != null && isInPlayerDishPosition)
        {
            cookingManager.ReleaseDishPosition(playerDishPosition);
            isInPlayerDishPosition = false;
            EnabledOrDisablePhysics(foodMesh, true);
        }
    }

    private void ThrowFoodToTrash()
    {
        if (gameObject.activeSelf && isInPlayerDishPosition)
        {
            cookingManager.ReleaseDishPosition(playerDishPosition);
            ReturnObjetToPool();
        }
    }
}