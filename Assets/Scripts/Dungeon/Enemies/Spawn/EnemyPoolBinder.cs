using UnityEngine;
using System;

[RequireComponent(typeof(EnemyBase))]
public class EnemyPoolBinder : MonoBehaviour
{
    private EnemySpawner spawner;
    private string enemyId;
    private EnemyBase enemy;

    public void Bind(EnemySpawner spawner, string enemyId, EnemyBase enemy)
    {
        Unbind();

        this.spawner = spawner;
        this.enemyId = enemyId;
        this.enemy = enemy;

        this.enemy.OnDeath += HandleDeath;
    }

    private void HandleDeath(EnemyBase e)
    {
        Unbind();
        spawner?.ReturnToPool(enemyId, enemy);
    }

    private void OnDisable()
    {
       Unbind();
    }

    private void OnDestroy()
    {
        Unbind();
    }

    private void Unbind()
    {
        if (enemy != null)
        {
            enemy.OnDeath -= HandleDeath;
        }
    }
}