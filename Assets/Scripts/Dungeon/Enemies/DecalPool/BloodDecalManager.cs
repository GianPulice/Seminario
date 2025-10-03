using System.Collections.Generic;
using UnityEngine;

public class BloodDecalManager : Singleton<BloodDecalManager>
{
    [SerializeField] private AbstractFactory factory;
    [SerializeField] private ObjectPooler pooler;
    [SerializeField] private float decalLifetime = 5f;

    private void Awake()
    {
        CreateSingleton(false);
    }

    public void Spawn(Vector3 position, Quaternion rotation)
    {
        MonoBehaviour decalMB = pooler.GetObjectFromPool<MonoBehaviour>();

        if (decalMB == null)
        {
            Debug.LogWarning($"[BloodDecalPool] Pool vacío o el objeto no tiene MonoBehaviour.");
            return;
        }
        
        decalMB.gameObject.transform.SetPositionAndRotation(position, rotation);
        decalMB.gameObject.SetActive(true);

        StartCoroutine(ReturnAfterDelay(decalMB, decalLifetime));
    }

    private System.Collections.IEnumerator ReturnAfterDelay(MonoBehaviour decal, float delay)
    {
        yield return new WaitForSeconds(delay);

        // devolver al pooler
        pooler.ReturnObjectToPool(decal);
    }
}
