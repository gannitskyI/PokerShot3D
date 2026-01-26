using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] public float maxHealth = 100f;  // ? добавляем public
    [SerializeField] private float regenPerSecond = 1f;        // реген вне боя
    [SerializeField] private float regenDelayAfterDamage = 3f; // задержка после урона

    [Header("Events")]
    public UnityEvent<float> OnDamageTaken = new UnityEvent<float>();   // float = dmg amount
    public UnityEvent<float> OnHealed = new UnityEvent<float>();        // float = heal amount
    public UnityEvent OnDeath = new UnityEvent();

    private float currentHealth;
    private float lastDamageTime;
    private bool isDead;

   
    public float CurrentHealth => currentHealth;
    public float NormalizedHealth => currentHealth / maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    private void Update()
    {
        if (isDead) return;

        // Регенерация (только если прошло время после урона)
        if (Time.time - lastDamageTime > regenDelayAfterDamage)
        {
            currentHealth = Mathf.Min(currentHealth + regenPerSecond * Time.deltaTime, maxHealth);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        lastDamageTime = Time.time;

        OnDamageTaken.Invoke(amount);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealed.Invoke(amount);
    }

    private void Die()
    {
        isDead = true;
        currentHealth = 0f;
        OnDeath.Invoke();
        Debug.Log($"[Health] {gameObject.name} погиб");
        // Позже: Game Over экран, restart и т.д.
    }

    // Для отладки в инспекторе
    [ContextMenu("Take 50 dmg")]
    private void DebugTakeDamage() => TakeDamage(50f);

    [ContextMenu("Heal 30 HP")]
    private void DebugHeal() => Heal(30f);
}