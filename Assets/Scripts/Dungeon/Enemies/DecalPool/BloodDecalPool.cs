using System.Collections.Generic;
using UnityEngine;

public class BloodDecalPool : Singleton<BloodDecalPool>
{
    [SerializeField] private GameObject decalPrefab;
    [SerializeField] private int initialSize = 20;
    [SerializeField] private float decalLifetime = 5f;

    private readonly Queue<GameObject> pool = new();

    private void Awake()
    {
        CreateSingleton(false);

        // Prellenar el pool
        for (int i = 0; i < initialSize; i++)
        {
            var decal = CreateNew();
            pool.Enqueue(decal);
        }
    }

    private GameObject CreateNew()
    {
        var decal = Instantiate(decalPrefab, transform);
        decal.SetActive(false);
        return decal;
    }

    public void Spawn(Vector3 position, Quaternion rotation)
    {
        GameObject decal = pool.Count > 0 ? pool.Dequeue() : CreateNew();

        decal.transform.SetPositionAndRotation(position, rotation);
        decal.SetActive(true);
        StartCoroutine(ReturnAfterDelay(decal, decalLifetime));
    }

    private System.Collections.IEnumerator ReturnAfterDelay(GameObject decal, float delay)
    {
        yield return new WaitForSeconds(delay);
        decal.SetActive(false);
        pool.Enqueue(decal);
    }
}
