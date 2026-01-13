using UnityEngine;
using System;

[CreateAssetMenu(fileName = "WaveConfig", menuName = "PokerShot/Wave Configuration", order = 1)]
public class WaveConfig : ScriptableObject
{
    [Header("Wave Identification")]
    public int waveNumber = 1;

    [Header("Enemy Spawning")]
    public int totalEnemies = 5;
    public int maxConcurrentEnemies = 10;
    public float spawnInterval = 2.5f;
    public float maxWaveDuration = 60f;

    [Header("Enemy Composition")]
    [SerializeField] private EnemySpawnInfo[] enemySpawnInfos;

    [Serializable]
    public class EnemySpawnInfo
    {
        [Tooltip("Prefab врага")]
        public GameObject enemyPrefab;           // ? это поле уже есть

        [Range(0f, 1f)]
        public float weight = 1f;

        public int minCount = 0;
        public int maxCount = 0;
    }

    public EnemySpawnInfo[] EnemySpawnInfos => enemySpawnInfos;
}