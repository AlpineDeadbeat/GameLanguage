using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public Enemy enemyPrefab;
    public int maxAlive = 5;
    public float spawnInterval = 5f;
    public Vector2 spawnRadius = new Vector2(2f, 2f);
    public bool autoSpawn = true;

    private readonly List<Enemy> _alive = new();

    void OnEnable()
    {
        if (autoSpawn) InvokeRepeating(nameof(TrySpawn), spawnInterval, spawnInterval);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(TrySpawn));
    }

    void TrySpawn()
    {
        Cleanup();
        if (_alive.Count >= maxAlive || enemyPrefab == null) return;
        SpawnOne();
    }

    public void SpawnWave(int count)
    {
        Cleanup();
        for (int i = 0; i < count; i++)
        {
            if (_alive.Count >= maxAlive) break;
            SpawnOne();
        }
    }

    void SpawnOne()
    {
        Vector2 offset = new Vector2(
            Random.Range(-spawnRadius.x, spawnRadius.x),
            Random.Range(-spawnRadius.y, spawnRadius.y)
        );
        var e = Instantiate(enemyPrefab, transform.position + (Vector3)offset, Quaternion.identity);
        _alive.Add(e);
    }

    void Cleanup()
    {
        _alive.RemoveAll(e => e == null);
    }
}