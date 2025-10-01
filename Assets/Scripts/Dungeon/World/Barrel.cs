using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(DropHandler))]
public class Barrel : MonoBehaviour, IDamageable
{
    [Header("Properties")]
    [SerializeField] private int hitPoints = 1;
    [SerializeField] private GameObject barrelVFX;

    private DropHandler dropHandler;

    private void Awake()
    {
        dropHandler = GetComponent<DropHandler>();
    }

    public void TakeDamage(int value)
    {
        Debug.Log("Hice daño");
        TakeHit(value);
    }

    public void TakeHit(int value)
    {
        hitPoints -= value;
        if(AudioManager.Instance != null) AudioManager.Instance.PlaySFX("BarrelHit");
        if (hitPoints <= 0)
        {
            BreakBarrel();
        }
    }

    private void BreakBarrel()
    {
        Debug.Log("Barrel is breaking!");
        if(barrelVFX != null)
        {
            var vfx =Instantiate(barrelVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 0.5f);
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("BarrelDestroy");
        
        int currentLayer = DungeonManager.Instance.CurrentLayer;
        dropHandler.DropLoot(currentLayer);

        Destroy(gameObject);
    }
}
