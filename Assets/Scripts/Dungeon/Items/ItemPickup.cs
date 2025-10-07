using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private Color interactColor;
    [SerializeField] private bool destroyOnPickup = true;

    public InteractionMode InteractionMode => InteractionMode.Press;

    private void Awake()
    {
        StartCoroutine(RegisterOutline());
    }
    private void OnDestroy()
    {
        OutlineManager.Instance.Unregister(gameObject);
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
        OutlineManager.Instance.ShowWithCustomColor(gameObject, interactColor);
        InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Interactive);

    }

    public void HideOutline()
    {

        OutlineManager.Instance.Hide(gameObject);
        InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Normal);
    }

    public void ShowMessage(TextMeshProUGUI interactionManagerUIText)
    {
        
    }

    public void HideMessage(TextMeshProUGUI interactionManagerUIText)
    {
        
    }
    private IEnumerator RegisterOutline()
    {
        yield return new WaitUntil(() => OutlineManager.Instance != null);
        OutlineManager.Instance.Register(gameObject);
    }
}