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
                SetupEnemy(enemy, info);  
                pools[prefab].Enqueue(enemy);
            }
        }
    }

    public GameObject GetEnemy(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("[EnemyPool] GetEnemy: prefab == null!");
            return null;
        }

        if (!pools.ContainsKey(prefab) || pools[prefab].Count == 0)
        {
            GameObject enemy = Instantiate(prefab);
            SetupEnemy(enemy, null);
            return enemy;
        }

        GameObject obj = pools[prefab].Dequeue();
        obj.SetActive(true);
        return obj;
    }
    public void ReturnEnemy(GameObject enemy)
    {
        if (enemy == null) return;

        enemy.SetActive(false);
        enemy.transform.position = new Vector3(1000, 1000, 1000);

        if (WaveTracker.Instance != null)
            WaveTracker.Instance.EnemyReturned(enemy);

        var refComp = enemy.GetComponent<OriginalPrefabReference>();
        if (refComp != null && refComp.prefab != null && pools.ContainsKey(refComp.prefab))
            pools[refComp.prefab].Enqueue(enemy);
        else
            Destroy(enemy);   
    }

    private void SetupEnemy(GameObject enemy, WaveConfig.EnemySpawnInfo info)
    {
        enemy.tag = "Enemy";
        enemy.layer = LayerMask.NameToLayer("Enemy");

        var health = enemy.GetComponent<Health>();
        if (health != null && info != null)
        {
            health.SetMaxHealth(15f);   
        }
    }
}