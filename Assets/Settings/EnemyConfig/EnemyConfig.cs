using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "GroundEnemy", menuName = "PokerShot/Enemy Configuration", order = 4)]
public class EnemyConfig : ScriptableObject
{
    public string displayName = "Grunt";
    public AssetReferenceT<GameObject> prefabReference;   
    public float maxHp = 15f;
    public float moveSpeed = 3f;
    public int dropChipsMin = 1;
    public int dropChipsMax = 1;
}