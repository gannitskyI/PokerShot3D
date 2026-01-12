using UnityEngine;
using System;

[CreateAssetMenu(fileName = "WaveConfig", menuName = "PokerShot/Wave Configuration", order = 1)]
public class WaveConfig : ScriptableObject
{
    [Header("Wave Identification")]
    [Tooltip("Номер волны (для отображения в UI или логике)")]
    public int waveNumber = 1;

    [Header("Enemy Spawning")]
    [Tooltip("Сколько всего врагов за волну")]
    public int totalEnemies = 5;

    [Tooltip("Максимальное количество врагов на арене одновременно")]
    public int maxConcurrentEnemies = 10;

    [Tooltip("Время между спавнами (сек)")]
    public float spawnInterval = 2.5f;

    [Tooltip("Время на всю волну до автоматического завершения (если не убиты все)")]
    public float maxWaveDuration = 60f;

    [Header("Enemy Composition")]
    [SerializeField] private EnemySpawnInfo[] enemySpawnInfos;

    [Serializable]
    public class EnemySpawnInfo
    {
        [Tooltip("Prefab врага (позже Addressable Asset Reference)")]
        public GameObject enemyPrefab;          // Пока prefab, потом ? AssetReference

        [Range(0f, 1f)]
        [Tooltip("Вес / вероятность спавна этого типа (сумма весов = 1)")]
        public float weight = 1f;

        [Tooltip("Минимальное количество этого типа за волну")]
        public int minCount = 0;

        [Tooltip("Максимальное количество этого типа за волну (0 = без лимита)")]
        public int maxCount = 0;
    }

    // Вспомогательные свойства (для читаемости)
    public EnemySpawnInfo[] EnemySpawnInfos => enemySpawnInfos;

    // Позже: метод для weighted random выбора типа врага
    public GameObject GetRandomEnemyType()
    {
        // Weighted random logic (реализуем в WaveSpawner)
        return null; // placeholder
    }
}