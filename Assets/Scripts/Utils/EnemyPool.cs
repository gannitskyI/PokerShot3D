using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        Instance = this;
    }

    public async void PreloadForWave(WaveConfig wave)
    {
        foreach (var info in wave.EnemySpawnInfos)
        {
            GameObject prefab = info.enemyPrefab;
            if (prefab == null) continue;

            if (!pools.ContainsKey(prefab))
            {
                pools[prefab] = new Queue<GameObject>();
            }

            int preloadCount = info.maxCount > 0 ? info.maxCount * 2 : 20;

            for (int i = 0; i < preloadCount; i++)
            {
                GameObject enemy = Instantiate(prefab);
                enemy.SetActive(false);
                SetupEnemy(enemy, info);  // передаём info вместо config
                pools[prefab].Enqueue(enemy);
            }
        }
    }

    public GameObject GetEnemy(GameObject prefab)
    {
        Debug.Log($"[EnemyPool] Запрос врага: {prefab?.name ?? "null prefab"}");
        if (prefab == null)
        {
            Debug.LogError("[EnemyPool] prefab == null в GetEnemy");
            return null;
        }

        if (!pools.ContainsKey(prefab) || pools[prefab].Count == 0)
        {
            GameObject enemy = Instantiate(prefab);
            SetupEnemy(enemy, null); // временно, потом передавать info
            return enemy;
        }

        GameObject obj = pools[prefab].Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void ReturnEnemy(GameObject enemy, GameObject prefabKey)
    {
        if (enemy == null) return;
        enemy.SetActive(false);
        enemy.transform.position = Vector3.zero;
        if (pools.ContainsKey(prefabKey))
            pools[prefabKey].Enqueue(enemy);
    }

    private void SetupEnemy(GameObject enemy, WaveConfig.EnemySpawnInfo info)
    {
        enemy.tag = "Enemy";
        enemy.layer = LayerMask.NameToLayer("Enemy");

        var health = enemy.GetComponent<EnemyHealth>();
        if (health && info != null)
        {
            // Пока хардкод, потом будет EnemyConfig.maxHp
            health.SetMaxHp(15f); // ? добавим setter
        }
    }
}