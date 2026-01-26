using UnityEngine;

[CreateAssetMenu(fileName = "BasicPistol", menuName = "PokerShot/Weapon Configuration", order = 3)]
public class WeaponConfig : ScriptableObject
{
    [Header("Base Stats")]
    public float fireRate = 8f;              // выстрелов в секунду
    public float damagePerShot = 2f;
    public int maxTargets = 3;
    public float range = 15f;

    [Header("Visuals")]
    public ParticleSystem muzzleFlashPrefab; // later
    public LayerMask enemyLayer;

    [Header("Upgrade Slots (позже)")]
    public float damageMultiplier = 1f;
    public float fireRateMultiplier = 1f;

     
        [Tooltip("Конфиг врага (SO с Addressable ссылкой)")]
        public EnemyConfig enemyConfig;

        [Range(0f, 1f)]
        public float weight = 1f;

        public int minCount = 0;
        public int maxCount = 0;
     
}