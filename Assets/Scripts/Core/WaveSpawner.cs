using UnityEngine;
using System.Collections;
using static RunStateController;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    private WaveConfig currentWave;
    private int spawned = 0;
    private int concurrent = 0;
    private Coroutine spawnRoutine;
    private bool isWaveActive = false;

    public void StartWave(WaveConfig wave)
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);

        currentWave = wave;
        spawned = 0;
        concurrent = 0;
        isWaveActive = true;

        spawnRoutine = StartCoroutine(SpawnWaveRoutine());

        if (WaveTracker.Instance != null)
            WaveTracker.Instance.StartWave();
    }

    private IEnumerator SpawnWaveRoutine()
    {
        if (currentWave == null) yield break;

        while (spawned < currentWave.totalEnemies && isWaveActive)
        {
            if (concurrent < currentWave.maxConcurrentEnemies)
            {
                SpawnOneEnemy();
                spawned++;
                concurrent++;
            }

            yield return new WaitForSeconds(currentWave.spawnInterval);
        }
    }

    private void Update()
    {
        if (!isWaveActive) return;

        if (WaveTracker.Instance != null && WaveTracker.Instance.LiveEnemies == 0 && spawned >= currentWave.totalEnemies)
        {
            RunStateController.Instance.EndWave();
            isWaveActive = false;
            currentWave = null;
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }
        }
    }

    private void SpawnOneEnemy()
    {
        if (currentWave == null) return;

        var chosenInfo = GetWeightedEnemyInfo(currentWave);
        if (chosenInfo == null || chosenInfo.enemyPrefab == null)
        {
            Debug.LogError("[WaveSpawner] Not prefab");
            return;
        }

        Vector3 pos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        pos.y = 0.5f;

        GameObject enemy = EnemyPool.Instance.GetEnemy(chosenInfo.enemyPrefab);
        if (enemy == null) return;

        enemy.transform.position = pos;

        if (WaveTracker.Instance != null)
            WaveTracker.Instance.EnemySpawned(enemy);

        Debug.Log($"Spawned {chosenInfo.enemyPrefab.name} at {pos}");
    }

    private WaveConfig.EnemySpawnInfo GetWeightedEnemyInfo(WaveConfig wave)
    {
        float totalWeight = 0f;
        foreach (var info in wave.EnemySpawnInfos) totalWeight += info.weight;

        float rand = Random.Range(0f, totalWeight);
        float current = 0f;

        foreach (var info in wave.EnemySpawnInfos)
        {
            current += info.weight;
            if (rand <= current) return info;
        }
        return null;
    }
}