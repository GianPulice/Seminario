
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private float duration = 0.3f; // Duración de la transición

    private Coroutine healthCoroutine;

    private void Awake()
    {
        // Suscribirse al evento estático del HUD
        PlayerDungeonHUD.OnHealthChanged += UpdateHealthUI;
    }

    private void OnDestroy()
    {
        PlayerDungeonHUD.OnHealthChanged -= UpdateHealthUI;
    }

    private void UpdateHealthUI(float currentHP, float maxHP)
    {
        float targetFill = Mathf.Clamp01(currentHP / maxHP);

        if (healthCoroutine != null)
            StopCoroutine(healthCoroutine);

        healthCoroutine = StartCoroutine(LerpHealthFill(targetFill));
    }

    private IEnumerator LerpHealthFill(float target)
    {
        float start = healthFillImage.fillAmount;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            healthFillImage.fillAmount = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        healthFillImage.fillAmount = target; // asegurar que quede exacto

    }
}
