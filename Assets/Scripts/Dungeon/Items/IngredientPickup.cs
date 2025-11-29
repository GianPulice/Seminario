
using UnityEngine;

public class IngredientPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private IngredientType ingredient;
    [SerializeField] private int amount = 1;
    [SerializeField] private bool destroyOnPickup = true;

    public InteractionMode InteractionMode => throw new System.NotImplementedException();


    public void HideMessage(TMPro.TextMeshProUGUI interactionManagerUIText)
    {
        throw new System.NotImplementedException();
    }

    public void HideOutline()
    {
        throw new System.NotImplementedException();
    }

    public void Interact(bool isPressed)
    {
        IngredientInventoryManager.Instance.IncreaseIngredientStock(ingredient, amount);

        Debug.Log($"+{amount} {ingredient}");
        if (destroyOnPickup) Destroy(gameObject);
    }
    public bool TryGetInteractionMessage(out string message)
    {
        string keyText = $"<color=yellow> {PlayerInputs.Instance.GetInteractInput()} </color>";
        message = $"Press {keyText} to enter administration";

        return true;
    }
    public void ShowMessage(TMPro.TextMeshProUGUI interactionManagerUIText)
    {
        throw new System.NotImplementedException();
    }

    public void ShowOutline()
    {
        throw new System.NotImplementedException();
    }
}
