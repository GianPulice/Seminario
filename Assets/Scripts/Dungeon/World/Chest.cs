using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(DropHandler))]
public class Chest : MonoBehaviour, IInteractable
{
    [Header("CONFIGURACIÓN REQUERIDA")]
    [Header("LOOT & SISTEMA DE RECOMPENSAS")]
    [Tooltip("Punto de spawn donde aparecerán los items del cofre")]
    [SerializeField] private Transform spawnPoint;

    [Header("ANIMACIÓN & EFECTOS VISUALES")]
    [Tooltip("Animator que controla la animación de apertura/cierre del cofre")]
    [SerializeField] private Animator animator;

    [Tooltip("Sistema de partículas que se activa al abrir el cofre (prefab para instanciar)")]
    [SerializeField] private GameObject fxOpen;

    [Header("COMPORTAMIENTO")]
    [Tooltip("Si se marca, el cofre se destruirá después de ser abierto")]
    [SerializeField] private bool destroyAfterOpen = false;

    [Tooltip("Tiempo de espera antes de destruir el cofre (si destroyAfterOpen está activado)")]
    [SerializeField] private float waitTimeBeforeDestroy = 2f;

    private bool opened;
    private Collider col;
    private DropHandler dropHandler;

    public InteractionMode InteractionMode => InteractionMode.Press;

    private void Awake()
    {
        GetComponents();
        StartCoroutine(RegisterOutline());
    }
    private void OnDestroy()
    {
        if (OutlineManager.Exists)
        {
            OutlineManager.Instance.Unregister(gameObject);
        }
    }
    public void Interact(bool isPressed)
    {
        if (opened) return;
        StartCoroutine(CO_OpenChest());
    }

    public void ShowOutline()
    {
        if (!opened)
        {

            OutlineManager.Instance.ShowWithCustomColor(gameObject, Color.yellow);
            InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Interactive);
        }
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
    private IEnumerator CO_OpenChest()
    {
        opened = true;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayOneShotSFX("ChestOpen");
        if (fxOpen != null)
        {
            var vfx = Instantiate(fxOpen, spawnPoint.position, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        if (animator != null)
            animator.SetBool("Open", true);
        //Espero a que termine la animación
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        SpawnLoot();

        if (destroyAfterOpen)
        {
            col.enabled = false;
            Destroy(gameObject, waitTimeBeforeDestroy);
        }
    }

    private void SpawnLoot()
    {
        int currentLayer = DungeonManager.Instance.CurrentLayer;
        dropHandler.DropLoot(currentLayer);
    }

    private void GetComponents()
    {
        col = GetComponent<Collider>();
        dropHandler = GetComponent<DropHandler>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

    }
    private IEnumerator RegisterOutline()
    {
        yield return new WaitUntil(() => OutlineManager.Exists);

        OutlineManager.Instance.Register(gameObject);
    }
}