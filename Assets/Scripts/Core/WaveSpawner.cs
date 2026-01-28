using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    private WaveConfig currentWave;
    private int spawned = 0;
    private int concurrent = 0;
    private Coroutine spawnRoutine;
    private bool isWaveActive = false;

    private void OnDestroy()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    public void StartWave(WaveConfig wave)
    {
        if (wave == null)
        {
            Debug.LogError("[WaveSpawner] Cannot start wave with null config!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[WaveSpawner] No spawn points assigned!");
            return;
        }

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        currentWave = wave;
        spawned = 0;
        concurrent = 0;
        isWaveActive = true;

        if (WaveTracker.Instance != null)
        {
            WaveTracker.Instance.StartWave();
        }
        else
        {
            Debug.LogWarning("[WaveTracker] WaveTracker not found - wave completion may not work!");
        }

        if (EnemyPool.Instance != null)
        {
            EnemyPool.Instance.PreloadForWave(wave);
        }
        else
        {
            Debug.LogWarning("[WaveSpawner] EnemyPool.Instance не найден!");
        }

        spawnRoutine = StartCoroutine(SpawnWaveRoutine());

        Debug.Log($"[WaveSpawner] Started wave {wave.waveNumber} - Total enemies: {wave.totalEnemies}");
    }

    private IEnumerator SpawnWaveRoutine()
    {
        if (currentWave == null)
        {
            Debug.LogError("[WaveSpawner] Wave config is null in spawn routine!");
            yield break;
        }

        while (spawned < currentWave.totalEnemies && isWaveActive)
        {
            if (concurrent < currentWave.maxConcurrentEnemies)
            {
                if (SpawnOneEnemy())
                {
                    
                }
                else
                {
                    Debug.LogWarning("[WaveSpawner] Failed to spawn enemy, will retry next interval");
                }
            }

            yield return new WaitForSeconds(currentWave.spawnInterval);
        }

        Debug.Log($"[WaveSpawner] Finished spawning {spawned} enemies");
    }

    private void Update()
    {
        if (!isWaveActive) return;

        if (WaveTracker.Instance != null)
        {
            int liveEnemies = WaveTracker.Instance.LiveEnemies;

            concurrent = liveEnemies;

            if (spawned >= currentWave.totalEnemies && liveEnemies == 0)
            {
                CompleteWave();
            }
        }
        else
        {
            Debug.LogWarning("[WaveSpawner] WaveTracker instance lost during wave!");
        }
    }

    private void CompleteWave()
    {
        if (!isWaveActive) return;

        isWaveActive = false;

        if (RunStateController.Instance != null)
        {
            RunStateController.Instance.EndWave();
        }
        else
        {
            Debug.LogError("[WaveSpawner] RunStateController not found - cannot end wave!");
        }

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        currentWave = null;
        spawned = 0;
        concurrent = 0;

        Debug.Log("[WaveSpawner] Wave completed");
    }

    private bool SpawnOneEnemy()
    {
        if (EnemyPool.Instance == null)
        {
            Debug.LogError("[WaveSpawner] EnemyPool.Instance null!");
            return false;
        }

        if (currentWave == null || currentWave.EnemySpawnInfos == null || currentWave.EnemySpawnInfos.Length == 0)
        {
            Debug.LogError("[WaveSpawner] Invalid wave config!");
            return false;
        }

        float totalWeight = 0f;
        foreach (var info in currentWave.EnemySpawnInfos)
        {
            if (info != null && info.enemyPrefab != null)
                totalWeight += info.weight;
        }

        if (totalWeight <= 0f)
        {
            Debug.LogError("[WaveSpawner] Total weight 0!");
            return false;
        }

        float rand = Random.Range(0f, totalWeight);
        float current = 0f;
        WaveConfig.EnemySpawnInfo selectedInfo = null;

        foreach (var info in currentWave.EnemySpawnInfos)
        {
            if (info == null || info.enemyPrefab == null) continue;

            current += info.weight;
            if (rand <= current)
            {
                selectedInfo = info;
                break;
            }
        }

        if (selectedInfo == null)
            selectedInfo = currentWave.EnemySpawnInfos[0];

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject enemy = EnemyPool.Instance.GetEnemy(selectedInfo.enemyPrefab);
        if (enemy == null)
        {
            Debug.LogError("[WaveSpawner] Failed to get enemy from pool!");
            return false;
        }

        enemy.transform.position = spawnPoint.position;
        enemy.transform.rotation = spawnPoint.rotation;
        enemy.SetActive(true);

        var health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
            health.ResetHealth();

        concurrent++;
        spawned++;

        if (WaveTracker.Instance != null)
            WaveTracker.Instance.EnemySpawned(enemy);

        Debug.Log($"[WaveSpawner] Spawned {enemy.name} at {spawnPoint.position}");

        return true;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[WaveSpawner] No spawn points, using origin");
            return new Vector3(0, 0.5f, 0);
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 pos = spawnPoint.position;
        pos.y = 0.5f;

        return pos;
    }

    private WaveConfig.EnemySpawnInfo GetWeightedEnemyInfo(WaveConfig wave)
    {
        if (wave == null || wave.EnemySpawnInfos == null || wave.EnemySpawnInfos.Length == 0)
        {
            Debug.LogError("[WaveSpawner] Invalid wave config for weighted selection!");
            return null;
        }

        float totalWeight = 0f;
        foreach (var info in wave.EnemySpawnInfos)
        {
            if (info != null && info.enemyPrefab != null)
            {
                totalWeight += info.weight;
            }
        }

        if (totalWeight <= 0f)
        {
            Debug.LogError("[WaveSpawner] Total weight is 0, cannot select enemy!");
            return wave.EnemySpawnInfos[0];
        }

        float rand = Random.Range(0f, totalWeight);
        float current = 0f;

        foreach (var info in wave.EnemySpawnInfos)
        {
            if (info == null || info.enemyPrefab == null)
            {
                continue;
            }

            current += info.weight;
            if (rand <= current)
            {
                return info;
            }
        }

        Debug.LogWarning("[WaveSpawner] Weighted selection fallback!");
        return wave.EnemySpawnInfos[0];
    }

    public void StopWave()
    {
        if (!isWaveActive) return;

        isWaveActive = false;

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        currentWave = null;
        spawned = 0;
        concurrent = 0;

        Debug.Log("[WaveSpawner] Wave stopped");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[WaveSpawner] No spawn points assigned!");
        }
        else
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] == null)
                {
                    Debug.LogWarning($"[WaveSpawner] Spawn point {i} is null!");
                }
            }
        }
    }
#endif
}