using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints; // точки по краю арены (создай 8–12 пустых GO)

    private Coroutine spawnRoutine;

    private void OnEnable()
    {
        if (RunStateController.Instance != null)
        {
            // Подписка на событие (пока без событий — вызываем вручную)
        }
    }

    public void StartWave(WaveConfig wave)
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnWaveRoutine(wave));
    }

    private IEnumerator SpawnWaveRoutine(WaveConfig wave)
    {
        int spawned = 0;
        int concurrent = 0;

        while (spawned < wave.totalEnemies)
        {
            if (concurrent < wave.maxConcurrentEnemies)
            {
                SpawnOneEnemy(wave);
                spawned++;
                concurrent++;
            }

            yield return new WaitForSeconds(wave.spawnInterval);
        }

        // Ждём пока все умрут (позже через событие OnEnemyDied)
        yield return new WaitUntil(() => concurrent <= 0);
        RunStateController.Instance.EndWave();
    }

    private void SpawnOneEnemy(WaveConfig wave)
    {
        var chosenInfo = GetWeightedEnemyInfo(wave);
        if (chosenInfo == null || chosenInfo.enemyPrefab == null)
        {
            Debug.LogError("[WaveSpawner] Нет префаба в выбранном info");
            return;
        }

        Debug.Log($"[WaveSpawner] Выбран префаб: {chosenInfo.enemyPrefab.name}");

        Vector3 pos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        pos.y = 0.5f;

        GameObject enemy = EnemyPool.Instance.GetEnemy(chosenInfo.enemyPrefab);
        enemy.transform.position = pos;

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