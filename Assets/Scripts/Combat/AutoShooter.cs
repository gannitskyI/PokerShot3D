using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(TapToMove))]
public class AutoShooter : MonoBehaviour
{
    [SerializeField] private WeaponConfig config;
    [SerializeField] private Transform[] muzzlePoints; // 3–4 точки вокруг

    public float damageMultiplier = 1f;
    public float fireRateMultiplier = 1f;

    private float baseFireRate;
    private float fireTimer;
    private List<Transform> enemiesInRange = new List<Transform>();

    private LineRenderer[] lineRenderers; // для визуала попаданий

    private void Awake()
    {
        if (config == null) Debug.LogWarning("[AutoShooter] Нет WeaponConfig!");
        if (muzzlePoints == null || muzzlePoints.Length == 0)
            Debug.LogWarning("[AutoShooter] Нет muzzle points!");

        baseFireRate = config.fireRate;
        lineRenderers = new LineRenderer[config.maxTargets];
        for (int i = 0; i < config.maxTargets; i++)
        {
            GameObject lrGO = new GameObject("ShotLine" + i);
            lrGO.transform.SetParent(transform);
            var lr = lrGO.AddComponent<LineRenderer>();
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.red;
            lr.endColor = Color.yellow;
            lr.enabled = false;
            lineRenderers[i] = lr;
        }
    }

    private void Update()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer >= 1f / (config.fireRate * fireRateMultiplier))
        {
            ShootNearest();
            fireTimer = 0f;
        }
    }

    private void ShootNearest()
    {
        enemiesInRange.Clear();

        Collider[] hits = Physics.OverlapSphere(transform.position, config.range, LayerMask.GetMask("Enemy"));
        foreach (var hit in hits)
            enemiesInRange.Add(hit.transform);

        enemiesInRange.Sort((a, b) => Vector3.Distance(transform.position, a.position)
                                       .CompareTo(Vector3.Distance(transform.position, b.position)));

        int targetsHit = 0;
        for (int i = 0; i < enemiesInRange.Count && targetsHit < config.maxTargets; i++)
        {
            Transform target = enemiesInRange[i];
            if (target == null) continue;

            Vector3 dir = (target.position - transform.position).normalized;
            if (Physics.Raycast(transform.position, dir, out RaycastHit hitInfo, config.range))
            {
                if (hitInfo.transform == target)
                {
                    var health = target.GetComponent<EnemyHealth>();
                    if (health != null)
                    {
                        health.TakeDamage(config.damagePerShot * damageMultiplier);
                        ShowShotVisual(muzzlePoints[targetsHit % muzzlePoints.Length].position, hitInfo.point);
                        targetsHit++;
                    }
                }
            }
        }

        // Выключаем неиспользованные линии
        for (int i = targetsHit; i < lineRenderers.Length; i++)
            lineRenderers[i].enabled = false;
    }

    private void ShowShotVisual(Vector3 start, Vector3 end)
    {
        var lr = lineRenderers[Random.Range(0, lineRenderers.Length)]; // рандомный для разнообразия
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.enabled = true;

        // Авто-выключение через 0.1 сек
        Invoke(nameof(DisableLine), 0.1f);
    }

    private void DisableLine()
    {
        foreach (var lr in lineRenderers)
            lr.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, config.range);
    }
}