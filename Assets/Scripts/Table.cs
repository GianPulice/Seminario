using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Collections;
using TMPro;

public class Table : MonoBehaviour, IInteractable
{
    private PlayerController playerController;

    private Table auxiliarTable;
    private ClientView auxiliarClientView;

    private GameObject table;
    private GameObject chair;
    private GameObject dish;
    private ParticleSystem dirty;

    private NavMeshObstacle[] navMeshObstacle;

    private List<Transform> dishPositions = new List<Transform>(); // Representa las posiciones hijas del plato

    private List<Food> currentFoods = new List<Food>();

    private float currentCleanProgress = 0f;

    private bool isOccupied = false;
    private bool isDirty = false;

    public Transform ChairPosition { get => chair.transform; }
    public Transform DishPosition { get => dish.transform; } // Solamente para que mire hacia adelante que es esta posicion

    public List<Transform> DishPositions { get => dishPositions; }

    public List<Food> CurrentFoods { get => currentFoods; set => currentFoods = value; }

    public InteractionMode InteractionMode
    {
        get
        {
            if (isDirty)
            {
                return InteractionMode.Hold;
            }

            else
            {
                return InteractionMode.Press;
            }
        }
    }

    public float CurrentCleanProgress { get => currentCleanProgress; set => currentCleanProgress = value; }

    public bool IsOccupied { get => isOccupied; set => isOccupied = value; }
    public bool IsDirty { get => isDirty; }


    void Awake()
    {
        FindObjectsAndComponents();
        StartCoroutine(RegisterOutline());
    }

    void OnDestroy()
    {
        OutlineManager.Instance.Unregister(table);
    }


    public void Interact(bool isPressed)
    {
        if (isDirty)
        {
            if (isPressed)
            {
                PlayerController.OnCleanDirtyTableIncreaseSlider?.Invoke(this);
            }

            else
            {
                PlayerController.OnCleanDirtyTableDecreaseSlider?.Invoke(this);
            }

            return;
        }

        if (auxiliarClientView != null && auxiliarClientView.CanTakeOrder)
        {
            PlayerController.OnTakeOrder?.Invoke();
            return;
        }

        PlayerController.OnHandOverFood?.Invoke();
        return;
    }

    public void ShowOutline()
    {
        if (isDirty)
        {
            OutlineManager.Instance.ShowWithDefaultColor(table);
            InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Interactive);
            PlayerView.OnActivateSliderCleanDirtyTable?.Invoke();
            return;
        }
                
        OutlineManager.Instance.ShowWithDefaultColor(table);
        InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Interactive);
    }

    public void HideOutline()
    {
        OutlineManager.Instance.Hide(table);
        InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Normal);

        PlayerView.OnDeactivateSliderCleanDirtyTable?.Invoke();

        if (ChairPosition.childCount > 0) // Si tiene a alguien sentado
        {
            ClientView clientView = gameObject.GetComponentInChildren<ClientView>();
            clientView.CanTakeOrder = false;

            PlayerController.OnTableCollisionExitForTakeOrder?.Invoke();
            PlayerController.OnTableCollisionExitForHandOverFood?.Invoke();

            auxiliarTable = null;
            auxiliarClientView = null;
            return;
        }
    }

    public bool TryGetInteractionMessage(out string message)
    {
        string keyText = $"<color=yellow> {PlayerInputs.Instance.GetInteractInput()} </color>";

        if (isDirty)
        {
            message = $"Hold {keyText} to clean the table";
            return true;
        }

        if (auxiliarTable == null || auxiliarTable != this)
        {
            auxiliarTable = this;
            auxiliarClientView = GetComponentInChildren<ClientView>();
        }

        if (ChairPosition.childCount > 0 && auxiliarClientView != null)
        {
            if (isOccupied && auxiliarClientView.ReturnSpriteWaitingToBeAttendedIsActive())
            {
                if (!auxiliarClientView.CanTakeOrder)
                {
                    auxiliarClientView.CanTakeOrder = true;
                    PlayerController.OnTableCollisionEnterForTakeOrder?.Invoke(this);
                }
                message = $"Press {keyText} to take order";
                return true;
            }

            bool hasChildren = false;
            foreach (Transform child in playerController.PlayerView.Dish.transform)
            {
                if (child.childCount > 0)
                {
                    hasChildren = true;
                    break;
                }
            }

            if (hasChildren && isOccupied && auxiliarClientView.ReturnSpriteFoodIsActive())
            {
                PlayerController.OnTableCollisionEnterForHandOverFood?.Invoke(this);

                message = $"Press {keyText} to deliver the order";
                return true;
            }
        }
        message = null;
        return false;
    }

    public void SetDirty(bool current)
    {
        isDirty = current;
        dirty.gameObject.SetActive(current);
        var main = dirty.main;
        main.loop = current;
    }

    /// <summary>
    /// Analizar el metodo por el tema de el NavMesh del NPC
    /// </summary>
    public void SetNavMeshObstacles(bool current)
    {
        for (int i = 0; i < navMeshObstacle.Length; i++)
        {
            // No ejecutar si ya estaba activado y current es true, esto sirve por si se fue de la cola de espera porque no se libero ninguna silla
            if (navMeshObstacle[i].isActiveAndEnabled && current)
            {
                continue;
            }

            navMeshObstacle[i].enabled = current;
        }
    }


    private void FindObjectsAndComponents()
    {
        playerController = FindFirstObjectByType<PlayerController>();

        table = transform.Find("Table").gameObject;
        chair = transform.Find("Chair").gameObject;
        dish = transform.Find("Dish").gameObject;
        dirty = GetComponentInChildren<ParticleSystem>(true); // Indica que busca componentes en gameObject desactivados

        navMeshObstacle = GetComponentsInChildren<NavMeshObstacle>();

        foreach (Transform childs in dish.transform)
        {
            dishPositions.Add(childs.GetComponent<Transform>());
        }
    }

    private IEnumerator RegisterOutline()
    {
        yield return new WaitUntil(() => OutlineManager.Instance != null);

        OutlineManager.Instance.Register(table);
    }
}
