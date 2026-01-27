using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "PokerShot/Player Configuration", order = 2)]
public class PlayerConfig : ScriptableObject
{
    [Header("Movement")]
    [Tooltip("Base movement speed (m/s)")]
    public float moveSpeed = 5f;

    [Tooltip("Distance at which player stops near target")]
    public float stopDistance = 0.2f;

    [Tooltip("Drag for natural deceleration")]
    public float drag = 5f;

    [Header("Combat")]
    [Tooltip("Base max HP")]
    public int maxHealth = 100;

    [Tooltip("HP regen per second when out of combat")]
    public float healthRegenPerSec = 1f;

    [Header("Hybrid Control")]
    [Tooltip("Pixel threshold to activate follow-mode")]
    public float dragThresholdPx = 10f;

    [Tooltip("Speed multiplier in follow-mode")]
    public float followSpeedMultiplier = 1.2f;

    [Tooltip("Minimum distance to ignore self-target taps")]
    public float minTapDistance = 0.5f;
}