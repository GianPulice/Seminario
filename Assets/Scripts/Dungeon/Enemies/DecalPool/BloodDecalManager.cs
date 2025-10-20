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
        if (factory == null || pooler == null) return;
        GameObject decalObj = CreateDecal();
        DecalComponent decalComp = decalObj.GetComponent<DecalComponent>();
        decalComp.transform.SetPositionAndRotation(position, rotation);
        decalComp.gameObject.SetActive(true);
        StartCoroutine(ReturnAfterDelay(decalComp, decalLifetime));

    }

    private GameObject CreateDecal()
    {
        string prefabName = pooler.Prefab.name;
        return factory.CreateObject(prefabName);
    }

    private System.Collections.IEnumerator ReturnAfterDelay(DecalComponent decal, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Aseguramos que el objeto no haya sido destruido en el interín
        if (decal != null && pooler != null)
        {
            decal.gameObject.SetActive(false);
            pooler.ReturnObjectToPool(decal);
        }
        else if (decal != null)
        {
            // Si el pooler es nulo o no se encontró, destruimos el objeto que la Factory creó.
            Debug.LogWarning("[BloodDecalManager] Pooler nulo. Destruyendo Decal creado por Factory para evitar fugas.");
            Destroy(decal.gameObject);
        }
    }
}
