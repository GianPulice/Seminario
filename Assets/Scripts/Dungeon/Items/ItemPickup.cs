using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private bool destroyOnPickup = true;

    private Outline outline;

    public InteractionMode InteractionMode => throw new System.NotImplementedException();

    private void Awake()
    {
        outline = GetComponent<Outline>();
    }

    public void Interact(bool isPressed)
    {
        switch (itemData.type)
        {
            case ItemType.Currency:
                MoneyManager.Instance.AddMoney(itemData.valueInTeeth);
                Debug.Log($"+{itemData.valueInTeeth} dientes");
                break;

            case ItemType.Recipe:
                Debug.Log($"Receta desbloqueada: {itemData.itemName}");
                break;

            case ItemType.Misc:
                Debug.Log($"Objeto misceláneo obtenido: {itemData.itemName}");
                break;
        }

        if (destroyOnPickup)
            Destroy(gameObject);
    }

    public void ShowOutline()
    {
        if (outline != null)
        {
            outline.OutlineWidth = 5f;
            InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Interactive);
        }
    }

    public void HideOutline()
    {
        if (outline != null)
        {
            outline.OutlineWidth = 0f;
            InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Normal);
        }
    }

    public void ShowMessage(TextMeshProUGUI interactionManagerUIText)
    {
        throw new System.NotImplementedException();
    }

    public void HideMessage(TextMeshProUGUI interactionManagerUIText)
    {
        throw new System.NotImplementedException();
    }
}