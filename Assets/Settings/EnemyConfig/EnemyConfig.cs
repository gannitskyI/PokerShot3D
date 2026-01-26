using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "GroundEnemy", menuName = "PokerShot/Enemy Configuration", order = 4)]
public class EnemyConfig : ScriptableObject
{
    public string displayName = "Грунт";
    public AssetReferenceT<GameObject> prefabReference;   // Addressable ссылка на prefab
    public float maxHp = 15f;
    public float moveSpeed = 3f;
    public int dropChipsMin = 1;
    public int dropChipsMax = 1;
}