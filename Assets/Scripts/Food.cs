using System.Collections;
using UnityEngine;
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
    private AudioSource currentAudioSource3D;
    private Slider cookingBarUI;
    private Image sliderFillCookingBarUI;
    private Slider cookingBarOutside;
    private Image sliderFillCookingBarOutside;
    private RectTransform cookingBarUIRectTransform;
    private MeshRenderer meshRenderer;
    private ParticleSystem smoke;

    private Transform stovePosition;
    private Transform playerDishPosition;

    private Vector3 nativeParentScaleSize;
                                                                 // Naranja                                   // Lima   
    private Color[] colorsSlider = new Color[] { Color.red, new Color(1f, 0.5f, 0f), Color.yellow, new Color(0.5f, 1f, 0f), Color.green } ;

    [SerializeField] private FoodType foodType;
    private CookingStates currentCookingState;

    private float cookTimeCounter = 0f;

    private bool isInstantiateFirstTime = true;
    private bool canChangeCookingBarUIPosition = false;
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
        SuscribeToPlayerViewEvent();
        SuscribeToPauseManagerEvent();
        CoroutineHelper.Instance.StartCoroutine(SuscribeToCookingManagerEvent());
        SuscribeToPlayerControllerEvents();
        GetComponents();
        Initialize();
        CoroutineHelper.Instance.StartHelperCoroutine(RegisterOutline());
    }

    // Simulacion de Update
    void UpdateFood()
    {
        RotateSliderOutsideToLookAtPlayer();
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
        UnsuscribeToPlayerViewEvent();
        UnsuscribeToPauseManagerEvent();
        UnsuscribeToCookingManagerEvent();
        UnsuscribeToPlayerControllerEvents();
        OutlineManager.Instance.Unregister(defaultMesh.root);
        OutlineManager.Instance.Unregister(foodMesh.root);
    }


    public void Interact(bool isPressed)
    {
        if (gameObject.activeSelf && !isServedInTable && !isInPlayerDishPosition && cookingManager.AvailableDishPositions.Count > 0)
        {
            currentAudioSource3D.Stop();
            AudioManager.Instance.PlayOneShotSFX("GrabFood");

            transform.localScale = nativeParentScaleSize;

            if (cookingBarUI.gameObject.activeSelf)
            {
                cookingBarUI.gameObject.SetActive(false);
            }

            if (cookingBarOutside.gameObject.activeSelf)
            {
                cookingBarOutside.gameObject.SetActive(false);
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

    /// <summary>
    /// Analizar para que lo haga con el boxCollider en vez del meshrenderer
    /// </summary>
    public float GetBottomOffset()
    {
        // Tomamos el mesh que esté activo (default o cocinado)
        MeshRenderer renderer = foodMesh.ms;

        if (renderer == null)
            return 0f;

        Bounds bounds = renderer.bounds;
        float bottomY = bounds.min.y;
        float centerY = transform.position.y;

        // La diferencia entre el centro del objeto y la base del mesh
        return centerY - bottomY;
    }


    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdateFood;
    }

    private void UnsuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate -= UpdateFood;
    }

    private void SuscribeToPlayerViewEvent()
    {
        PlayerView.OnEnterInCookMode += OnEnterInCookMode;
        PlayerView.OnExitInCookMode += OnExitInCookMode;
    }

    private void UnsuscribeToPlayerViewEvent()
    {
        PlayerView.OnEnterInCookMode -= OnEnterInCookMode;
        PlayerView.OnExitInCookMode -= OnExitInCookMode;
    }

    private void SuscribeToPauseManagerEvent()
    {
        PauseManager.OnGamePaused += OnPauseAudio3D;
        PauseManager.OnGameUnPaused += OnUnPauseAduio3D;
    }

    private void UnsuscribeToPauseManagerEvent()
    {
        PauseManager.OnGamePaused -= OnPauseAudio3D;
        PauseManager.OnGameUnPaused -= OnUnPauseAduio3D;
    }

    private IEnumerator SuscribeToCookingManagerEvent()
    {
        yield return new WaitUntil(() => CookingManager.Instance != null);

        CookingManager.Instance.OnAvailableStoveIndex += SetCookingBarUIPositionWhenCookFood;
    }

    private void UnsuscribeToCookingManagerEvent()
    {
        CookingManager.Instance.OnAvailableStoveIndex -= SetCookingBarUIPositionWhenCookFood;
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
        currentAudioSource3D = GetComponent<AudioSource>();   
        cookingBarUI = transform.Find("CanvasUI").GetComponentInChildren<Slider>();
        cookingBarOutside = transform.Find("CanvasOutside").GetComponentInChildren<Slider>();
        sliderFillCookingBarUI = cookingBarUI.fillRect.GetComponent<Image>();
        sliderFillCookingBarOutside = cookingBarOutside.fillRect.GetComponent<Image>();
        cookingBarUIRectTransform = cookingBarUI.GetComponent<RectTransform>();
        smoke = GetComponentInChildren<ParticleSystem>(true); // Inidica que busca componentes dentro de gameObjects que estan desactivados
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
        SetupFoodMesh(ref foodMesh, gameObject.name.Replace("(Clone)", "").Trim() + "Mesh");

        SetMeshRootActive(defaultMesh, true);
        SetMeshRootActive(foodMesh, false);

        meshRenderer = defaultMesh.ms;
        currentCookingState = CookingStates.Raw;
        defaultMesh.ms.material = foodData.RawMaterial;
        nativeParentScaleSize = transform.localScale;
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
            AudioClip clipCooking = AudioManager.Instance.GetSFX("Cooking");
            currentAudioSource3D.clip = clipCooking;
            currentAudioSource3D.loop = true;
            currentAudioSource3D.Play();
            stovePosition = cookingManager.CurrentStove;

            cookingBarUI.maxValue = foodData.TimeToBeenCooked;
            cookingBarUI.value = 0;
            cookingBarOutside.maxValue = foodData.TimeToBeenCooked;
            cookingBarOutside.value = 0;

            cookTimeCounter = 0f;

            bool isFoodAlreadyCooked = false;

            meshRenderer.material = foodData.RawMaterial;

            while (cookTimeCounter <= foodData.TimeToBeenCooked + foodData.TimeToBeenBurned) // Esto es para que la logica se ejecute durante el tiempo de coccion + el tiempo de quemarse y despues no funcione mas la corrutina
            {
                cookTimeCounter += Time.deltaTime;

                if (cookTimeCounter <= foodData.TimeToBeenCooked)
                {
                    cookingBarUI.value = cookTimeCounter;
                    sliderFillCookingBarUI.color = EvaluateColor(cookingBarUI.value / cookingBarUI.maxValue);
                    cookingBarOutside.value = cookTimeCounter;
                    sliderFillCookingBarOutside.color = EvaluateColor(cookingBarOutside.value / cookingBarOutside.maxValue);
                }

                if (cookTimeCounter >= foodData.TimeToBeenCooked && !isFoodAlreadyCooked)
                {
                    currentAudioSource3D.Stop();
                    AudioManager.Instance.PlayOneShotSFX("FoodIsAlredyCooked");

                    cookingBarUI.value = cookingBarUI.maxValue;
                    sliderFillCookingBarUI.color = EvaluateColor(1f);

                    cookingBarOutside.value = cookingBarOutside.maxValue;
                    sliderFillCookingBarOutside.color = EvaluateColor(1f);

                    yield return new WaitForSeconds(0.1f); // Esperara este tiempo para que se muestre el slider completo

                    cookingBarUI.gameObject.SetActive(false);
                    cookingBarOutside.gameObject.SetActive(false);

                    isFoodAlreadyCooked = true;
                }

                if (cookTimeCounter >= foodData.TimeToBeenCooked + foodData.TimeToBeenBurned && !smoke.gameObject.activeSelf)
                {
                    AudioClip clipBurning = AudioManager.Instance.GetSFX("Burning");
                    currentAudioSource3D.clip = clipBurning;
                    currentAudioSource3D.loop = true;
                    currentAudioSource3D.Play();
                    smoke.gameObject.SetActive(true);
                }

                if (isInPlayerDishPosition)
                {
                    CheckCookingState();
                    yield break;
                }

                if (cookTimeCounter < foodData.TimeToBeenCooked)
                {
                    meshRenderer.material = foodData.RawMaterial;
                }

                else if (cookTimeCounter >= foodData.TimeToBeenCooked && cookTimeCounter < foodData.TimeToBeenCooked + foodData.TimeToBeenBurned)
                {
                    meshRenderer.material = foodData.FoodMaterial;
                }

                else if (cookTimeCounter >= foodData.TimeToBeenCooked + foodData.TimeToBeenBurned)
                {
                    meshRenderer.material = foodData.BurnedMaterial;
                }

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

    private void OnEnterInCookMode()
    {
        if (cookTimeCounter >= foodData.TimeToBeenCooked) return;

        cookingBarUI.gameObject.SetActive(true);
        cookingBarOutside.gameObject.SetActive(false);
    }

    private void OnExitInCookMode()
    {
        if (cookTimeCounter >= foodData.TimeToBeenCooked) return;

        cookingBarUI.gameObject.SetActive(false);
        cookingBarOutside.gameObject.SetActive(true);
    }

    private void OnPauseAudio3D()
    {
        currentAudioSource3D.Pause();
    }

    private void OnUnPauseAduio3D()
    {
        currentAudioSource3D.UnPause();
    }

    private void RotateSliderOutsideToLookAtPlayer()
    {
        Vector3 playerDirection = (playerController.transform.position - cookingBarOutside.transform.position).normalized;
        Vector3 lookDirection = new Vector3(playerDirection.x, 0, playerDirection.z);

        if (lookDirection != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(lookDirection);
            cookingBarOutside.transform.rotation = rotation;
        }
    }

    private void RestartValues()
    {
        meshRenderer.material = foodData.RawMaterial;

        transform.localScale = nativeParentScaleSize;
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        cookingBarUIRectTransform.anchoredPosition = new Vector2(0.16f, 1.367f);

        cookingBarUI.gameObject.SetActive(true);
        cookingBarOutside.gameObject.SetActive(true);
        smoke.gameObject.SetActive(false);

        stovePosition = null;
        playerDishPosition = null;

        EnabledOrDisablePhysics(defaultMesh, true);
        EnabledOrDisablePhysics(foodMesh, true);

        currentCookingState = CookingStates.Raw;

        cookTimeCounter = 0f;
        isInPlayerDishPosition = false;
        canChangeCookingBarUIPosition = false;
        isServedInTable = false;

        OutlineManager.Instance.Hide(defaultMesh.root);
        OutlineManager.Instance.Hide(foodMesh.root);

        SetMeshRootActive(defaultMesh, true);
        SetMeshRootActive(foodMesh, false);
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
            AudioManager.Instance.PlayOneShotSFX("DeliverOrder");

            Vector3 biggerSize = nativeParentScaleSize * 2f;
            SetGlobalScale(transform, biggerSize);
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
            Vector3 biggerSize = nativeParentScaleSize * 1.5f;
            SetGlobalScale(transform, biggerSize);
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

    private void SetGlobalScale(Transform transform, Vector3 globalScale)
    {
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(
            globalScale.x / transform.lossyScale.x * transform.localScale.x,
            globalScale.y / transform.lossyScale.y * transform.localScale.y,
            globalScale.z / transform.lossyScale.z * transform.localScale.z
        );
    }

    private Color EvaluateColor(float t)
    {
        if (t <= 0f) return colorsSlider[0];
        if (t >= 1f) return colorsSlider[colorsSlider.Length - 1];

        float scaled = t * (colorsSlider.Length - 1);
        int index = Mathf.FloorToInt(scaled);
        float lerp = scaled - index;

        return Color.Lerp(colorsSlider[index], colorsSlider[index + 1], lerp);
    }

    private void SetCookingBarUIPositionWhenCookFood(int stoveIndex)
    {
        if (!gameObject.activeSelf) return;
        if (canChangeCookingBarUIPosition && gameObject.activeSelf) return;

        // Si el indice es 1 no hacer nada

        if (stoveIndex == 0)
        {
            cookingBarUIRectTransform.anchoredPosition = new Vector2(0.7f, 0f);
        }

        else if (stoveIndex == 2)
        {
            cookingBarUIRectTransform.anchoredPosition = new Vector2(-0.70f, 0f);
        }

        canChangeCookingBarUIPosition = true;
    }
}