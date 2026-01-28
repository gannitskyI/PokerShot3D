using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float playerSearchInterval = 0.5f;
    [SerializeField] private float minDistanceToPlayer = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;

    private Transform player;
    private Rigidbody rb;
    private float lastSearchTime;
    private bool hasValidTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.freezeRotation = true;
            rb.linearDamping = 0.5f;
        }
        else
        {
            Debug.LogError($"[EnemyMovement] {gameObject.name}: Rigidbody component missing!");
        }
    }

    private void OnEnable()
    {
        if (PlayerEvents.Instance != null)
        {
            PlayerEvents.Instance.OnPlayerSpawned.AddListener(OnPlayerSpawned);
        }

        FindPlayer();
        lastSearchTime = Time.time;
    }

    private void OnDisable()
    {
        if (PlayerEvents.Instance != null)
        {
            PlayerEvents.Instance.OnPlayerSpawned.RemoveListener(OnPlayerSpawned);
        }

        hasValidTarget = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void OnPlayerSpawned()
    {
        FindPlayer();
        Debug.Log($"[EnemyMovement] {gameObject.name}: Player spawn event received");
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
            hasValidTarget = true;
            Debug.Log($"[EnemyMovement] {gameObject.name}: Player found at {player.position}");
        }
        else
        {
            ChipMagnet chipMagnet = FindObjectOfType<ChipMagnet>();
            if (chipMagnet != null)
            {
                player = chipMagnet.transform;
                hasValidTarget = true;
                Debug.Log($"[EnemyMovement] {gameObject.name}: Player found via ChipMagnet");
            }
            else
            {
                player = null;
                hasValidTarget = false;
                Debug.LogWarning($"[EnemyMovement] {gameObject.name}: Player not found!");
            }
        }
    }

    private void FixedUpdate()
    {
        if (Time.time - lastSearchTime > playerSearchInterval)
        {
            lastSearchTime = Time.time;

            if (player == null || !player.gameObject.activeInHierarchy)
            {
                hasValidTarget = false;
                FindPlayer();
            }
        }

        if (!hasValidTarget || player == null)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
            return;
        }

        MoveTowardsPlayer();
    }

    private void MoveTowardsPlayer()
    {
        if (player == null || rb == null) return;

        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0;

        float distanceToPlayer = directionToPlayer.magnitude;
        if (distanceToPlayer < minDistanceToPlayer)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        directionToPlayer.Normalize();

        Vector3 targetVelocity = directionToPlayer * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;

        if (rotationSpeed > 0 && directionToPlayer.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0f, speed);
    }

    public Transform GetTarget()
    {
        return player;
    }

    public bool HasValidTarget()
    {
        return hasValidTarget && player != null;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        if (hasValidTarget && player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);

            if (rb != null && rb.linearVelocity.magnitude > 0.1f)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, rb.linearVelocity.normalized * 2f);
            }
        }
        else
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (moveSpeed < 0)
        {
            moveSpeed = 0;
            Debug.LogWarning($"[EnemyMovement] {gameObject.name}: Move speed cannot be negative");
        }

        if (rotationSpeed < 0)
        {
            rotationSpeed = 0;
            Debug.LogWarning($"[EnemyMovement] {gameObject.name}: Rotation speed cannot be negative");
        }

        if (playerSearchInterval < 0.1f)
        {
            playerSearchInterval = 0.1f;
            Debug.LogWarning($"[EnemyMovement] {gameObject.name}: Search interval too low");
        }

        if (minDistanceToPlayer < 0)
        {
            minDistanceToPlayer = 0;
            Debug.LogWarning($"[EnemyMovement] {gameObject.name}: Min distance cannot be negative");
        }
    }
#endif
}