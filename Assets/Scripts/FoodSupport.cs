using System.Collections;
using TMPro;
using UnityEngine;

public class FoodSupport : MonoBehaviour, IInteractable
{
    private PlayerController playerController;

    private GameObject currentFood;

    public InteractionMode InteractionMode { get => InteractionMode.Press; }


    void Awake()
    {
        GetComponents();
        StartCoroutine(RegisterOutline());
    }

    void OnDestroy()
    {
        OutlineManager.Instance.Unregister(gameObject);
    }


    public void Interact(bool isPressed)
    {
        if (currentFood != null)
        {
            PlayerController.OnSupportFood?.Invoke(currentFood);
            currentFood.transform.position = transform.position + new Vector3(0, 0.375f, 0);
            currentFood.transform.SetParent(transform);
            currentFood = null;
        }
    }

    public void ShowOutline()
    {
        foreach (Transform child in playerController.PlayerView.Dish.transform)
        {
            // Verifica que las posiciones de la bandeja tengan hijos (COMIDAS) y el soporte no tenga ningun hijo (comidas)
            if (child.childCount > 0 && gameObject.transform.childCount < 1)
            {
                currentFood = child.gameObject;
                OutlineManager.Instance.ShowWithDefaultColor(gameObject);
            }
        }
    }

    public void HideOutline()
    {
        currentFood = null;
        OutlineManager.Instance.Hide(gameObject);
    }

    public void ShowMessage(TextMeshProUGUI interactionManagerUIText)
    {
        foreach (Transform child in playerController.PlayerView.Dish.transform)
        {
            // Verifica que las posiciones de la bandeja tengan hijos (COMIDAS) y el soporte no tenga ningun hijo (comidas)
            if (child.childCount > 0 && gameObject.transform.childCount < 1)
            {
                string keyText = $"<color=yellow> {PlayerInputs.Instance.GetInteractInput()} </color>";
                interactionManagerUIText.text = $"Press" + keyText + "to rest food";
            }
        }
    }

    public void HideMessage(TextMeshProUGUI interactionManagerUIText)
    {
        interactionManagerUIText.text = string.Empty;
    }


    private void GetComponents()
    {
        playerController = FindFirstObjectByType<PlayerController>();
    }

    private IEnumerator RegisterOutline()
    {
        yield return new WaitUntil(() => OutlineManager.Instance != null);

        OutlineManager.Instance.Register(gameObject);
    }
}
