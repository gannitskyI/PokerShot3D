using UnityEngine;
using System.Collections;
using UnityPhysics = UnityEngine.Physics;  // ? псевдоним для избежания конфликта

[RequireComponent(typeof(Health), typeof(AutoShooter))]
public class ComboEffects : MonoBehaviour
{
    private Health playerHealth;
    private AutoShooter autoShooter;

    private void Awake()
    {
        playerHealth = GetComponent<Health>();
        autoShooter = GetComponent<AutoShooter>();
    }

    public void ApplyCombo(PokerEvaluator.HandResult result)
    {
        switch (result.type)
        {
            case PokerEvaluator.HandType.Pair:
                StartCoroutine(ApplyDamageBoost(2f, 6f));
                break;

            case PokerEvaluator.HandType.ThreeOfAKind:
                AOEExplosion(6f, 40f);
                break;

            case PokerEvaluator.HandType.Flush:
                playerHealth.Heal(25f);
                break;

            case PokerEvaluator.HandType.RoyalFlush:
                NukeArena();
                break;

            default:
                break;
        }
    }

    private IEnumerator ApplyDamageBoost(float multiplier, float duration)
    {
        if (autoShooter == null) yield break;

        autoShooter.damageMultiplier *= multiplier;
        yield return new WaitForSeconds(duration);
        autoShooter.damageMultiplier /= multiplier;
    }

    private void AOEExplosion(float radius, float damage)
    {
        Collider[] hits = UnityPhysics.OverlapSphere(transform.position, radius, LayerMask.GetMask("Enemy"));
        foreach (var hit in hits)
        {
            var health = hit.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(damage);
        }

        Debug.Log($"[Combo] AOE-взрыв: {hits.Length} врагов получили {damage} dmg");
    }

    private void NukeArena()
    {
        var enemies = FindObjectsOfType<Health>();
        foreach (var e in enemies)
        {
            if (e.gameObject.CompareTag("Enemy") && !e.CompareTag("Boss"))
                e.TakeDamage(9999f);
        }
        Debug.Log("[Combo] Роял-флеш — НЮК АРЕНЫ!");
    }
}