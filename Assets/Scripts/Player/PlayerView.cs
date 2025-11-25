using System;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    private GameObject dish; // Representa la bandeja del player

    private static event Action<bool> onEnabledDishForced;

    private static Action onEnterTutorial;
    private static Action onExitTutorial;

    private static Action onEnterInCookMode; // Entrar en el modo cocinar para mostrar la UI de cocina
    private static Action onExitInCookMode; // Salir del modo cocinar para ocultar la UI de cocina

    private static Action onEnterInAdministrationMode; // Entrar en el modo administracion para mostrar la UI de administracion
    private static Action onExitInAdministrationMode; // Salir del modo administracion para ocultar la UI de administracion

    private static Action onActivateSliderCleanDirtyTable; // Mostrar la UI del slider
    private static Action onDeactivateSliderCleanDirtyTable; // Esconder la UI del slider

    public GameObject Dish { get => dish; }

    public static Action<bool> OnEnabledDishForced { get => onEnabledDishForced; set => onEnabledDishForced = value; }  

    public static Action OnEnterTutorial { get => onEnterTutorial; set => onEnterTutorial = value; }
    public static Action OnExitTutorial { get => onExitTutorial; set => onExitTutorial = value; }

    public static Action OnEnterInCookMode { get => onEnterInCookMode; set => onEnterInCookMode = value; }
    public static Action OnExitInCookMode { get => onExitInCookMode; set => onExitInCookMode = value; }
    
    public static Action OnEnterInAdministrationMode { get => onEnterInAdministrationMode; set => onEnterInAdministrationMode = value;}
    public static Action OnExitInAdministrationMode { get => onExitInAdministrationMode; set => onExitInAdministrationMode = value;}    

    public static Action OnActivateSliderCleanDirtyTable { get => onActivateSliderCleanDirtyTable; set => onActivateSliderCleanDirtyTable = value; }
    public static Action OnDeactivateSliderCleanDirtyTable { get => onDeactivateSliderCleanDirtyTable; set => onDeactivateSliderCleanDirtyTable = value; }


    void Awake()
    {
        SuscribeToOwnEvent();
        GetComponents();
    }

    void OnDestroy()
    {
        UnsuscribeToOwnEvent();
    }


    public void ShowOrHideDish(bool current)
    {
        dish.SetActive(current);
    }


    private void SuscribeToOwnEvent()
    {
        onEnabledDishForced += ShowOrHideDish;
    }

    private void UnsuscribeToOwnEvent()
    {
        onEnabledDishForced -= ShowOrHideDish;
    }

    private void GetComponents()
    {
        dish = GetComponentInChildren<MeshCollider>().gameObject;

        //dish = transform.Find("Dish").gameObject;
    }
}
