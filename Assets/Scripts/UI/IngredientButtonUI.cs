
using TMPro;
using UnityEngine;

public class IngredientButtonUI : MonoBehaviour
{
    [Header("Igredient Type")]
    [SerializeField] private IngredientType ingredientType;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI stockText;

    public IngredientType IngredientType => ingredientType;

    public void UpdateUI(int price, int stock)
    {
        if (priceText == null) return;
        if (stockText == null) return;

        if (priceText != null)
        {
            priceText.text = $"Cost: {price}";
        }

        if (stockText != null)
        {
            stockText.text = $"Stock: {stock}"; 
        }
    }
}
