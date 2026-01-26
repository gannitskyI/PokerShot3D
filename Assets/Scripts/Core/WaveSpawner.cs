using UnityEngine;
using System.Collections;
using static RunStateController;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints; // точки по краю арены (8–12 GO)

    // Поля для состояния волны
    private WaveConfig currentWave;
    private int spawned = 0;
    private int concurrent = 0;
    private bool isWaveActive = false;  // ? добавили поле
    private Coroutine spawnRoutine;

    public void StartWave(WaveConfig wave)
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);

        currentWave = wave;
        spawned = 0;
        concurrent = 0;
        isWaveActive = true;  // ? волна активна

        spawnRoutine = StartCoroutine(SpawnWaveRoutine());

        if (WaveTracker.Instance != null)
            WaveTracker.Instance.StartWave();
    }

    private IEnumerator SpawnWaveRoutine()
    {
        while (spawned < currentWave.totalEnemies)
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
        if (isWaveActive && WaveTracker.Instance != null && WaveTracker.Instance.ConcurrentEnemies == 0 && spawned >= currentWave.totalEnemies)
        {
            RunStateController.Instance.EndWave();
            isWaveActive = false;
            currentWave = null;
        }
    }

    private void SpawnOneEnemy()
    {
        var chosenInfo = GetWeightedEnemyInfo(currentWave);
        if (chosenInfo == null || chosenInfo.enemyPrefab == null)
        {
            Debug.LogError("[WaveSpawner] Нет префаба в выбранном info");
            return;
        }

        Vector3 pos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        pos.y = 0.5f;

        GameObject enemy = EnemyPool.Instance.GetEnemy(chosenInfo.enemyPrefab);
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