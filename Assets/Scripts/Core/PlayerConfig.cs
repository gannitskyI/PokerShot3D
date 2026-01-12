using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "PokerShot/Player Configuration", order = 2)]
public class PlayerConfig : ScriptableObject
{
    [Header("Movement")]
    [Tooltip("Базовая скорость (м/с)")]
    public float moveSpeed = 5f;

    [Tooltip("Расстояние, на котором игрок останавливается у цели")]
    public float stopDistance = 0.2f;

    [Tooltip("Drag для естественного торможения")]
    public float drag = 5f;

    [Header("Combat (позже апгрейды)")]
    [Tooltip("Базовый HP")]
    public int maxHealth = 100;

    [Tooltip("Реген HP/сек вне боя")]
    public float healthRegenPerSec = 1f;

    [Header("Hybrid Control")]
    [Tooltip("Порог сдвига (px) для активации follow-mode")]
    public float dragThresholdPx = 10f;

    [Tooltip("Множитель скорости в follow-mode (быстрее обычного)")]
    public float followSpeedMultiplier = 1.2f;

    [Header("Hybrid Control")]
    [Tooltip("Минимальное расстояние для тап-цели (игнорируем self-target)")]
    public float minTapDistance = 0.5f;
}