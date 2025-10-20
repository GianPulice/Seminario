using System.Collections;
using UnityEngine;

/// <summary>
/// Permite hacer parpadear el mesh en rojo cuando recibe daño.
/// No crea materiales nuevos: usa un MaterialPropertyBlock.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class DamageFlash : MonoBehaviour
{
    [Header("Flash")]
    [Tooltip("Color del destello")]
    [SerializeField] private Color flashColor = Color.red;
    [Tooltip("Duración de cada destello (segundos)")]
    [SerializeField] private float flashTime = 0.08f;
    [Tooltip("Cuántas veces parpadea")]
    [SerializeField] private int flashes = 3;

    private static readonly int ColorID = Shader.PropertyToID("_Color"); 

    private Renderer rendererRef;
    private MaterialPropertyBlock block;
    private Color originalColor;

    private void Awake()
    {
        rendererRef = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();

        // Guarda el color original la primera vez
        rendererRef.GetPropertyBlock(block);
        originalColor = block.HasColor(ColorID)
                        ? block.GetColor(ColorID)
                        : rendererRef.sharedMaterial.GetColor(ColorID);
    }

    /// <summary>Llamar desde TakeDamage.</summary>
    public void TriggerFlash()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }
    public void ResetFlash()
    {
        StopAllCoroutines();

        if (block == null) block = new MaterialPropertyBlock();
        if (rendererRef == null) rendererRef = GetComponent<Renderer>();

        block.SetColor(ColorID, originalColor);
        rendererRef.SetPropertyBlock(block);
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < flashes; i++)
        {
            // Pone rojo
            block.SetColor(ColorID, flashColor);
            rendererRef.SetPropertyBlock(block);
            yield return new WaitForSeconds(flashTime);

            // Restaura color original
            block.SetColor(ColorID, originalColor);
            rendererRef.SetPropertyBlock(block);
            yield return new WaitForSeconds(flashTime);
        }
    }
}
