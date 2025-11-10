using System;
using UnityEngine;
using TMPro;

public class MoneyManagerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;

    [SerializeField] private RectTransform rectTransformToMove;

    [Header("Animación de Posición")]
    [SerializeField] private Vector2 normalPosition = new Vector2(-200, 100);
    [SerializeField] private Vector2 adminPosition = new Vector2(0, 0);
    [SerializeField] private float animTime = 0.4f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutQuad;


    public static Action<TextMeshProUGUI> OnTextGetComponent { get => onTextGetComponent; set => onTextGetComponent = value; }

    private static event Action<TextMeshProUGUI> onTextGetComponent;

    void Awake()
    {
        if (rectTransformToMove == null)
        {
            Debug.LogError("¡No se ha asignado 'Rect Transform To Move' en el Inspector de MoneyManagerUI!", this);
            enabled = false;
            return;
        }

        rectTransformToMove.anchoredPosition = normalPosition;

        InvokeEvent();

        SuscribeToPlayerViewEvents();
    }
    void OnDestroy()
    {
        UnsuscribeToPlayerViewEvents();
    }
    private void InvokeEvent()
    {
        if (moneyText == null)
        {
            Debug.LogError("¡MoneyText no está asignado en el Inspector de MoneyManagerUI!", this);
            return;
        }
        onTextGetComponent?.Invoke(moneyText);
    }
    private void SuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInAdministrationMode += HandleEnterAdminMode;
        PlayerView.OnExitInAdministrationMode += HandleExitAdminMode;

        PlayerView.OnEnterInCookMode += HandleEnterCookMode;
        PlayerView.OnExitInCookMode += HandleExitCookMode;
    }
    private void UnsuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInAdministrationMode -= HandleEnterAdminMode;
        PlayerView.OnExitInAdministrationMode -= HandleExitAdminMode;

        PlayerView.OnEnterInCookMode -= HandleEnterCookMode;
        PlayerView.OnExitInCookMode -= HandleExitCookMode;
    }

    private void HandleEnterAdminMode()
    {
        if (rectTransformToMove == null) return;

        LeanTween.cancel(rectTransformToMove.gameObject); // Cancelar animación anterior
        LeanTween.move(rectTransformToMove, adminPosition, animTime)
            .setEase(easeType)
            .setIgnoreTimeScale(true); 

    }

    private void HandleExitAdminMode()
    {
        if (rectTransformToMove == null) return;

        LeanTween.cancel(rectTransformToMove.gameObject);
        LeanTween.move(rectTransformToMove, normalPosition, animTime)
            .setEase(easeType)
            .setIgnoreTimeScale(true);
    
    }
    private void HandleEnterCookMode()
    {
        rectTransformToMove.gameObject.SetActive(false);
    }
    private void HandleExitCookMode()
    {
        rectTransformToMove.gameObject.SetActive(true);
    }
}
