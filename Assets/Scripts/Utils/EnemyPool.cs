using UnityEngine;
using System.Collections.Generic;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();
    private HashSet<GameObject> allPooledObjects = new HashSet<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[EnemyPool] Duplicate instance destroyed");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        foreach (var obj in allPooledObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        allPooledObjects.Clear();
        pools.Clear();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void PreloadForWave(WaveConfig wave)
    {
        if (wave == null)
        {
            Debug.LogWarning("[EnemyPool] PreloadForWave called with null wave config");
            return;
        }

        foreach (var info in wave.EnemySpawnInfos)
        {
            GameObject prefab = info.enemyPrefab;
            if (prefab == null)
            {
                Debug.LogWarning("[EnemyPool] Skipping null enemy prefab in wave config");
                continue;
            }

            if (!pools.ContainsKey(prefab))
            {
                pools[prefab] = new Queue<GameObject>();
            }

            int preloadCount = info.maxCount > 0 ? info.maxCount * 2 : 20;
            int currentPoolSize = pools[prefab].Count;
            int toCreate = Mathf.Max(0, preloadCount - currentPoolSize);

            for (int i = 0; i < toCreate; i++)
            {
                GameObject enemy = CreateEnemy(prefab, info);
                if (enemy != null)
                {
                    enemy.SetActive(false);
                    pools[prefab].Enqueue(enemy);
                }
            }

            Debug.Log($"[EnemyPool] Preloaded {toCreate} enemies of type {prefab.name}. Pool size: {pools[prefab].Count}");
        }
    }

    public GameObject GetEnemy(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("[EnemyPool] GetEnemy called with null prefab!");
            return null;
        }

        GameObject enemy;
        if (pools.ContainsKey(prefab) && pools[prefab].Count > 0)
        {
            enemy = pools[prefab].Dequeue();
        }
        else
        {
            enemy = CreateEnemy(prefab, null);
        }

        if (enemy != null)
        {
            ResetEnemy(enemy);
            enemy.SetActive(true);
        }

        return enemy;
    }

    public void ReturnEnemy(GameObject enemy)
    {
        if (enemy == null)
        {
            return;
        }

        enemy.SetActive(false);
        enemy.transform.position = new Vector3(1000, 1000, 1000);

        if (WaveTracker.Instance != null)
        {
            WaveTracker.Instance.EnemyReturned(enemy);
        }

        CleanupEnemy(enemy);

        var refComp = enemy.GetComponent<OriginalPrefabReference>();
        if (refComp != null && refComp.prefab != null)
        {
            if (pools.ContainsKey(refComp.prefab))
            {
                pools[refComp.prefab].Enqueue(enemy);
            }
            else
            {
                Debug.LogWarning($"[EnemyPool] Pool not found for prefab {refComp.prefab.name}. Creating new pool.");
                pools[refComp.prefab] = new Queue<GameObject>();
                pools[refComp.prefab].Enqueue(enemy);
            }
        }
        else
        {
            Debug.LogWarning($"[EnemyPool] Enemy {enemy.name} has no prefab reference. Destroying instead of pooling.");
            allPooledObjects.Remove(enemy);
            Destroy(enemy);
        }
    }

    private GameObject CreateEnemy(GameObject prefab, WaveConfig.EnemySpawnInfo info)
    {
        if (prefab == null)
        {
            return null;
        }

        GameObject enemy = Instantiate(prefab);

        allPooledObjects.Add(enemy);

        var refComp = enemy.GetComponent<OriginalPrefabReference>();
        if (refComp == null)
        {
            refComp = enemy.AddComponent<OriginalPrefabReference>();
        }
        refComp.prefab = prefab;

        SetupEnemyBase(enemy, info);

        return enemy;
    }

    private void SetupEnemyBase(GameObject enemy, WaveConfig.EnemySpawnInfo info)
    {
        enemy.tag = "Enemy";

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0)
        {
            enemy.layer = enemyLayer;
        }

        var health = enemy.GetComponent<Health>();
        if (health != null && info != null)
        {
            health.SetMaxHealth(15f);
        }
    }

    private void ResetEnemy(GameObject enemy)
    {
        if (enemy == null) return;

        var health = enemy.GetComponent<Health>();
        if (health != null)
        {
            health.ResetHealth();
        }

        var rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        enemy.transform.rotation = Quaternion.identity;
    }

    private void CleanupEnemy(GameObject enemy)
    {
        if (enemy == null) return;

        var rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public int GetPoolSize(GameObject prefab)
    {
        if (prefab == null || !pools.ContainsKey(prefab))
        {
            return 0;
        }

        return pools[prefab].Count;
    }

    public void ClearAllPools()
    {
        foreach (var obj in allPooledObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        allPooledObjects.Clear();
        pools.Clear();

        Debug.Log("[EnemyPool] All pools cleared");
    }
}